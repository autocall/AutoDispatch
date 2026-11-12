using System.Net.Http.Headers;
using System.Text;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Logic.WebServices;

/// <summary>
///     Системные письма через Mailgun (как в Safer): Basic api:key, multipart в /v3/{domain}/messages </summary>
public class MailgunWebService : BaseWebService {

    private static readonly HttpClient Http = new HttpClient { BaseAddress = new Uri(AppSettings.MailgunApiUrl) };

    public MailgunWebService(ServicesHost host) { }

    /// <summary>
    ///     Возвращает id сообщения Mailgun </summary>
    public async Task<ApiResult<string>> SendAsync(string to, string subject, string html) {
        if (AppSettings.MailgunApiKey == null || AppSettings.MailgunDomain == null || AppSettings.MailgunFrom == null) {
            return ApiResult<string>.Failed(System.Net.HttpStatusCode.ServiceUnavailable, "Mailgun is not configured");
        }
        try {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/v3/{AppSettings.MailgunDomain}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{AppSettings.MailgunApiKey}")));
            var content = new MultipartFormDataContent {
                { new StringContent(AppSettings.MailgunFrom), "from" },
                { new StringContent(to), "to" },
                { new StringContent(subject), "subject" },
                { new StringContent(html), "html" }
            };
            request.Content = content;

            using var response = await Http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false) {
                _l.e($"Mailgun [{(int)response.StatusCode}] to {to}: {body.Truncate(500)}");
                return ApiResult<string>.Failed(response.StatusCode, body.Truncate(200));
            }
            return ApiResult<string>.Successed(body);
        } catch (Exception e) {
            _l.e($"Mailgun send to {to} failed", e);
            return ApiResult<string>.Failed(e);
        }
    }
}
