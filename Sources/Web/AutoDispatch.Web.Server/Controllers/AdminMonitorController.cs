using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/Monitor/[action]")]
public class AdminMonitorController : BaseController {

    private MonitorService Service => Host.GetService<MonitorService>();
    private LogService LogService => Host.GetService<LogService>();
    private LoginService LoginService => Host.GetService<LoginService>();

    private readonly IWebHostEnvironment _environment;

    #region [ .ctor ]

    public AdminMonitorController(ServicesHost host, IWebHostEnvironment environment) : base(host) {
        this._environment = environment;
    }

    #endregion [ .ctor ]

    [HttpGet("~/Admin/Monitor")]
    public async Task<ActionResult> Index() {
        return View(new MonitorIndexViewModel {
            Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            Environment = _environment.EnvironmentName,
            StartedAt = Process.GetCurrentProcess().StartTime.ToUniversalTime(),
            Databases = await Service.PingAsync(),
            LogTable = await LogService.GetTableSizeAsync(),
            Online = Service.GetOnline(),
            LogStats = await LogService.GetStatsAsync(),
            RecentLogins = await LoginService.GetRecentAsync(20)
        });
    }
}
