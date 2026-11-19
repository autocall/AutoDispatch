using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     Вход App: V1 — Api/Token (OAuth, form), Api/Account/Check, Api/Account/Logout; теперь Api/Account/SignIn → SessionDto
    ///     и Api/Account/Session (JWT, AI/wiki/protocol.md). Logout на сервере не нужен: у JWT нет серверной сессии </summary>
    public class LoginDataService : BaseDataService {

        public LoginDataService(HttpClient http) : base(http) { }

        #region [ Public Methods ]

        public async Task<ApiLoginResponse> LoginAsync(ApiLoginRequest request) {
            using (var message = CreateRequest(HttpMethod.Post, "Api/Account/SignIn", new { email = request.UserName, password = request.UserPassword }, authorize: false)) {
                using (var response = await Http.SendAsync(message)) {
                    var text = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode == false) {
                        throw new Exception(ReadError(text) ?? $"Server responded {(int)response.StatusCode}");
                    }
                    return JsonConvert.DeserializeObject<ApiLoginResponse>(text);
                }
            }
        }

        /// <summary>
        ///     Письмо со ссылкой сброса (Api/Account/ForgotPassword, анонимный); V1 — IApiManager.ForgotPasswordAsync модуля AutoCall </summary>
        public async Task<string> ForgotPasswordAsync(string email) {
            using (var message = CreateRequest(HttpMethod.Post, "Api/Account/ForgotPassword", new { email }, authorize: false)) {
                using (var response = await Http.SendAsync(message)) {
                    var text = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode == false) {
                        throw new Exception(ReadError(text) ?? $"Server responded {(int)response.StatusCode}");
                    }
                    return (string)Newtonsoft.Json.Linq.JObject.Parse(text)["message"];
                }
            }
        }

        public async Task<ApiCheckTokenResponse> CheckTokenAsync(ApiCheckTokenRequest request) {
            await SendAsync(HttpMethod.Get, "Api/Account/Session");
            return new ApiCheckTokenResponse();
        }

        #endregion [ Public Methods ]
    }
}
