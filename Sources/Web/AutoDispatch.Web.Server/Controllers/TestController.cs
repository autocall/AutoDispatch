using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     GET /Test — проверка, что хост поднялся (CI, nginx, публикация) </summary>
[AllowAnonymous]
public class TestController : BaseController {

    private readonly IWebHostEnvironment _environment;

    public TestController(ServicesHost host, IWebHostEnvironment environment) : base(host) {
        this._environment = environment;
    }

    [HttpGet, Route("/Test")]
    public ActionResult Index() {
        return JsonSuccess(new {
            ok = true,
            version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            environment = _environment.EnvironmentName,
            utc = DateTime.UtcNow
        });
    }
}
