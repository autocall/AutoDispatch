using System.Net;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Автокомплит локаций (Autocomplete New через Safer). Замена svc/CityService WebV1 </summary>
[Route("Api/Places")]
public class ApiPlacesController : ApiBaseController {

    private PlaceService Service => Host.GetService<PlaceService>();

    public ApiPlacesController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Подсказки без координат: Api/Places?search=Dalla </summary>
    [HttpGet]
    public async Task<ActionResult> Search(string search) {
        return JsonSuccess(await Service.SearchAsync(search));
    }

    /// <summary>
    ///     Координаты, штат и индекс выбранного места </summary>
    [HttpGet("{placeId}")]
    public async Task<ActionResult> Get(string placeId) {
        var dto = await Service.GetAsync(placeId);
        return dto == null ? JsonFail(HttpStatusCode.NotFound, "Place not found") : JsonSuccess(dto);
    }
}
