using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog.Context;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Server.Extensions;

namespace AutoDispatch.Web.Server.Infrastructure;

/// <summary>
///     После аутентификации: свойства UserName и RequestPath во все логи запроса (колонки таблицы Log),
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
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        using (LogContext.PushProperty("UserName", userName)) {
            await _next(context);
        }
    }
}
