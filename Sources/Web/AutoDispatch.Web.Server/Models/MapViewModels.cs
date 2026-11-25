using AutoDispatch.Web.Logic.Dtos.App;

namespace AutoDispatch.Web.Server.Models;

public class MapIndexViewModel {
    /// <summary>
    ///     Параметр r как пришёл — страница шлёт его с каждым запросом вместо cookie </summary>
    public string Token { get; set; }
    /// <summary>
    ///     Точки маршрута без остановок; остановки из ссылки (Stop) — отдельно, страница ведёт их как поставленные кликом </summary>
    public List<MapPointDto> Points { get; set; }
    public List<MapLatLngDto> Stops { get; set; }
    /// <summary>
    ///     В ссылке есть груз Show Trip — страница запросит историю его положений (Map/History) </summary>
    public bool History { get; set; }
    public MapSettingsDto Settings { get; set; }
    public string GoogleMapJsKey { get; set; }
    public string AerisWeatherId { get; set; }
    public string AerisWeatherSecret { get; set; }
    public int MaxStops { get; set; }
    /// <summary>
    ///     Открыта в WebView2 App (Map?embedded=true) — без навигации сайта; в браузере — обычный _Layout </summary>
    public bool Embedded { get; set; }
}

/// <summary>
///     Тело POST-запросов страницы: токен ссылки и, для Map/Link — остановки, для Map/Settings — настройки </summary>
public class MapRequest {
    public string R { get; set; }
    public List<MapLatLngDto> Stops { get; set; }
    public MapSettingsDto Settings { get; set; }
}

/// <summary>
///     Тело Api/Maps/OrderStopsLink </summary>
public class MapOrderStopsLinkRequest {
    public Guid TrailerId { get; set; }
}

/// <summary>
///     Панель «Map options» (_MapOptionsPart): токен ссылки для Map/Settings, настройки, ключи радара погоды </summary>
public class MapOptionsPartModel {
    public string Token { get; set; }
    public MapSettingsDto Settings { get; set; }
    public string AerisWeatherId { get; set; }
    public string AerisWeatherSecret { get; set; }
    public bool Embedded { get; set; }
}

/// <summary>
///     Map/SelectLocation: начальная точка из ссылки (kind Location) или null — пустая карта </summary>
public class MapSelectLocationViewModel {
    public string Token { get; set; }
    public MapLatLngDto Point { get; set; }
    public MapSettingsDto Settings { get; set; }
    public string GoogleMapJsKey { get; set; }
    public bool Embedded { get; set; }
    public MapOptionsPartModel Options { get; set; }
}

/// <summary>
///     Map/OrderStops: остановки трейлера в порядке PickupOrder/DeliveryOrder </summary>
public class MapOrderStopsViewModel {
    public string Token { get; set; }
    public List<MapStopDto> Stops { get; set; }
    public MapSettingsDto Settings { get; set; }
    public string GoogleMapJsKey { get; set; }
    public int MaxStops { get; set; }
    public bool Embedded { get; set; }
    public MapOptionsPartModel Options { get; set; }
}

/// <summary>
///     Тело Map/OrderStops/Route: токен ссылки и остановки в порядке, выставленном в App </summary>
public class MapStopsRouteRequest {
    public string R { get; set; }
    public List<MapLatLngDto> Points { get; set; }
}
