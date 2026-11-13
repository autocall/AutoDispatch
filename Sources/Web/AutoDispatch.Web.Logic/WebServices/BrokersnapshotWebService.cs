using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Logic.WebServices.Safer;

namespace AutoDispatch.Web.Logic.WebServices;

/// <summary>
///     Safer (brokersnapshot.com, Bearer): геокодинг api/v2/geo (автокомплит, координаты по place_id, обратный геокодинг, маршрут)
///     и карточки компаний FMCSA api/v3 (V1 DataServices/SaferFmcsa/CarrierViewDataService: setup packet, confirmation, census).
///     Замена прокси keeppingin.com (Google Places Legacy). Ответы geo отдаются как есть, разбор — в PlaceService/GeocodeService </summary>
public class BrokersnapshotWebService : BaseWebService {

    private static readonly HttpClient Http = new HttpClient {
        BaseAddress = new Uri(AppSettings.BrokersnapshotApiUrl),
        Timeout = AppSettings.BrokersnapshotTimeout
    };

    /// <summary>
    ///     Карточки компаний и сертификаты — дольше геокодинга (V1: минута) </summary>
    private static readonly HttpClient HttpCompany = new HttpClient {
        BaseAddress = new Uri(AppSettings.BrokersnapshotApiUrl),
        Timeout = AppSettings.ExternalTimeout
    };

    private readonly MetricCollector _metrics;

    public BrokersnapshotWebService(ServicesHost host) {
        this._metrics = host.ServiceProvider.GetRequiredService<MetricCollector>();
    }

    #region [ geo ]

    /// <summary>
    ///     Autocomplete (New): { Success, Data: { predictions: [ { description, place_id, main_text,
    ///     secondary_text, types } ], source } }. citiesOnly — kind=cities (App), иначе адреса и города (поиск на карте) </summary>
    public Task<ApiResult<string>> AutocompleteAsync(string search, bool citiesOnly) {
        var kind = citiesOnly ? "&kind=cities" : "&kind=geocode";
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

    #endregion [ geo ]

    #region [ company (api/v3) ]

    /// <summary>
    ///     Карточка по DOT (api/v3/company) или по MC (api/v3/companies, первая найденная) в форме api/v1 </summary>
    public async Task<ApiResult<SaferCompanyModel>> CompanyAsync(int? dot, int? mc) {
        if (dot == null && mc == null) {
            return ApiResult<SaferCompanyModel>.Failed(System.Net.HttpStatusCode.BadRequest, "Undefined DOT & MC");
        }
        var url = dot != null
            ? $"/api/v3/company?dot={dot}&include=3"
            : $"/api/v3/companies?prefix=MC&docket={mc}&limit=1&include=3";
        var result = await this.GetAsync(url, HttpCompany);
        if (result.Success == false) {
            return ApiResult<SaferCompanyModel>.Failed(result.StatusCode, result.ErrorMessage);
        }
        try {
            SaferModel envelope;
            SaferCompanyV3Model company;
            if (dot != null) {
                var companyResult = DispatchJson.Deserialize<SaferModel<SaferCompanyV3Model>>(result.Data);
                envelope = companyResult;
                company = companyResult.Data;
            } else {
                var companiesResult = DispatchJson.Deserialize<SaferModel<SaferCompanyV3Model[]>>(result.Data);
                envelope = companiesResult;
                company = companiesResult.Data?.FirstOrDefault();
            }
            if (envelope.Success == false) {
                return ApiResult<SaferCompanyModel>.Failed(System.Net.HttpStatusCode.BadRequest, envelope.Error);
            }
            if (company == null) {
                return ApiResult<SaferCompanyModel>.Failed(System.Net.HttpStatusCode.NotFound, "Company was not found");
            }
            return ApiResult<SaferCompanyModel>.Successed(SaferCompanyV3Converter.ToModel(company, mc));
        } catch (Exception e) {
            _l.e($"Brokersnapshot company {url}", e);
            return ApiResult<SaferCompanyModel>.Failed(e);
        }
    }

    /// <summary>
    ///     Поиск компаний (census): queries — Limit, DOT, Name, Address, City, State, Contact, Prefix, Docket </summary>
    public async Task<ApiResult<SaferCompanyModel[]>> CompaniesAsync(Dictionary<string, string> queries) {
        if (queries == null || queries.Count == 0) {
            return ApiResult<SaferCompanyModel[]>.Failed(System.Net.HttpStatusCode.BadRequest, "Undefined Query");
        }
        queries["include"] = "3";
        var url = "/api/v3/companies?" + String.Join("&", queries.Select(e => $"{e.Key}={Uri.EscapeDataString(e.Value ?? "")}"));
        var result = await this.GetAsync(url, HttpCompany);
        if (result.Success == false) {
            return ApiResult<SaferCompanyModel[]>.Failed(result.StatusCode, result.ErrorMessage);
        }
        try {
            var companiesResult = DispatchJson.Deserialize<SaferModel<SaferCompanyV3Model[]>>(result.Data);
            if (companiesResult.Success == false) {
                return ApiResult<SaferCompanyModel[]>.Failed(System.Net.HttpStatusCode.BadRequest, companiesResult.Error);
            }
            return ApiResult<SaferCompanyModel[]>.Successed(
                (companiesResult.Data ?? new SaferCompanyV3Model[0]).Select(e => SaferCompanyV3Converter.ToModel(e, null)).ToArray());
        } catch (Exception e) {
            _l.e($"Brokersnapshot companies {url}", e);
            return ApiResult<SaferCompanyModel[]>.Failed(e);
        }
    }

    public class Certificate {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
    }

    /// <summary>
    ///     Сертификат MC (PDF) для setup packet: api/DownloadCertificate, без токена; имя файла — из Content-Disposition </summary>
    public async Task<ApiResult<Certificate>> DownloadCertificateAsync(string prefix, int? docket, int? dot) {
        var url = $"/api/DownloadCertificate?prefix={prefix}&docket={docket}&dot={dot}";
        try {
            using var response = await HttpCompany.GetAsync(url);
            if (response.IsSuccessStatusCode == false) {
                return ApiResult<Certificate>.Failed(response.StatusCode, response.ReasonPhrase);
            }
            var data = await response.Content.ReadAsByteArrayAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "unknown.pdf";
            return ApiResult<Certificate>.Successed(new Certificate { Data = data, FileName = fileName });
        } catch (Exception e) {
            _l.e($"Brokersnapshot {url} failed", e);
            return ApiResult<Certificate>.Failed(e);
        }
    }

    #endregion [ company (api/v3) ]

    private async Task<ApiResult<string>> GetAsync(string url, HttpClient http = null) {
        if (AppSettings.BrokersnapshotApiToken == null) {
            return ApiResult<string>.Failed(System.Net.HttpStatusCode.ServiceUnavailable, "Brokersnapshot is not configured");
        }
        http = http ?? Http;
        var sw = Stopwatch.StartNew();
        try {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.BrokersnapshotApiToken);
            using var response = await http.SendAsync(request);
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
