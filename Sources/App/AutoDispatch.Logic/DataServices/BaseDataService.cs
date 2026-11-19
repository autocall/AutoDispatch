using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoDispatch.Data.Api;
using AutoDispatch.Dtos.Enums;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     Базовый класс data-сервисов: в V1 — HttpWebRequest на BaseUrl из app.config (UrlApi) и OWIN-токен; теперь BaseApi AutoCall V3
    ///     (HttpClient на Settings.WebUrl, Bearer из UserSession.Current, X-Request-Id). Эндпоинты — маршруты V3 сервера AutoDispatch,
    ///     ответ — данные как есть или 400 { error } (AI/wiki/protocol.md «Остальные эндпоинты App»). DTO контракта читаются Newtonsoft
    ///     (атрибуты AutoDispatch.Dtos), регистр имён не важен </summary>
    public abstract class BaseDataService : BaseApi {

        protected BaseDataService(HttpClient http) : base(http) { }

        /// <summary>
        ///     Запрос с JSON-телом (Newtonsoft, без $type) или без тела; ответ — строка тела, ошибка — Exception с текстом сервера </summary>
        protected async Task<string> SendAsync(HttpMethod method, string url, object body = null) {
            using (var request = CreateRequest(method, url)) {
                if (body != null) {
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                }
                using (var response = await Http.SendAsync(request)) {
                    var text = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode == false) {
                        throw new Exception(ReadError(text) ?? $"Server responded {(int)response.StatusCode}");
                    }
                    return text;
                }
            }
        }

        /// <summary>
        ///     Ответ сервера в конверт V1 (*Response: Status/ErrorMessage), как делали data-сервисы V1 с HttpRequestResult </summary>
        protected async Task<TResponse> GetResponseAsync<TResponse>(Func<Task<TResponse>> action) where TResponse : BaseResponse, new() {
            try {
                return await action();
            } catch (Exception e) {
                return new TResponse() { Status = ResponseStatus.Failed, ErrorMessage = e.Message };
            }
        }
    }
}
