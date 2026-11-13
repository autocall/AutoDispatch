using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities.Log;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Метрики: агрегация минуты без сервера и приём отчёта App (Api/Metrics) поднятым сервером </summary>
public class Metrics_Test {

    private DateTime _now = new DateTime(2026, 10, 15, 10, 0, 10, DateTimeKind.Utc);

    private static readonly Guid User1 = Guid.NewGuid(), User2 = Guid.NewGuid();

    private static ClientMetricsDto Report(int exceptions = 0) => new() { Exceptions = exceptions };

    private static int Value(List<Metric> rows, MetricTypeEnum type) => rows.Single(e => e.MetricId == type).Value;

    /// <summary>
    ///     Два отчёта одного пользователя в минуте: пользователи не удваиваются, счётчики складываются </summary>
    [Fact]
    public void Test01_TwoReportsPerMinute() {
        var collector = new MetricCollector(() => _now);
        collector.AddClientReport(User1, Report(exceptions: 1));
        _now = _now.AddSeconds(30);
        collector.AddClientReport(User1, Report(exceptions: 2));
        collector.AddClientReport(User2, Report());
        Assert.Empty(collector.TakeClosed());

        _now = _now.AddMinutes(1);
        var rows = collector.TakeClosed();

        Assert.Equal(2, Value(rows, MetricTypeEnum.OnlineUsers));
        Assert.Equal(3, Value(rows, MetricTypeEnum.AppExceptions));
        Assert.Empty(collector.TakeClosed());
    }

    [Fact]
    public void Test02_Step() {
        Assert.Equal(0, MetricConverter.GetStep(MetricTypeEnum.WebCpu, 69));
        Assert.Equal(1, MetricConverter.GetStep(MetricTypeEnum.WebCpu, 75));
        Assert.Equal(2, MetricConverter.GetStep(MetricTypeEnum.WebCpu, 90));
        Assert.Null(MetricConverter.GetStep(MetricTypeEnum.WebCpu, null));
        // у счётчиков шкалы нет
        Assert.Null(MetricConverter.GetStep(MetricTypeEnum.AppExceptions, 1));
    }

    /// <summary>
    ///     Против поднятого сервера: без токена 401, с токеном отчёт принят </summary>
    [Fact]
    public async Task Test03_ReportAccepted() {
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var password = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build()["Secrets:System:Password"];
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });

        var anonymous = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Metrics", Report(1));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = password });
        var token = (string)JObject.Parse(await signIn.Content.ReadAsStringAsync())["token"];
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Metrics", Report(1));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    ///     Api/Metrics/Add (скрипт сервера БД): чужой токен 401, неизвестный тип 400, DB CPU принят </summary>
    [Fact]
    public async Task Test04_ServerMetricAccepted() {
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var token = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build()["Secrets:Metric:Token"];
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        var url = $"{AppSettings.Website}/Api/Metrics/Add";

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");
        var wrong = await http.PostAsJsonAsync(url, new { type = 2, value = 42 });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var unknown = await http.PostAsJsonAsync(url, new { type = 250, value = 42 });
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);

        var response = await http.PostAsJsonAsync(url, new { type = 2, value = 42 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
