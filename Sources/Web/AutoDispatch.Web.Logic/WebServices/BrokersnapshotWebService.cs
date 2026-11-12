using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Monitoring;

namespace AutoDispatch.Web.Logic.WebServices;

/// <summary>
///     Геокодинг Safer (brokersnapshot.com/api/v2/geo, Bearer): автокомплит городов и координаты по place_id.
///     Замена прокси keeppingin.com (Google Places Legacy). Ответы Safer отдаются как есть, разбор — в PlaceService </summary>
public class BrokersnapshotWebService : BaseWebService {

    private static readonly HttpClient Http = new HttpClient {
        BaseAddress = new Uri(AppSettings.BrokersnapshotApiUrl),
        Timeout = AppSettings.BrokersnapshotTimeout
    };

    private readonly MetricCollector _metrics;

    public BrokersnapshotWebService(ServicesHost host) {
        this._metrics = host.ServiceProvider.GetRequiredService<MetricCollector>();
    }

    /// <summary>
    ///     Autocomplete (New): { Success, Data: { predictions: [ { description, place_id, main_text,
    ///     secondary_text, types } ], source } }. citiesOnly — kind=cities (App), иначе адреса и города (поиск на карте) </summary>
    public Task<ApiResult<string>> AutocompleteAsync(string search, bool citiesOnly) {
        var kind = citiesOnly ? "&kind=cities" : "";
        return this.GetAsync($"/api/v2/geo/google/autocomplete?input={Uri.EscapeDataString(search)}{kind}");
    }

    /// <summary>
    ///     Обратный геокодинг: { Success, Data: { found, formatted_address, city, state, ... } } </summary>
    public Task<ApiResult<string>> LatLngAsync(decimal lat, decimal lng) {
        return this.GetAsync($"/api/v2/geo/google/latlng?lat={lat.ToString(CultureInfo.InvariantCulture)}" +
                             $"&lng={lng.ToString(CultureInfo.InvariantCulture)}");
    }

    /// <summary>
    ///     Маршрут по точкам "lat,lng|lat,lng|…" (origin, waypoints, destination), кэш участков на стороне Safer:
    ///     { Success, Data: { legs: [ { from_lat, from_lng, to_lat, to_lng, polyline } ], source } }.
    ///     Без optimize — порядок остановок как задан </summary>
    public Task<ApiResult<string>> RouteAsync(string points) {
        return this.GetAsync($"/api/v2/route?points={Uri.EscapeDataString(points)}&optimize=false");
    }

    /// <summary>
    ///     Геокодинг по place_id: { Success, Data: { found, lat, lng, formatted_address, city, state, zip, ... } } </summary>
    public Task<ApiResult<string>> PlaceIdAsync(string placeId) {
        return this.GetAsync($"/api/v2/geo/google/placeid?place_id={Uri.EscapeDataString(placeId)}");
    }

    private async Task<ApiResult<string>> GetAsync(string url) {
        if (AppSettings.BrokersnapshotApiToken == null) {
            return ApiResult<string>.Failed(System.Net.HttpStatusCode.ServiceUnavailable, "Brokersnapshot is not configured");
        }
        var sw = Stopwatch.StartNew();
        try {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.BrokersnapshotApiToken);
            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false) {
                _metrics.AddCall(MetricTypeEnum.GeoLatency, sw.Elapsed, MetricTypeEnum.GeoErrorCounter);
                _l.e($"Brokersnapshot [{(int)response.StatusCode}] {url}: {body.Truncate(500)}");
                return ApiResult<string>.Failed(response.StatusCode, body.Truncate(200));
            }
            _metrics.AddCall(MetricTypeEnum.GeoLatency, sw.Elapsed, MetricTypeEnum.GeoCounter);
            return ApiResult<string>.Successed(body);
        } catch (Exception e) {
            _metrics.AddCall(MetricTypeEnum.GeoLatency, sw.Elapsed, MetricTypeEnum.GeoErrorCounter);
            _l.e($"Brokersnapshot {url} failed", e);
            return ApiResult<string>.Failed(e);
        }
    }
}
