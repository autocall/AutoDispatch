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
            return (int)message.StatusCode < 500 ? ClientLogSendResult.Rejected : ClientLogSendResult.Unavailable;
        } catch (Exception) when (!cancellationToken.IsCancellationRequested) {
            return ClientLogSendResult.Unavailable;
        }
    }
}
