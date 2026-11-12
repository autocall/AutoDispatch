using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Extensions;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Карта маршрута (замена Dispatch/RouteLocation WebV1). Без логина: пропуск — зашифрованный r из Api/Maps/Link,
///     он же нужен каждому запросу страницы; так карта работает и в WebView2 App, и в браузере </summary>
[AllowAnonymous]
[Route("Map")]
public class MapController : BaseController {

    private MapService Service => Host.GetService<MapService>();
    private UserSettingService Settings => Host.GetService<UserSettingService>();

    public MapController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Все записи контроллера и сервиса — в Scope=map </summary>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
        using var scope = _l.Scope("map");
        await base.OnActionExecutionAsync(context, next);
    }

    [HttpGet("~/Map")]
    public async Task<ActionResult> Index(string r, bool embedded = false) {
        var link = Service.ParseLink(r);
        if (link == null) {
            return Forbidden(r);
        }
        var points = link.Points.Where(p => p.Kind != MapPointKindEnum.Stop).ToList();
        var stops = link.Points.Where(p => p.Kind == MapPointKindEnum.Stop)
            .Select(p => new MapLatLngDto { Latitude = p.Latitude, Longitude = p.Longitude }).ToList();
        // открытие в App уже видно по "Map link"; в браузере страница анонимная, UserName пуст — кто выдал ссылку, известно
        // только из токена
        if (embedded == false) {
            _l.i($"Map opened in browser: user {link.UserId}, {points.Count} points, {stops.Count} stops," +
                 $" {Request.GetIp()}, {Request.GetUserAgent()}");
        }
        return View(new MapIndexViewModel {
            Token = r,
            Points = points,
            Stops = stops,
            Settings = await Settings.GetMapAsync(link.UserId),
            GoogleMapJsKey = AppSettings.GoogleMapJsKey,
            AerisWeatherId = AppSettings.AerisWeatherId,
            AerisWeatherSecret = AppSettings.AerisWeatherSecret,
            MaxStops = AppSettings.MapMaxStops,
            Embedded = embedded
        });
    }

    /// <summary>
    ///     Новый токен с остановками — страница подставляет его в адрес, чтобы Copy link и браузер открывали карту с ними </summary>
    [HttpPost("Link")]
    public ActionResult Link([FromBody] MapRequest model) {
        var link = Service.ParseLink(model?.R);
        if (link == null) {
            return Forbidden(model?.R);
        }
        return JsonSuccess(new { token = Service.CreateToken(link, model.Stops) });
    }

    /// <summary>
    ///     Настройки карты пользователя, выдавшего ссылку (секция Map в UserSetting) </summary>
    [HttpPost("Settings")]
    public async Task<ActionResult> SaveSettings([FromBody] MapRequest model) {
        var link = Service.ParseLink(model?.R);
        if (link == null) {
            return Forbidden(model?.R);
        }
        await Settings.SetMapAsync(link.UserId, model.Settings);
        return JsonSuccess();
    }

    /// <summary>
    ///     Маршрут по точкам и остановкам ссылки </summary>
    [HttpPost("Route")]
    public async Task<ActionResult> Route([FromBody] MapRequest model) {
        var link = Service.ParseLink(model?.R);
        if (link == null) {
            return Forbidden(model?.R);
        }
        return JsonSuccess(await Service.GetRouteAsync(link));
    }

    /// <summary>
    ///     Адрес точки для панели: { address } (null — не найден) </summary>
    [HttpGet("Address")]
    public async Task<ActionResult> Address(string r, decimal lat, decimal lng) {
        if (Service.ParseLink(r) == null) {
            return Forbidden(r);
        }
        return JsonSuccess(new { address = await Service.GetAddressAsync(lat, lng) });
    }

    /// <summary>
    ///     Подсказки поиска адреса: [ { placeId, display } ] </summary>
    [HttpGet("Places")]
    public async Task<ActionResult> Places(string r, string search) {
        if (Service.ParseLink(r) == null) {
            return Forbidden(r);
        }
        return JsonSuccess(await Service.SearchAsync(search));
    }

    /// <summary>
    ///     Координаты выбранной подсказки </summary>
    [HttpGet("Places/{placeId}")]
    public async Task<ActionResult> Place(string r, string placeId) {
        if (Service.ParseLink(r) == null) {
            return Forbidden(r);
        }
        var dto = await Service.GetPlaceAsync(placeId);
        return dto == null ? JsonFail(HttpStatusCode.NotFound, "Place not found") : JsonSuccess(dto);
    }

    /// <summary>
    ///     Подделка, обрезанная ссылка или ссылка, выданная до смены Secrets:Map:Key </summary>
    private ActionResult Forbidden(string r) {
        _l.w($"Map link invalid: {Request.GetIp()}, r={r.TruncateToken()}");
        return new ContentResult {
            StatusCode = (int)HttpStatusCode.Forbidden,
            ContentType = "text/plain",
            Content = "The map link is invalid. Open the map from AutoDispatch again."
        };
    }
}
