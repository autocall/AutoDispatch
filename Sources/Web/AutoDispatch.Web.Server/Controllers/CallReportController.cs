using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.CallReport;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Звонки и длительность по пользователям за день (CDR Vodia). Owner — своя компания, Admin — все </summary>
[Authorize(Roles = $"{nameof(RoleEnum.Owner)},{nameof(RoleEnum.Admin)}")]
[Route("CallReport")]
public class CallReportController : BaseController {

    private CallReportService Service => Host.GetService<CallReportService>();

    #region [ .ctor ]

    public CallReportController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("")]
    public async Task<ActionResult> Index(CallReportFilterDto filter) {
        return View(new CallReportIndexViewModel {
            Filter = filter,
            Data = await Service.GetReportAsync(filter, GetCompanyId(), User.IsInRole(nameof(RoleEnum.Admin)))
        });
    }
}
