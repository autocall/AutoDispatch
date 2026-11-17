using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Карриеры App (V1 api/DatCompany): поля FMCSA по DOT или MC. SendSetupPocket V1 на сервере не существовал — не переносится </summary>
[Route("Api/DatCompany/[action]")]
public class ApiDatCompanyController : ApiBaseController {

    private DatCompanyService Service => Host.GetService<DatCompanyService>();

    public ApiDatCompanyController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Api/DatCompany/Search?dot=123 или ?mc=456 → словарь «Legal Name:» → значение (V1 Fields).
    ///     Ключи — как есть: camelCase JsonExtension переписал бы их в «legal Name:», а клиент ищет по оригинальным </summary>
    [HttpGet]
    public async Task<ActionResult> Search(int? dot, int? mc) {
        var fields = await Service.GetFieldsAsync(dot, mc);
        return Content(DispatchJson.Serialize(fields, Newtonsoft.Json.TypeNameHandling.None), "application/json");
    }
}
