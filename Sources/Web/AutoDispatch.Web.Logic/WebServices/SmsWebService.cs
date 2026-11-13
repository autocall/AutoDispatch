using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Logic.WebServices;

/// <summary>
///     SMS через Bandwidth (V1 SmsDataService.SendBrandwidthSmsAsync без картинок — они были нужны только чату).
///     Voxbone в V1 помечен как не работающий и не переносится </summary>
public class SmsWebService : BaseWebService {

    private static readonly HttpClient Http = new HttpClient { Timeout = AppSettings.ExternalTimeout };

    public SmsWebService(ServicesHost host) { }

    public async Task<ApiResult<string>> SendSmsAsync(string fromPhone, string toPhone, string message) {
        if (AppSettings.BandwidthUrl == null || AppSettings.BandwidthUser == null) {
            return ApiResult<string>.Failed(System.Net.HttpStatusCode.ServiceUnavailable, "Bandwidth is not configured");
        }
        try {
            using var request = new HttpRequestMessage(HttpMethod.Post, AppSettings.BandwidthUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{AppSettings.BandwidthUser}:{AppSettings.BandwidthPassword}")));
            var data = DispatchJson.Serialize(new {
                from = DispatchStringExtension.ToLocalPhone(fromPhone),
                to = DispatchStringExtension.ToLocalPhone(toPhone),
                applicationId = AppSettings.BandwidthApplicationId,
                text = message,
                media = new string[0]
            }, TypeNameHandling.None);
            request.Content = new StringContent(data, Encoding.UTF8, "application/json");
            using var response = await Http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false) {
                _l.e($"Bandwidth [{(int)response.StatusCode}] to {toPhone}: {json.Truncate(500)}");
                return ApiResult<string>.Failed(response.StatusCode, json.Truncate(200));
            }
            _l.i("SendBrandwidthSmsAsync\n" + json);
            return ApiResult<string>.Successed(json);
        } catch (Exception e) {
            _l.e($"Bandwidth to {toPhone} failed", e);
            return ApiResult<string>.Failed(e);
        }
    }
}
