#nullable enable
using AutoDispatch.Data.Metrics;

namespace AutoDispatch.Data.Api;

/// <summary> Отчёт о состоянии App на сервер (Api/Metrics), раз в 30 с. Копия AutoCall V3 </summary>
public class MetricsApi : BaseApi {

    private const string Url = "Api/Metrics";

    public MetricsApi(HttpClient http) : base(http) { }

    /// <summary> false — отчёт не принят; повторять не нужно, отчёт без накопления. </summary>
    public async Task<bool> ReportAsync(ClientMetricsReport report, CancellationToken cancellationToken = default) {
        using var request = CreateRequest(HttpMethod.Post, Url, report);
        using HttpResponseMessage message = await Http.SendAsync(request, cancellationToken);
        return message.IsSuccessStatusCode;
    }
}
