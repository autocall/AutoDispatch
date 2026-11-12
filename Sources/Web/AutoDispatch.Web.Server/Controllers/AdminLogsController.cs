using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.Log;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/Logs/[action]")]
public class AdminLogsController : BaseController {

    private LogService Service => Host.GetService<LogService>();

    #region [ .ctor ]

    public AdminLogsController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("~/Admin/Logs")]
    public async Task<ActionResult> Index(LogFilterDto filter) {
        filter.Level ??= "Information";
        return View(new LogsIndexViewModel {
            Filter = filter,
            Scopes = await Service.GetScopesAsync(),
            Stats = await Service.GetStatsAsync(),
            Entries = await Service.SearchAsync(filter)
        });
    }
}
