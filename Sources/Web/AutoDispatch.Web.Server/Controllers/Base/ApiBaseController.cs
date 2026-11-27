using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     API для App: JWT Bearer, JSON; ModelState проверяется вручную через JsonFail(GetErrors())
///     (автоответ 400 отключён в Program — ApiBehaviorOptions.SuppressModelStateInvalidFilter) </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public abstract class ApiBaseController : BaseController {
    protected ApiBaseController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Статус пользователя (IsDeleted) проверяется на каждом запросе API: JWT живёт долго и сам по себе не отзывается
    ///     (V1 проверял VerifyId на каждом запросе; решено 2026-11-13, AI/wiki/audit.md A3). Удалённый — 401 </summary>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any() == false) {
            var user = await Host.UserManager.FindByIdAsync(GetUserId().ToString());
            if (user == null || user.IsDeleted) {
                // повторы одного пользователя режет LogDeduplicator (5 в минуту)
                _l.w($"Api 401: user {GetUserId()} {(user == null ? "not found" : "is deleted")}");
                context.Result = JsonFail(HttpStatusCode.Unauthorized, "Unauthorized");
                return;
            }
        }
        await next();
    }
}
