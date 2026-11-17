using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Поиск компаний FMCSA для App (V1 Home/SearchCensusCompanies): CensusCompanySearchDto → CensusCompanyDto[] через Safer </summary>
[Route("Api/Census/[action]")]
public class ApiCensusController : ApiBaseController {

    private CensusService Service => Host.GetService<CensusService>();

    public ApiCensusController(ServicesHost host) : base(host) { }

    [HttpPost]
    public async Task<ActionResult> Search([FromBody] CensusCompanySearchDto search) {
        return JsonSuccess(await Service.SearchCensusCompaniesAsync(search));
    }
}
