using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Свои компании-отправители писем и документов (V1 ActualCompany/ActualCompaniesByUserId: компания — из токена, не из тела) </summary>
[Route("Api/ActualCompany")]
public class ApiActualCompanyController : ApiBaseController {

    private DispatchCompanyService Service => Host.GetService<DispatchCompanyService>();

    public ApiActualCompanyController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     ActualCompanyDto[] компании пользователя </summary>
    [HttpGet]
    public async Task<ActionResult> List() {
        return JsonSuccess(await Service.GetAllAsync(GetCompanyId()));
    }
}
