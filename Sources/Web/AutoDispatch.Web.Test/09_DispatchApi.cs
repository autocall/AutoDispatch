using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Таблица обработчиков без сервера и HTTP-тесты Api/Dispatch против поднятого сервера: конверт EventRequest с $type,
///     как шлёт App V1 (DispatchJson), Request/RequestZip, long-polling Response с отменой предыдущего ожидания </summary>
public class DispatchApi_Test {

    private readonly ITestOutputHelper _output;
    private readonly string _password;

    public DispatchApi_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
    }

    /// <summary>
    ///     Все методы IDispatchService с одним *RequestData попали в таблицу; EventNotificationAsync — нет; роли из MinAccess </summary>
    [Fact]
    public void Test01_Handlers() {
        var handlers = DispatchRequestHandlers.All;
        _output.WriteLine($"Handlers: {handlers.Count}");
        var expected = typeof(IDispatchService).GetMethods()
            .Count(m => m.GetParameters().Length == 1 && typeof(BaseEventRequestData).IsAssignableFrom(m.GetParameters()[0].ParameterType));
        Assert.Equal(expected, handlers.Count);
        Assert.False(handlers.ContainsKey(nameof(IDispatchService.EventNotificationAsync)));
        Assert.Equal(typeof(GetModulesRequestData), handlers["GetModules"].RequestType);
        Assert.Null(handlers["GetModules"].MinAccess);

        var deleteLoad = handlers["DeleteLoad"];
        Assert.Equal(Data.Enums.RoleEnum.Owner, deleteLoad.MinAccess.Role);
        Assert.True(deleteLoad.HasAccess(new[] { Data.Enums.RoleEnum.Owner }));
        Assert.True(deleteLoad.HasAccess(new[] { Data.Enums.RoleEnum.Admin }));
        Assert.False(deleteLoad.HasAccess(new[] { Data.Enums.RoleEnum.Dispatcher }));
        // ExtraRoles: ConfirmationSend — Dispatcher и выше, плюс Agent
        var confirmation = handlers["ConfirmationSend"];
        Assert.True(confirmation.HasAccess(new[] { Data.Enums.RoleEnum.Agent }));
        Assert.False(confirmation.HasAccess(new[] { Data.Enums.RoleEnum.Warehouse }));
    }

    /// <summary>
    ///     Binder: $type только из AutoDispatch.Dtos и core-коллекций </summary>
    [Fact]
    public void Test02_TypeWhitelist() {
        var json = DispatchJson.Serialize(new EventRequest { Name = "GetModules", Data = new GetModulesRequestData(Guid.NewGuid()) });
        _output.WriteLine(json);
        Assert.Contains("AutoDispatch.Dtos.Data.GetModulesRequestData, AutoDispatch.Dtos", json);
        var request = DispatchJson.Deserialize<EventRequest>(json);
        Assert.IsType<GetModulesRequestData>(request.Data);

        var evil = json.Replace("AutoDispatch.Dtos.Data.GetModulesRequestData, AutoDispatch.Dtos", "System.Diagnostics.Process, System.Diagnostics.Process");
        Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => DispatchJson.Deserialize<EventRequest>(evil));
    }

    [Fact]
    public async Task Test03_RequestAndResponse() {
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        }) { Timeout = TimeSpan.FromSeconds(90) };
        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = _password });
        var session = JObject.Parse(await signIn.Content.ReadAsStringAsync());
        var token = (string)session["token"];
        var userId = (Guid)session["id"];
        Assert.False(String.IsNullOrEmpty(token));

        // без Bearer закрыто
        var anonymous = await Post(http, "Request", new EventRequest { Name = "GetModules", Data = new GetModulesRequestData(userId) });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Request: GetModules
        var modules = await Read(await Post(http, "Request", new EventRequest { Name = "GetModules", Data = new GetModulesRequestData(userId) }));
        Assert.Equal(ResponseStatus.Success, modules.Status);
        Assert.Equal(4, ((GetModulesResponseData)modules.Data).Modules.Count);

        // чужой UserId в Data — Access denied, неизвестное имя — Failed
        var foreign = await Read(await Post(http, "Request", new EventRequest { Name = "GetModules", Data = new GetModulesRequestData(Guid.NewGuid()) }));
        Assert.Equal(ResponseStatus.Failed, foreign.Status);
        Assert.Equal("Access denied", foreign.ErrorMessage);
        var unknown = await Read(await Post(http, "Request", new EventRequest { Name = "NoSuchMethod", Data = new GetModulesRequestData(userId) }));
        Assert.Equal(ResponseStatus.Failed, unknown.Status);
        _output.WriteLine($"Unknown: {unknown.ErrorMessage}");

        // RequestZip: тот же конверт gzip-ом
        var zipBody = new ByteArrayContent(ZipExtension.SerializeObject(new EventRequest { Name = "GetModules", Data = new GetModulesRequestData(userId) }));
        zipBody.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        var zipResponse = await http.PostAsync($"{AppSettings.Website}/Api/Dispatch/RequestZip", zipBody);
        Assert.Equal(HttpStatusCode.OK, zipResponse.StatusCode);
        var zipResult = ZipExtension.DeserializeObject<EventResponse>(await zipResponse.Content.ReadAsByteArrayAsync());
        Assert.Equal(ResponseStatus.Success, zipResult.Status);

        // Response без EventId — текущий Id компании сразу, без ожидания
        var stopwatch = Stopwatch.StartNew();
        var first = await Read(await Post(http, "Response", new EventRequest { Data = new EventNotificationRequestData(userId, null, new[] { (byte)EventTypeDto.Driver }) }));
        _output.WriteLine($"Response(null) in {stopwatch.ElapsedMilliseconds} ms: EventId {first.EventId}, {first.ErrorMessage}");
        Assert.Equal(ResponseStatus.Success, first.Status);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
        // EmptyEventDto.EventId — [JsonIgnore] по контракту V1: текущий id — в конверте (first.EventId), в событии всегда 0
        var events = ((EventNotificationResponseData)first.Data).Events;
        Assert.Single(events);
        var currentEventId = first.EventId;

        // второй опрос того же пользователя отменяет первый: первый — Failed, второй досиживает EventWaitTimeout и отдаёт пустое событие
        stopwatch.Restart();
        var waiting = Post(http, "Response", new EventRequest { Data = new EventNotificationRequestData(userId, currentEventId, new[] { (byte)EventTypeDto.Driver }) });
        await Task.Delay(500);
        var second = await Read(await Post(http, "Response", new EventRequest { Data = new EventNotificationRequestData(userId, currentEventId, new[] { (byte)EventTypeDto.Driver }) }));
        var cancelled = await Read(await waiting);
        _output.WriteLine($"Cancelled: {cancelled.Status} {cancelled.ErrorMessage}; second in {stopwatch.ElapsedMilliseconds} ms: {second.Status}");
        Assert.Equal(ResponseStatus.Failed, cancelled.Status);
        Assert.Equal(currentEventId, cancelled.EventId);
        Assert.Equal(ResponseStatus.Success, second.Status);
        Assert.Equal(currentEventId, second.EventId);
        Assert.True(stopwatch.Elapsed >= AppSettings.EventWaitTimeout - TimeSpan.FromSeconds(1));
    }

    /// <summary>
    ///     Остальные эндпоинты App на маршрутах V3 (решено 2026-11-17) и расширенный SessionDto; Distance (Google) — только статус в лог </summary>
    [Fact]
    public async Task Test04_Helpers() {
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        }) { Timeout = TimeSpan.FromSeconds(90) };
        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = _password });
        var sessionBody = await signIn.Content.ReadAsStringAsync();
        _output.WriteLine($"Session: {sessionBody}");
        var session = JObject.Parse(sessionBody);
        Assert.NotNull(session["hasBnsfRailway"]);
        Assert.NotNull(session["customStatuses"]);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)session["token"]);

        async Task<string> Get(string url) {
            var response = await http.GetAsync($"{AppSettings.Website}/{url}");
            var body = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"GET {url}: {response.StatusCode} {(body.Length > 300 ? body[..300] + "…" : body)}");
            Assert.True(response.IsSuccessStatusCode, body);
            return body;
        }
        async Task<string> PostJson(string url, object data) {
            var response = await http.PostAsync($"{AppSettings.Website}/{url}", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"POST {url}: {response.StatusCode} {(body.Length > 300 ? body[..300] + "…" : body)}");
            Assert.True(response.IsSuccessStatusCode, body);
            return body;
        }

        Assert.IsType<JArray>(JToken.Parse(await PostJson("Api/Dispatch/Helpers/SearchLoads", new Dictionary<string, string> { ["City"] = "Dal" })));
        Assert.IsType<JArray>(JToken.Parse(await PostJson("Api/Dispatch/Helpers/SearchLoads", new Dictionary<string, string> { ["Company"] = "A" })));
        Assert.IsType<JArray>(JToken.Parse(await PostJson("Api/Dispatch/Helpers/SearchCustomers", "a")));
        Assert.IsType<JArray>(JToken.Parse(await Get("Api/ActualCompany")));
        Assert.IsType<JArray>(JToken.Parse(await Get("Api/SetupPacket/FactoringCompanies")));

        var predictions = JObject.Parse(await Get("Api/Dispatch/Helpers/PlaceAutocomplete?search=Dalla"));
        Assert.Equal("OK", (string)predictions["status"]);
        var placeId = (string)predictions["predictions"].First()["place_id"];
        var details = JObject.Parse(await Get($"Api/Dispatch/Helpers/PlaceDetails?placeId={Uri.EscapeDataString(placeId)}"));
        Assert.Equal("OK", (string)details["status"]);

        var fields = JObject.Parse(await Get("Api/DatCompany/Search?dot=2214091"));
        Assert.True(fields.Count > 0);
        Assert.NotNull(fields["Legal Name:"]);
        Assert.IsType<JArray>(JToken.Parse(await PostJson("Api/Census/Search", new CensusCompanySearchDto { Limit = 5, NAME = "WALMART" })));

        var distance = await http.GetAsync($"{AppSettings.Website}/Api/Dispatch/Helpers/Distance?lat1=32.78&lng1=-96.8&lat2=29.76&lng2=-95.37");
        _output.WriteLine($"Distance: {distance.StatusCode} {await distance.Content.ReadAsStringAsync()}");
        Assert.NotEqual(HttpStatusCode.Unauthorized, distance.StatusCode);
    }

    /// <summary>
    ///     Как App V1: JSON с $type, Content-Type x-www-form-urlencoded </summary>
    private static Task<HttpResponseMessage> Post(HttpClient http, string action, EventRequest request) {
        var content = new StringContent(DispatchJson.Serialize(request), Encoding.UTF8, "application/x-www-form-urlencoded");
        return http.PostAsync($"{AppSettings.Website}/Api/Dispatch/{action}", content);
    }

    private async Task<EventResponse> Read(HttpResponseMessage response) {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode}: {body}");
        return DispatchJson.Deserialize<EventResponse>(body);
    }
}
