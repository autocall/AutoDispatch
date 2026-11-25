using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.CallReport;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

[Authorize]
public class HomeController : BaseController {

    private UserService UserService => Host.GetService<UserService>();
    private CallReportService CallReportService => Host.GetService<CallReportService>();

    public HomeController(ServicesHost host) : base(host) { }

    public async Task<ActionResult> Index() {
        var user = await Host.UserManager.GetUserAsync(User);
        return View(await UserService.GetIdentityAsync(user));
    }

    /// <summary>
    ///     Блок Phone channel congestion: грузится htmx после страницы, фильтры перерисовывают только его (как AutoCall V3) </summary>
    [Authorize(Roles = $"{nameof(RoleEnum.Owner)},{nameof(RoleEnum.Admin)}")]
    public async Task<ActionResult> Congestion(CallReportFilterDto filter) {
        return PartialView("_CongestionPart", new CongestionViewModel {
            Filter = filter,
            Data = await CallReportService.GetCongestionAsync(filter, GetCompanyId(), User.IsInRole(nameof(RoleEnum.Admin)))
        });
    }
}
