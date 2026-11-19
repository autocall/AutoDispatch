#nullable enable
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoCall.Common;

namespace AutoDispatch.Data.Api;

public interface IApi { }
public abstract class BaseApi : IApi {

    /// <summary>
    ///     Контракт AutoDispatch Web: camelCase, регистр имён при чтении не важен </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected readonly HttpClient Http;

    public BaseApi(HttpClient http) {
        this.Http = http;
    }

    /// <summary>
    ///     Запрос к AutoDispatch Web; с authorize — Bearer из UserSession.Current (эндпоинты Api/* кроме входа).
    ///     X-Request-Id = сессия App + номер: сервер пишет его в запись запроса (ClientRequestId) </summary>
    protected static HttpRequestMessage CreateRequest(HttpMethod method, string url, object? body = null, bool authorize = true) {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Request-Id", AppSession.NextRequestId());
        if (body != null) {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }
        if (authorize && !string.IsNullOrEmpty(UserSession.Current?.AccessToken)) {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UserSession.Current.AccessToken);
        }
        return request;
    }

    /// <summary>
    ///     Текст ошибки из тела AutoDispatch Web: { error } либо { errors: { field: message } } </summary>
    protected static string? ReadError(string body) {
        if (string.IsNullOrWhiteSpace(body)) {
            return null;
        }
        try {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) {
                return null;
            }
            if (doc.RootElement.TryGetProperty("error", out var error)) {
                return error.GetString();
            }
            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object) {
                return string.Join("; ", errors.EnumerateObject().Select(e => e.Value.GetString()));
            }
        } catch (JsonException) {
        }
        return null;
    }
}
