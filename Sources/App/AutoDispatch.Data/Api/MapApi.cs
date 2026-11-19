#nullable enable
using System.Text.Json;

namespace AutoDispatch.Data.Api;

/// <summary> Точка маршрута для Api/Maps/Link. Kind: 1 — паттерн, 2 — запись (груз), как MapPointKindEnum сервера (копия AutoCall V3). </summary>
public sealed record MapPointDto(int Kind, float Latitude, float Longitude) {
    public const int KindPattern = 1;
    public const int KindRecord = 2;
}

/// <summary> Точка для Api/Maps/SelectLocationLink (MapLatLngDto сервера) </summary>
public sealed record MapLatLngDto(float Latitude, float Longitude);

/// <summary>
///     Ссылки на страницы карт сервера (/Map?r=…, /Map/SelectLocation?r=…, /Map/OrderStops?r=…): параметры шифрует сервер.
///     Замена Dispatch/RouteLocation?t=…, SelectLocation, OrderStops V1 (EO.WebBrowser с Bearer) — AI/wiki/public-site.md </summary>
public class MapApi : BaseApi {

    public MapApi(HttpClient http) : base(http) { }

    /// <summary> Маршрут: точки в порядке маршрута; пустой список — пустая карта. </summary>
    public Task<string> CreateLinkAsync(IReadOnlyList<MapPointDto> points, CancellationToken cancellationToken = default) {
        return PostForUrlAsync("Api/Maps/Link", points, cancellationToken);
    }

    /// <summary> Выбор точки на карте; null — без начальной точки. </summary>
    public Task<string> CreateSelectLocationLinkAsync(MapLatLngDto? point, CancellationToken cancellationToken = default) {
        return PostForUrlAsync("Api/Maps/SelectLocationLink", point, cancellationToken);
    }

    /// <summary> Остановки трейлера на карте. </summary>
    public Task<string> CreateOrderStopsLinkAsync(Guid trailerId, CancellationToken cancellationToken = default) {
        return PostForUrlAsync("Api/Maps/OrderStopsLink", new { trailerId }, cancellationToken);
    }

    private async Task<string> PostForUrlAsync(string url, object? body, CancellationToken cancellationToken) {
        using var request = CreateRequest(HttpMethod.Post, url, body);
        if (body == null) {
            request.Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json");
        }
        using HttpResponseMessage message = await Http.SendAsync(request, cancellationToken);
        string body2 = await message.Content.ReadAsStringAsync(cancellationToken);
        if (!message.IsSuccessStatusCode) {
            throw new Exception(ReadError(body2) ?? $"Map link: server responded {(int)message.StatusCode}");
        }
        using var doc = JsonDocument.Parse(body2);
        return doc.RootElement.GetProperty("url").GetString() ?? throw new Exception("Map link: empty url");
    }
}
