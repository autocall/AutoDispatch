using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Настройки App по секциям, копия AutoCall V3: чтение — все секции одним объектом, запись — по одной секции; JSON секции сервер
///     не разбирает (замена WCF svc/SettingsService WebV1, AI/wiki/user-settings.md) </summary>
[Route("Api/Settings")]
public class ApiSettingsController : ApiBaseController {

    private UserSettingService Service => Host.GetService<UserSettingService>();

    public ApiSettingsController(ServicesHost host) : base(host) { }

    [HttpGet]
    public async Task<ActionResult> Get() {
        var json = await Service.GetAllJsonAsync(GetUserId());
        return new ContentResult { StatusCode = (int)HttpStatusCode.OK, Content = json ?? "null", ContentType = "application/json" };
    }

    /// <summary>
    ///     Тело запроса — JSON секции. Проверяется синтаксис: битая секция сломала бы ответ Get целиком </summary>
    [HttpPut("{section}")]
    public async Task<ActionResult> Set(string section) {
        if (section.Length > 50) {
            return JsonFail(HttpStatusCode.BadRequest, "Section name is too long");
        }
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        if (String.IsNullOrWhiteSpace(json)) {
            return JsonFail(HttpStatusCode.BadRequest, "Body is empty");
        }
        try {
            JsonDocument.Parse(json).Dispose();
        } catch (JsonException) {
            return JsonFail(HttpStatusCode.BadRequest, "Body is not valid JSON");
        }
        await Service.SetJsonAsync(GetUserId(), section, json);
        return JsonSuccess();
    }
}
