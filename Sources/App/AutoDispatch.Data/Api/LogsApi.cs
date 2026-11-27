#nullable enable
using AutoDispatch.Data.Logs;

namespace AutoDispatch.Data.Api;

public interface IClientLogSender {
    Task<ClientLogSendResult> SendAsync(ClientLogBatch batch, CancellationToken cancellationToken = default);
}

/// <summary> Логи App пачками на AutoDispatch Web (Api/Logs); вызывается только ServerLogSink, сам ничего не логирует. </summary>
public class LogsApi : BaseApi, IClientLogSender {

    private const string Url = "Api/Logs";

    public LogsApi(HttpClient http) : base(http) { }

    public async Task<ClientLogSendResult> SendAsync(ClientLogBatch batch, CancellationToken cancellationToken = default) {
        try {
            using var request = CreateRequest(HttpMethod.Post, Url, batch);
            using HttpResponseMessage message = await Http.SendAsync(request, cancellationToken);
            if (message.IsSuccessStatusCode) {
                return ClientLogSendResult.Sent;
            }
            // 401 (токен истёк — пачка уйдёт после повторного входа), 408, 429 и 5xx — повторить позже; остальные 4xx — отказ.
            // Раньше 401 выбрасывал пачку: «Connection lost … 401» и «Session expired» на сервер не доходили (app-testing 9.17)
            int code = (int)message.StatusCode;
            return code == 401 || code == 408 || code == 429 || code >= 500 ? ClientLogSendResult.Unavailable : ClientLogSendResult.Rejected;
        } catch (Exception) when (!cancellationToken.IsCancellationRequested) {
            return ClientLogSendResult.Unavailable;
        }
    }
}
