using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Logic.WebServices.CarrierView;

/// <summary>
///     Трекинг водителей carrierview.com/api/loads (V1 DataServices/CarrierView): токен — DispatchCompany.CarrierViewToken </summary>
public class CarrierViewWebService : BaseWebService {

    private static readonly HttpClient Http = new HttpClient {
        BaseAddress = new Uri(AppSettings.CarrierViewUrl),
        Timeout = AppSettings.ExternalTimeout
    };

    public CarrierViewWebService(ServicesHost host) { }

    // {"success":false,"error_code":"required_fields_errors","errors":["Driver phone: Only US numbers available"]}
    // {"success":false,"error_code":"required_fields_errors","errors":{"driver_phone":["The driver phone field is required."]}}
    // {"success":false,"error_code":"creation_error","errors":{"code":"landline"}} - received unexpected format
    public Task<ApiResult<Response>> CreateLoadAsync(string token, CreateLoadRequest requestData) {
        _l.i("CreateLoad Request: " + DispatchJson.Serialize(requestData, TypeNameHandling.None));
        return SendAsync<Response>(token, HttpMethod.Post, "/api/loads", requestData);
    }

    public Task<ApiResult<Response>> EditLoadAsync(string token, int id, EditRequest requestData) {
        _l.i("EditLoad Request: " + DispatchJson.Serialize(requestData, TypeNameHandling.None));
        return SendAsync<Response>(token, HttpMethod.Patch, $"/api/loads/{id}", requestData);
    }

    public Task<ApiResult<Response>> GetLoadAsync(string token, int id) {
        return SendAsync<Response>(token, HttpMethod.Get, $"/api/loads/{id}", null);
    }

    // {"success":true,"data":{"position":{"driver_phone":"+15705735390","type":"stopped","rotation":"-171.9494382413","address":"Westfield, OH","latitude":"41.0291409","longitude":"-81.9084239","late_secs":0,"stopped_duration":16347,"drive_duration":null,"paused_duration":null}}}
    public Task<ApiResult<Response2>> GetPositionAsync(string token, int id) {
        return SendAsync<Response2>(token, HttpMethod.Get, $"/api/loads/{id}/last-position", null);
    }

    /// <summary>
    ///     DETACH + DISABLE </summary>
    public Task<ApiResult<Response>> DeleteLoadAsync(string token, DeleteLoadRequest requestData) {
        return SendAsync<Response>(token, HttpMethod.Patch, $"/api/loads/{requestData.Id}/disable", null);
    }

    private static async Task<ApiResult<T>> SendAsync<T>(string token, HttpMethod method, string url, object body) {
        try {
            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (body != null) {
                request.Content = new StringContent(DispatchJson.Serialize(body, TypeNameHandling.None), Encoding.UTF8, "application/json");
            }
            using var response = await Http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false) {
                _l.e($"CarrierView [{(int)response.StatusCode}] {method} {url}: {json.Truncate(500)}");
                return ApiResult<T>.Failed(response.StatusCode, json.Truncate(500));
            }
            try {
                return ApiResult<T>.Successed(DispatchJson.Deserialize<T>(json));
            } catch (Exception e) {
                _l.e($"CarrierView {method} {url}: {json.Truncate(500)}", e);
                return ApiResult<T>.Failed(e);
            }
        } catch (Exception e) {
            _l.e($"CarrierView {method} {url} failed", e);
            return ApiResult<T>.Failed(e);
        }
    }
}

#region [ models (имена полей — как в JSON carrierview.com) ]

public class Response {
    public bool success { get; set; }
    public DataResponse data { get; set; }
    public string error_code { get; set; }
    public IDictionary<string, string[]> errors { get; set; }
}

public class Response2 {
    public bool success { get; set; }
    public DataResponse2 data { get; set; }
    public string error_code { get; set; }
    public IDictionary<string, string[]> errors { get; set; }
}

public class DataResponse2 {
    public PositionResponse position { get; set; }
}

public class DataResponse {
    public int id { get; set; }
    public string tracking_number_url { get; set; }
    public string client_url { get; set; }
    public string driver_phone { get; set; }
    public string load_id { get; set; }
    public string mc_number { get; set; }
    public bool route_started { get; set; }
    public Track_DriverResponse track_driver { get; set; }
    public bool driver_is_late { get; set; }
    public object time_left_sec { get; set; }
    public object distance_left_meters { get; set; }
    public bool delivery_arrived { get; set; }
    public bool delivery_departed { get; set; }
    public string[] emails { get; set; }
    public string time_filter { get; set; }
    public string[] statuses { get; set; }
    public LocationResponse[] locations { get; set; }
}

public class Track_DriverResponse {
    public string phone { get; set; }
    public string app_status { get; set; }
    public long last_request_time_utc { get; set; }
}

public class LocationResponse {
    public int id { get; set; }
    public string address { get; set; }
    public CoordsResponse coords { get; set; }
    public string timezone { get; set; }
    public string dateFrom { get; set; }
    public string dateTo { get; set; }
    public string type { get; set; }
    public string company { get; set; }
    public string comment { get; set; }
    public DateTime? arrived_utc { get; set; }
    public string arrived { get; set; }
    public DateTime? departed_utc { get; set; }
    public string departed { get; set; }
}

public class CoordsResponse {
    public string lat { get; set; }
    public string lng { get; set; }
}

public class PositionResponse {
    public string driver_phone { get; set; }
    public string type { get; set; }
    public float rotation { get; set; }
    public string address { get; set; }
    public float latitude { get; set; }
    public float longitude { get; set; }
    public object late_secs { get; set; }
    public object stopped_duration { get; set; }
    public int? drive_duration { get; set; }
    public object paused_duration { get; set; }
}

public class EditRequest {
    public string mc_number { get; set; }
    public List<EditLocationRequest> locations { get; set; }
}

public class EditLocationRequest {
    public int id { get; set; }
    public string comment { get; set; }
}

public class CreateLoadRequest {
    public string driver_phone { get; set; }
    public string location_type { get; set; }
    public string mc_number { get; set; }
    public string load_id { get; set; }
    public string[] emails { get; set; }
    public CreateLocationRequest[] locations { get; set; }
}

public class CreateLocationRequest {
    public string address { get; set; }
    public string company { get; set; }
    public string comment { get; set; }
    public string type { get; set; }
    public string dateFrom { get; set; }
    public string dateTo { get; set; }
}

public class DeleteLoadRequest {
    public int Id { get; set; }
}

#endregion [ models (имена полей — как в JSON carrierview.com) ]
