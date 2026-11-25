using System.Net;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Ссылка на карту маршрута для App: точки уходят в зашифрованный параметр r страницы /Map.
///     Замена сборки Dispatch/RouteLocation?t=… на стороне клиента V1 </summary>
[Route("Api/Maps")]
public class ApiMapsController : ApiBaseController {

    private MapService Service => Host.GetService<MapService>();

    public ApiMapsController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Тело — точки в порядке маршрута (паттерн, запись, запись, паттерн), может быть пустым; ответ { url }.
    ///     loadId — груз Show Trip: карта покажет историю его положений (V1 RouteLocation?DispatchLoadId=) </summary>
    [HttpPost("Link")]
    public ActionResult Link([FromBody] List<MapPointDto> points, [FromQuery] Guid? loadId = null) {
        points ??= new List<MapPointDto>();
        using var scope = _l.Scope("map");
        using var details = _l.Property(AppSettings.LogDetailsProperty, JsonExtension.Serialize(points));
        var url = Service.CreateLink(GetUserId(), points, loadId);
        _l.i($"Map link: {points.Count} points{(loadId == null ? "" : $", load {loadId}")}");
        return JsonSuccess(new { url });
    }

    /// <summary>
    ///     Ссылка на выбор места (V1 EditLocationDialog → Dispatch/SelectLocation?latlng=): тело — текущая точка или пусто; ответ { url } </summary>
    [HttpPost("SelectLocationLink")]
    public ActionResult SelectLocationLink([FromBody] MapLatLngDto point) {
        using var scope = _l.Scope("map");
        var url = Service.CreateSelectLocationLink(GetUserId(), point);
        _l.i($"Select location link: {(point == null ? "no point" : $"{point.Latitude}, {point.Longitude}")}");
        return JsonSuccess(new { url });
    }

    /// <summary>
    ///     Ссылка на остановки трейлера (V1 EditStopsDialog → Dispatch/OrderStops?TrailerId=): тело { trailerId }; ответ { url } </summary>
    [HttpPost("OrderStopsLink")]
    public ActionResult OrderStopsLink([FromBody] MapOrderStopsLinkRequest model) {
        using var scope = _l.Scope("map");
        if (model == null || model.TrailerId == Guid.Empty) {
            return JsonFail(HttpStatusCode.BadRequest, "TrailerId is required");
        }
        var url = Service.CreateOrderStopsLink(GetUserId(), model.TrailerId);
        _l.i($"Order stops link: trailer {model.TrailerId}");
        return JsonSuccess(new { url });
    }
}
