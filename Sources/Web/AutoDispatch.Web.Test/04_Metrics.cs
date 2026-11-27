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

    private static readonly Guid User1 = Guid.NewGuid(), User2 = Guid.NewGuid(), User3 = Guid.NewGuid();

    // телефония — как AutoCall V3: каждый отчёт с одним звонком MicroSIP
    private static ClientMetricsDto Report(int exceptions = 0, bool? sip = null, int abtoErrors = 0) => new() {
        Exceptions = exceptions,
        Calls = [new ClientCallMetricsDto { Service = "MicroSIP", Calls = 1 }, new ClientCallMetricsDto { Service = "AbtoPhone", Errors = abtoErrors }],
        SipRegistered = sip
    };

    private static int Value(List<Metric> rows, MetricTypeEnum type) => rows.Single(e => e.MetricId == type).Value;

    /// <summary>
    ///     Два отчёта одного пользователя в минуте: пользователи не удваиваются, счётчики складываются </summary>
    [Fact]
    public void Test01_TwoReportsPerMinute() {
        var collector = new MetricCollector(() => _now);
        collector.AddClientReport(User1, Report(exceptions: 1, sip: true));
        _now = _now.AddSeconds(30);
        collector.AddClientReport(User1, Report(exceptions: 2, sip: false, abtoErrors: 2));
        collector.AddClientReport(User2, Report(sip: true));
        collector.AddClientReport(User3, Report());
        Assert.Empty(collector.TakeClosed());

        _now = _now.AddMinutes(1);
        var rows = collector.TakeClosed();

        Assert.Equal(3, Value(rows, MetricTypeEnum.OnlineUsers));
        Assert.Equal(3, Value(rows, MetricTypeEnum.AppExceptions));
        Assert.Equal(4, Value(rows, MetricTypeEnum.MicroSipCallCounter));
        Assert.Equal(2, Value(rows, MetricTypeEnum.AbtoPhoneCallErrorCounter));
        // SIP — по последнему отчёту: у 1 регистрация пропала, у 2 есть, 3 не сообщает
        Assert.Equal(1, Value(rows, MetricTypeEnum.AbtoPhoneUnregisteredUsers));
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

    /// <summary>
    ///     Медленные запросы App складываются за минуту; отчёт старого App без поля — ноль </summary>
    [Fact]
    public void Test05_ClientSlowRequests() {
        var collector = new MetricCollector(() => _now);
        collector.AddClientReport(User1, new ClientMetricsDto { SlowRequests = 2 });
        collector.AddClientReport(User2, new ClientMetricsDto { SlowRequests = 1 });
        collector.AddClientReport(User3, Report());

        _now = _now.AddMinutes(1);
        var rows = collector.TakeClosed();

        Assert.Equal(3, Value(rows, MetricTypeEnum.AppSlowRequests));
        Assert.Equal(3, Value(rows, MetricTypeEnum.OnlineUsers));
    }

    /// <summary>
    ///     Внешний вызов: задержка — медиана, успешные и ошибки — раздельные счётчики; у каждого сервиса свои три метрики </summary>
    [Fact]
    public void Test06_ExternalCalls() {
        var collector = new MetricCollector(() => _now);
        collector.AddExternal(MetricTypeEnum.SmsLatency, TimeSpan.FromMilliseconds(100), true);
        collector.AddExternal(MetricTypeEnum.SmsLatency, TimeSpan.FromMilliseconds(300), true);
        collector.AddExternal(MetricTypeEnum.SmsLatency, TimeSpan.FromMilliseconds(5000), false);
        collector.AddExternal(MetricTypeEnum.CarrierViewLatency, TimeSpan.FromMilliseconds(50), false);

        _now = _now.AddMinutes(1);
        var rows = collector.TakeClosed();

        Assert.Equal(300, Value(rows, MetricTypeEnum.SmsLatency));
        Assert.Equal(2, Value(rows, MetricTypeEnum.SmsCounter));
        Assert.Equal(1, Value(rows, MetricTypeEnum.SmsErrorCounter));
        Assert.Equal(1, Value(rows, MetricTypeEnum.CarrierViewErrorCounter));
        Assert.DoesNotContain(rows, e => e.MetricId == MetricTypeEnum.CarrierViewCounter);
        // шкала задержки внешних сервисов
        Assert.Equal(1, MetricConverter.GetStep(MetricTypeEnum.SmsLatency, 5000));
        // у каждого сервиса три разные метрики, тройки не пересекаются
        var all = MetricConverter.Externals.SelectMany(e => new[] { e.Latency, e.Calls, e.Errors }).ToList();
        Assert.Equal(all.Count, all.Distinct().Count());
        Assert.All(all, e => Assert.True(Enum.IsDefined(e)));
    }

    /// <summary>
    ///     Карточка Server на /Admin/Monitor: процент, дни до заполнения при среднем приросте, ступень диска </summary>
    [Fact]
    public void Test07_ServerStats() {
        var stats = new ServerStatsDto { DiskTotalMb = 1_000_000, DiskFreeMb = 600_000, DiskDayAverageMb = 1000 };
        Assert.Equal(400_000, stats.DiskUsedMb);
        Assert.Equal(40, stats.DiskPercent);
        Assert.Equal(600, stats.DaysUntilFull);
        // прироста нет или он неизвестен — дней нет
        Assert.Null(new ServerStatsDto { DiskTotalMb = 1000, DiskFreeMb = 500, DiskDayAverageMb = -10 }.DaysUntilFull);
        Assert.Null(new ServerStatsDto { DiskTotalMb = 1000, DiskFreeMb = 500 }.DaysUntilFull);
        Assert.Equal(0, MetricConverter.DiskStep(84));
        Assert.Equal(1, MetricConverter.DiskStep(85));
        Assert.Equal(2, MetricConverter.DiskStep(97));
    }
}
