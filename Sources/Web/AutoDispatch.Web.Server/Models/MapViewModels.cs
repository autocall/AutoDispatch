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
