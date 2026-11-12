using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Dtos.Log;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Логи: короткий текст исключения для Details и дедупликация перед БД-синком, без сервера </summary>
public class Logging_Test {

    private static Exception Thrown(Exception ex) {
        try {
            throw ex;
        } catch (Exception e) {
            return e;
        }
    }

    private static Exception Nested() {
        try {
            try {
                throw new ArgumentException("inner");
            } catch (Exception e) {
                throw new InvalidOperationException("outer", e);
            }
        } catch (Exception e) {
            return e;
        }
    }

    /// <summary>
    ///     Цепочка Inner с типами, кадры только AutoDispatch.*, не больше MaxFrames </summary>
    [Fact]
    public void Test01_FormatChain() {
        var text = ExceptionFormatter.Format(Nested());
        var lines = text.Split(Environment.NewLine);

        Assert.Equal("InvalidOperationException: outer", lines[0]);
        Assert.Equal("<- ArgumentException: inner", lines[1]);
        var frames = lines.Skip(2).ToList();
        Assert.NotEmpty(frames);
        Assert.True(frames.Count <= ExceptionFormatter.MaxFrames);
        Assert.All(frames, f => Assert.StartsWith("   at AutoDispatch.Web.Test.", f));
        Assert.DoesNotContain("System.", text);
        // кадр самого внутреннего исключения идёт первым
        Assert.Contains("Nested", frames[0]);
    }

    /// <summary>
    ///     Код статуса в заголовке, AggregateException разворачивается, исключение без стека не ломает формат </summary>
    [Fact]
    public void Test02_FormatCodesAndAggregate() {
        var http = ExceptionFormatter.Format(Thrown(new HttpRequestException("bad gateway", null, HttpStatusCode.BadGateway)));
        Assert.StartsWith("HttpRequestException (502): bad gateway", http);

        var aggregate = ExceptionFormatter.Format(new AggregateException(new TimeoutException("t1"), new TimeoutException("t2")));
        Assert.Equal($"TimeoutException: t1{Environment.NewLine}<- TimeoutException: t2", aggregate);
    }

    /// <summary>
    ///     Warning+ одного пользователя: MaxPerWindow за окно, дальше отбрасывается, после окна снова проходит;
    ///     цифры в тексте не делают сообщения разными; другой пользователь и Information не режутся </summary>
    [Fact]
    public void Test03_Deduplicate() {
        var now = new DateTime(2026, 10, 23, 10, 0, 0, DateTimeKind.Utc);
        var dedup = new LogDeduplicator(() => now);

        for (var i = 0; i < LogDeduplicator.MaxPerWindow; i++) {
            Assert.True(dedup.IsEnabled(LogEventLevel.Error, "a@x", "app.ui", $"Search failed. Pattern {i}"));
        }
        Assert.False(dedup.IsEnabled(LogEventLevel.Error, "a@x", "app.ui", "Search failed. Pattern 99"));
        Assert.True(dedup.IsEnabled(LogEventLevel.Error, "b@x", "app.ui", "Search failed. Pattern 99"));
        Assert.True(dedup.IsEnabled(LogEventLevel.Warning, "a@x", "app.ui", "Search failed. Pattern 99"));
        for (var i = 0; i < LogDeduplicator.MaxPerWindow * 2; i++) {
            Assert.True(dedup.IsEnabled(LogEventLevel.Information, "a@x", "app.ui", "Search started"));
        }

        now += LogDeduplicator.Window;
        Assert.True(dedup.IsEnabled(LogEventLevel.Error, "a@x", "app.ui", "Search failed. Pattern 1"));
    }

    /// <summary>
    ///     Квота Api/Logs: сверх ClientLogQuota за окно пачки отбрасываются, предупреждение один раз, другой пользователь не задет,
    ///     после окна счётчик с нуля </summary>
    [Fact]
    public void Test04_ClientQuota() {
        var now = new DateTime(2026, 10, 23, 10, 0, 0, DateTimeKind.Utc);
        var quota = new ClientLogQuota(() => now);

        Assert.True(quota.TryTake(1, AppSettings.ClientLogQuota - 1, out _));
        Assert.True(quota.TryTake(1, 1, out _));
        Assert.False(quota.TryTake(1, 1, out var firstDrop));
        Assert.True(firstDrop);
        Assert.False(quota.TryTake(1, 1, out firstDrop));
        Assert.False(firstDrop);
        Assert.True(quota.TryTake(2, 1, out _));

        now += AppSettings.ClientLogQuotaWindow;
        Assert.True(quota.TryTake(1, 1, out _));
    }

    /// <summary>
    ///     Period важнее Day; без Period — сутки выбранного дня по времени сервера, переведённые в UTC </summary>
    [Fact]
    public void Test07_FilterPeriod() {
        var now = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
        var filter = new LogFilterDto { Period = "6h", Days = "2026-10-20/2026-10-22" };
        Assert.Equal(now.AddHours(-6), filter.EffectiveFrom(now));
        Assert.Null(filter.EffectiveTo());

        filter.Period = null;
        var from = filter.EffectiveFrom(now)!.Value;
        var to = filter.EffectiveTo()!.Value;
        Assert.Equal(TimeSpan.FromDays(3), to - from);
        Assert.Equal(new DateTime(2026, 10, 20), TimeZoneInfo.ConvertTimeFromUtc(from, TimeZoneInfo.Local));

        // один день, перевёрнутый порядок, мусор
        Assert.Equal((new DateTime(2026, 10, 20), new DateTime(2026, 10, 20)), new LogFilterDto { Days = "2026-10-20" }.DayRange());
        Assert.Equal((new DateTime(2026, 10, 20), new DateTime(2026, 10, 22)), new LogFilterDto { Days = "2026-10-22/2026-10-20" }.DayRange());
        Assert.Null(new LogFilterDto { Days = "garbage" }.DayRange());

        var copy = filter.Clone();
        copy.Scope.Add("auth");
        Assert.Empty(filter.Scope);
    }

    [Fact]
    public void Test05_ClientScope() {
        Assert.Equal("app.dat", LogService.ClientScope("dat"));
        Assert.Equal("app.ui", LogService.ClientScope(" app.ui "));
        Assert.Equal("app", LogService.ClientScope(null));
        Assert.Equal("app", LogService.ClientScope("app."));
        Assert.Equal(50, LogService.ClientScope(new string('x', 60)).Length);
    }

    /// <summary>
    ///     Против поднятого сервера: без токена 401, с токеном пачка принята, пустое тело 400 </summary>
    [Fact]
    public async Task Test06_ApiLogsAccepted() {
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var password = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build()["Secrets:System:Password"];
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        var batch = new ClientLogBatchDto {
            Session = "test-session",
            Entries = [new ClientLogEntryDto { Time = DateTime.UtcNow, Level = "Warning", Scope = "ui", Message = "Test entry", Details = "{\"a\":1}" }]
        };

        var anonymous = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Logs", batch);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = password });
        var token = (string)JObject.Parse(await signIn.Content.ReadAsStringAsync())["token"];
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Logs", batch);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // пустой объект — пустая пачка, 200; null — 400
        var empty = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Logs", new { });
        Assert.Equal(HttpStatusCode.OK, empty.StatusCode);
        var none = await http.PostAsync($"{AppSettings.Website}/Api/Logs", new StringContent("null", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, none.StatusCode);
    }
}
