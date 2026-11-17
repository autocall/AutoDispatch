using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Канал App (V1 DispatchApiController, протокол — AI/wiki/protocol.md): один POST Request на все методы IDispatchService
///     по EventRequest.Name, Response — long-polling событий компании, *Zip — то же с gzip. Тело — JSON с $type (DispatchJson),
///     читается из потока: V1-клиент шлёт его с Content-Type x-www-form-urlencoded. Ответ всегда 200 с EventResponse,
///     ошибка — Status Failed + ErrorMessage, как в V1. UserId в Data сверяется с токеном; статус пользователя (IsDeleted)
///     проверяется на каждом запросе — JWT живёт долго (решено 2026-11-13) </summary>
[Route("Api/Dispatch/[action]")]
public class ApiDispatchController : ApiBaseController {

    private const string JsonContentType = "application/json";
    private const string BinaryContentType = "application/octet-stream";

    private readonly DispatchPollRegistry _polls;

    private DispatchService Service => Host.GetService<DispatchService>();

    public ApiDispatchController(ServicesHost host, DispatchPollRegistry polls) : base(host) {
        this._polls = polls;
    }

    [HttpPost, DisableRequestSizeLimit]
    public async Task<ActionResult> Request() {
        var wrapper = DispatchJson.Deserialize<EventRequest>(await ReadBodyAsync());
        var response = await InvokeAsync(wrapper);
        return Content(DispatchJson.Serialize(response), JsonContentType);
    }

    [HttpPost, DisableRequestSizeLimit]
    public async Task<ActionResult> RequestZip() {
        var wrapper = ZipExtension.DeserializeObject<EventRequest>(await ReadBinaryAsync());
        var response = await InvokeAsync(wrapper);
        return File(ZipExtension.SerializeObject(response), BinaryContentType);
    }

    /// <summary>
    ///     Long-polling: ждёт события компании новее Data.EventId (DispatchService.EventNotificationAsync) </summary>
    [HttpPost]
    public async Task<ActionResult> Response() {
        var wrapper = DispatchJson.Deserialize<EventRequest>(await ReadBodyAsync());
        var response = await WaitEventsAsync(wrapper);
        return Content(DispatchJson.Serialize(response), JsonContentType);
    }

    [HttpPost]
    public async Task<ActionResult> ResponseZip() {
        var wrapper = ZipExtension.DeserializeObject<EventRequest>(await ReadBinaryAsync());
        var response = await WaitEventsAsync(wrapper);
        return File(ZipExtension.SerializeObject(response), BinaryContentType);
    }

    #region [ private ]

    private async Task<EventResponse> InvokeAsync(EventRequest wrapper) {
        try {
            var userId = GetUserId();
            if (wrapper?.Data == null || wrapper.Data.UserId != userId) {
                throw new AccessDeniedException();
            }
            if (DispatchRequestHandlers.All.TryGetValue(wrapper.Name ?? "", out var handler) == false) {
                throw new InfoException($"Unknown request: {wrapper.Name}");
            }
            if (handler.RequestType.IsInstanceOfType(wrapper.Data) == false) {
                throw new InfoException($"Request {wrapper.Name} expects {handler.RequestType.Name}");
            }
            await CheckUserAsync(userId);
            if (handler.HasAccess(GetRoles()) == false) {
                throw new AccessDeniedException();
            }
            var data = await handler.Invoke(Service, wrapper.Data);
            return new EventResponse { Status = ResponseStatus.Success, Data = data };
        } catch (Exception e) {
            e = e.GetBaseException();
            if (e is InfoException == false) {
                _l.e($"Request {wrapper?.Name}: UserId {wrapper?.Data?.UserId}", e);
            }
            return new EventResponse { Status = ResponseStatus.Failed, ErrorMessage = e.Message };
        }
    }

    private async Task<EventResponse> WaitEventsAsync(EventRequest wrapper) {
        var userId = GetUserId();
        int lastEventId = 0;
        var cts = _polls.Begin(userId, HttpContext.RequestAborted);
        try {
            if (wrapper?.Data is EventNotificationRequestData data == false || data.UserId != userId) {
                throw new AccessDeniedException();
            }
            lastEventId = data.EventId ?? 0;
            await CheckUserAsync(userId);
            var responseData = await Service.EventNotificationAsync(data, cts);
            lastEventId = responseData.Events.Any() ? responseData.Events.Max(r => r.EventId) : lastEventId;
            return new EventResponse { Status = ResponseStatus.Success, Data = responseData, EventId = lastEventId };
        } catch (Exception e) {
            // отмена новым запросом того же пользователя — штатно, как в V1 (Failed без записи в лог)
            if (e is OperationCanceledException == false) {
                _l.e($"Response: UserId {userId}", e);
            }
            return new EventResponse { Status = ResponseStatus.Failed, ErrorMessage = e.GetBaseException().Message, EventId = lastEventId };
        } finally {
            _polls.End(userId, cts);
        }
    }

    /// <summary>
    ///     V1 проверял VerifyId на каждом запросе; в V3 статус = IsDeleted (Delete в /Admin/Users) </summary>
    private async Task CheckUserAsync(Guid userId) {
        var user = await Host.UserManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted) {
            throw new AccessDeniedException();
        }
    }

    private List<RoleEnum> GetRoles() {
        return base.User.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => Enum.TryParse<RoleEnum>(c.Value, out var role) ? role : RoleEnum.Unknown)
            .Where(r => r != RoleEnum.Unknown)
            .ToList();
    }

    private async Task<string> ReadBodyAsync() {
        using var reader = new StreamReader(base.Request.Body);
        return await reader.ReadToEndAsync();
    }

    private async Task<byte[]> ReadBinaryAsync() {
        using var memory = new MemoryStream();
        await base.Request.Body.CopyToAsync(memory);
        return memory.ToArray();
    }

    #endregion [ private ]
}
