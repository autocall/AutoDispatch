using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Logic.WebServices.OpenAI;

/// <summary>
///     chat/completions для разбора rate confirmation (V1 OpenAIDataService) </summary>
public class OpenAIWebService : BaseWebService {

    private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

    public OpenAIWebService(ServicesHost host) { }

    public async Task<ApiResult<ResponseData>> RequestAsync(RequestData requestData) {
        if (AppSettings.OpenAiApiKey == null) {
            return ApiResult<ResponseData>.Failed(System.Net.HttpStatusCode.ServiceUnavailable, "OpenAI is not configured");
        }
        try {
            using var request = new HttpRequestMessage(HttpMethod.Post, AppSettings.OpenAiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.OpenAiApiKey);
            request.Content = new StringContent(DispatchJson.Serialize(requestData, TypeNameHandling.None), Encoding.UTF8, "application/json");
            using var response = await Http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var responseData = DispatchJson.Deserialize<ResponseData>(json);
            if (response.IsSuccessStatusCode == false) {
                return ApiResult<ResponseData>.Failed(response.StatusCode, responseData?.error?.message ?? json.Truncate(200));
            }
            responseData.ContentJson = json; // for logs
            return ApiResult<ResponseData>.Successed(responseData);
        } catch (Exception e) {
            _l.e("OpenAI request failed", e);
            return ApiResult<ResponseData>.Failed(e);
        }
    }
}

#region [ models (имена полей — как в JSON OpenAI) ]

public abstract class RequestData {
    public string model { get; set; }
    public decimal temperature { get; set; }
}

public class RequestImageData : RequestData {
    public MessageImageData[] messages { get; set; }
}

public class RequestTextData : RequestData {
    public MessageTextData[] messages { get; set; }
}

public class MessageTextData {
    public string role { get; set; }
    public string content { get; set; }
}

public class MessageImageData {
    public string role { get; set; }
    public MessageImageContent[] content { get; set; }
}

public class MessageImageContent {
    public string type { get; set; }
    public string text { get; set; }
    public MessageImageImage image_url { get; set; }
}

public class MessageImageImage {
    public string url { get; set; }
}

public class ResponseData {
    public string id { get; set; }
    public string model { get; set; }
    public UsageData usage { get; set; }
    public ChoiceData[] choices { get; set; }
    public Error error { get; set; }
    /// <summary>
    ///     Extra: сырой ответ для лога </summary>
    public string ContentJson { get; set; }
}

public class UsageData {
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}

public class ChoiceData {
    public MessageTextData message { get; set; }
    public string finish_reason { get; set; }
    public int index { get; set; }
}

public class Error {
    public string message { get; set; }
    public string type { get; set; }
    public object param { get; set; }
    public object code { get; set; }
}

#endregion [ models (имена полей — как в JSON OpenAI) ]
