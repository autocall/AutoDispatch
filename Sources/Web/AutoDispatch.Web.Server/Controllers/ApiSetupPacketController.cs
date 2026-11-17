using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Справочники setup packet для App (V1 CarrierRegistration/FactoringCompanies — тот же список используют шаги регистрации карриера, этап 5) </summary>
[Route("Api/SetupPacket/[action]")]
public class ApiSetupPacketController : ApiBaseController {

    private SetupPacketService Service => Host.GetService<SetupPacketService>();

    public ApiSetupPacketController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     SetupPacketFactoringCompanyDto[] по имени </summary>
    [HttpGet]
    public async Task<ActionResult> FactoringCompanies() {
        var companies = await Service.GetFactoringCompaniesAsync();
        return JsonSuccess(companies.OrderBy(c => c.Name).ToList());
    }
}
