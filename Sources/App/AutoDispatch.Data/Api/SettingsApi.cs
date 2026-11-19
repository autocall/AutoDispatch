#nullable enable
using System.Text;

namespace AutoDispatch.Data.Api;

/// <summary>
///     Настройки клиента на AutoDispatch Web (Api/Settings) по секциям: чтение — все секции одним объектом, запись — по одной;
///     JSON туда и обратно как есть, пользователь — из токена </summary>
public class SettingsApi : BaseApi {

    private const string Url = "Api/Settings";

    public SettingsApi(HttpClient http) : base(http) { }

    /// <summary> { "SoundSettings": {…}, "Volume": 50, … }; пустая строка — настроек ещё нет (сервер отдаёт null). </summary>
    public async Task<string> GetSettingsAsync() {
        using var request = CreateRequest(HttpMethod.Get, Url);
        using var message = await Http.SendAsync(request);
        var body = await message.Content.ReadAsStringAsync();
        if (!message.IsSuccessStatusCode) {
            throw new Exception(ReadError(body) ?? $"Settings: server responded {(int)message.StatusCode}");
        }
        return body == "null" ? string.Empty : body;
    }

    public async Task SetSectionAsync(string section, string json) {
        using var request = CreateRequest(HttpMethod.Put, $"{Url}/{section}");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var message = await Http.SendAsync(request);
        if (!message.IsSuccessStatusCode) {
            var body = await message.Content.ReadAsStringAsync();
            throw new Exception(ReadError(body) ?? $"Settings: server responded {(int)message.StatusCode}");
        }
    }
}
