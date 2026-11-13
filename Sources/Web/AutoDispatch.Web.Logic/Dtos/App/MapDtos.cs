namespace AutoDispatch.Web.Logic.Dtos.App;

public enum MapPointKindEnum {
    Unknown = 0,
    /// <summary>
    ///     Origin/destination паттерна (серча) — красный пунктир до точки записи, как в V1 </summary>
    Pattern = 1,
    /// <summary>
    ///     Origin/destination записи (груза) — зелёная линия, в неё вставляются остановки </summary>
    Record = 2,
    /// <summary>
    ///     Остановка, поставленная кликом на карте: страница перевыпускает ссылку с ними (Map/Link),
    ///     чтобы скопированная ссылка открывала карту уже с остановками </summary>
    Stop = 3
}

/// <summary>
///     Точка маршрута из App (Api/Maps/Link) </summary>
public class MapPointDto {
    public MapPointKindEnum Kind { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

/// <summary>
///     Содержимое ссылки r: кто открыл и что рисовать </summary>
public class MapLinkDto {
    public Guid UserId { get; set; }
    public List<MapPointDto> Points { get; set; }
}

public class MapLatLngDto {
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

/// <summary>
///     Отрезок между соседними точками ссылки; остановки вставлены в основной (первый Record) </summary>
public class MapSegmentDto {
    public MapPointKindEnum Kind { get; set; }
    /// <summary>
    ///     Google Encoded Polyline каждого плеча, как отдал Safer </summary>
    public List<string> Polylines { get; set; }
}

public class MapRouteDto {
    public List<MapSegmentDto> Segments { get; set; }
}

public enum MapTypeEnum {
    Unknown = 0,
    Roadmap = 1,
    Hybrid = 2
}

/// <summary>
///     Тема карты в WebView2 App; в браузере тема — сайта, это значение не используется </summary>
public enum MapThemeEnum {
    Unknown = 0,
    Light = 1,
    Dark = 2
}

/// <summary>
///     Секция Map настроек пользователя (UserSetting): тип карты, слои, тема </summary>
public class MapSettingsDto {
    public MapTypeEnum Type { get; set; }
    public bool Traffic { get; set; }
    public bool Weather { get; set; }
    public MapThemeEnum Theme { get; set; }
}
