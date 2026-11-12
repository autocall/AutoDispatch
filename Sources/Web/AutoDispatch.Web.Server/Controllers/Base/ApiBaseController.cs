using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     API для App: JWT Bearer, JSON; ModelState проверяется вручную через JsonFail(GetErrors())
///     (автоответ 400 отключён в Program — ApiBehaviorOptions.SuppressModelStateInvalidFilter) </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public abstract class ApiBaseController : BaseController {
    protected ApiBaseController(ServicesHost host) : base(host) { }
}
