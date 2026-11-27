using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog.Context;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Server.Extensions;

namespace AutoDispatch.Web.Server.Infrastructure;

/// <summary>
///     После аутентификации: свойства UserName, RequestPath и Session во все логи запроса (колонки таблицы Log), ClientRequestId,
///     отметка онлайн-активности пользователя </summary>
public class RequestContextMiddleware {

    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next) {
        this._next = next;
    }

    public async Task Invoke(HttpContext context, OnlineTracker tracker) {
        var user = context.User;
        // схема по умолчанию — cookie, Bearer проверяет только [Authorize] контроллера: здесь он ещё не разобран
        if (user.Identity?.IsAuthenticated != true && context.Request.Headers.ContainsKey("Authorization")) {
            var bearer = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (bearer.Succeeded) {
                context.User = user = bearer.Principal;
            }
        }
        string userName = null;
        if (user.Identity?.IsAuthenticated == true) {
            userName = user.FindFirst(ClaimTypes.Email)?.Value ?? user.Identity.Name;
            var companyClaim = user.FindFirst("CompanyId");
            if (companyClaim != null) {
                tracker.Touch(user.GetUserId(), Guid.Parse(companyClaim.Value), userName, context.Request.GetLoginSource());
            }
        }
        // X-Request-Id App: «{сессия}-{номер}» — сессия в колонку Session (фильтр Session на /Admin/Logs показывает записи
        // клиента и сервера одной ленты), весь id — в LogEvent (ClientRequestId); у сайта заголовка нет
        var requestId = context.Request.Headers["X-Request-Id"].ToString().Truncate(64);
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("UserName", userName))
        using (LogContext.PushProperty(AppSettings.LogSessionProperty, SessionOf(requestId)))
        using (LogContext.PushProperty("ClientRequestId", requestId.Length > 0 ? requestId : null)) {
            await _next(context);
        }
    }

    /// <summary>
    ///     Сессия App из X-Request-Id (AppSession.NextRequestId: «20261127-101500-a1b2c3-17»); нет номера — null </summary>
    public static string SessionOf(string requestId) {
        var index = requestId?.LastIndexOf('-') ?? -1;
        return index > 0 ? requestId.Substring(0, index).Truncate(32) : null;
    }
}
