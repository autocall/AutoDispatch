using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Ссылка на карту маршрута для App: точки уходят в зашифрованный параметр r страницы /Map.
///     Замена сборки Dispatch/RouteLocation?t=… на стороне клиента V1 </summary>
[Route("Api/Maps")]
public class ApiMapsController : ApiBaseController {

    private MapService Service => Host.GetService<MapService>();

    public ApiMapsController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Тело — точки в порядке маршрута (паттерн, запись, запись, паттерн), может быть пустым; ответ { url } </summary>
    [HttpPost("Link")]
    public ActionResult Link([FromBody] List<MapPointDto> points) {
        points ??= new List<MapPointDto>();
        using var scope = _l.Scope("map");
        using var details = _l.Property(AppSettings.LogDetailsProperty, JsonExtension.Serialize(points));
        var url = Service.CreateLink(GetUserId(), points);
        _l.i($"Map link: {points.Count} points");
        return JsonSuccess(new { url });
    }
}
