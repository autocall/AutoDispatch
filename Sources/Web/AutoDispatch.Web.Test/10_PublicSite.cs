using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Этап 5, против поднятого сервера: страницы карт для App (Map/SelectLocation, Map/OrderStops — ссылки из Api/Maps,
///     пропуск — токен r, токены страниц не взаимозаменяемы) и публичная /Confirmation (без логина, по Guid груза) </summary>
public class PublicSite_Test {

    private readonly ITestOutputHelper _output;
    private readonly string _password;

    public PublicSite_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
    }

    private static HttpClient Http() => new(new HttpClientHandler {
        ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
    }) { Timeout = TimeSpan.FromSeconds(90) };

    private async Task<(string Token, Guid UserId)> SignInAsync(HttpClient http) {
        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = _password });
        var session = JObject.Parse(await signIn.Content.ReadAsStringAsync());
        var token = (string)session["token"];
        Assert.False(String.IsNullOrEmpty(token));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (token, (Guid)session["id"]);
    }

    [Fact]
    public async Task Test01_SelectLocation() {
        using var http = Http();
        await SignInAsync(http);

        // без Bearer закрыто
        var anonymous = await new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
            .PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/SelectLocationLink", new MapLatLngDto { Latitude = 41.85m, Longitude = -87.65m });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        // ссылка с точкой и без
        var withPoint = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/SelectLocationLink", new MapLatLngDto { Latitude = 41.85m, Longitude = -87.65m });
        Assert.Equal(HttpStatusCode.OK, withPoint.StatusCode);
        var url = (string)JObject.Parse(await withPoint.Content.ReadAsStringAsync())["url"];
        _output.WriteLine($"Select location: {url}");
        Assert.StartsWith($"{AppSettings.Website}{AppSettings.MapSelectLocationPath}?r=", url);
        var empty = await http.PostAsync($"{AppSettings.Website}/Api/Maps/SelectLocationLink", new StringContent("null", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, empty.StatusCode);

        // страница: embedded (WebView2) и браузер; точка на странице
        var page = await http.GetAsync(url + "&embedded=true");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();
        Assert.Contains("data-controller=\"select-location\"", html);
        Assert.Contains("41.85", html);
        Assert.DoesNotContain("navbar", html);
        // раскладка как у /Map: панель Map options (тип карты, Traffic, Weather, тема)
        Assert.Contains("data-controller=\"map-options\"", html);
        var browser = await http.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, browser.StatusCode);

        // подделка и пустой r — 403 текстом
        var forged = await http.GetAsync($"{AppSettings.Website}{AppSettings.MapSelectLocationPath}?r=AAAA");
        Assert.Equal(HttpStatusCode.Forbidden, forged.StatusCode);
        var noToken = await http.GetAsync($"{AppSettings.Website}{AppSettings.MapSelectLocationPath}");
        Assert.Equal(HttpStatusCode.Forbidden, noToken.StatusCode);

        // адрес точки (Safer) — улица и город раздельно; без доступа к Safer — только статус в лог
        var r = url.Substring(url.IndexOf("?r=") + 3);
        var location = await http.GetAsync($"{AppSettings.Website}/Map/Location?r={r}&lat=41.878113&lng=-87.629799");
        var body = await location.Content.ReadAsStringAsync();
        _output.WriteLine($"Location: {location.StatusCode} {body}");
        if (location.StatusCode == HttpStatusCode.OK) {
            var json = JObject.Parse(body);
            Assert.NotNull(json["formattedAddress"]);
            Assert.NotNull(json["city"]);
            Assert.NotNull(json["address"]);
        }
    }

    [Fact]
    public async Task Test02_OrderStops() {
        using var http = Http();
        var (_, userId) = await SignInAsync(http);

        var noTrailer = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/OrderStopsLink", new MapOrderStopsLinkRequest());
        Assert.Equal(HttpStatusCode.BadRequest, noTrailer.StatusCode);

        var trailerId = Guid.NewGuid();
        var link = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/OrderStopsLink", new MapOrderStopsLinkRequest { TrailerId = trailerId });
        Assert.Equal(HttpStatusCode.OK, link.StatusCode);
        var url = (string)JObject.Parse(await link.Content.ReadAsStringAsync())["url"];
        _output.WriteLine($"Order stops: {url}");
        Assert.StartsWith($"{AppSettings.Website}{AppSettings.MapOrderStopsPath}?r=", url);

        // трейлера нет — пустой список остановок, страница открывается
        var page = await http.GetAsync(url + "&embedded=true");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var html = await page.Content.ReadAsStringAsync();
        Assert.Contains("data-controller=\"order-stops\"", html);
        Assert.Contains("data-order-stops-stops-value=\"[]\"", html);
        // панель Map options и сохранение настроек токеном OrderStops (те же настройки, что пришли со страницей — без изменений)
        Assert.Contains("data-controller=\"map-options\"", html);
        var settings = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(html, "data-map-options-settings-value=\"([^\"]*)\"").Groups[1].Value);
        var token = url.Substring(url.IndexOf("?r=") + 3);
        var save = await http.PostAsync($"{AppSettings.Website}/Map/Settings", new StringContent(
            $"{{\"r\":\"{token}\",\"settings\":{settings}}}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        var forgedSave = await http.PostAsync($"{AppSettings.Website}/Map/Settings", new StringContent(
            $"{{\"r\":\"AAAA\",\"settings\":{settings}}}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.Forbidden, forgedSave.StatusCode);

        // токены страниц не взаимозаменяемы: токен OrderStops на /Map и SelectLocation — 403, токен /Map на OrderStops — 403
        var r = url.Substring(url.IndexOf("?r=") + 3);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync($"{AppSettings.Website}{AppSettings.MapPath}?r={r}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync($"{AppSettings.Website}{AppSettings.MapSelectLocationPath}?r={r}")).StatusCode);
        var mapLink = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/Link", new List<MapPointDto>());
        var mapUrl = (string)JObject.Parse(await mapLink.Content.ReadAsStringAsync())["url"];
        var mapR = mapUrl.Substring(mapUrl.IndexOf("?r=") + 3);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync($"{AppSettings.Website}{AppSettings.MapOrderStopsPath}?r={mapR}")).StatusCode);

        // Show Trip: ссылка /Map с грузом — страница с историей положений (груза нет — пусто), на OrderStops такой токен тоже 403
        var tripLink = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/Link?loadId={Guid.NewGuid()}", new List<MapPointDto> {
            new MapPointDto { Kind = MapPointKindEnum.Record, Latitude = 41.85m, Longitude = -87.65m },
            new MapPointDto { Kind = MapPointKindEnum.Record, Latitude = 39.79m, Longitude = -85.72m }
        });
        var tripUrl = (string)JObject.Parse(await tripLink.Content.ReadAsStringAsync())["url"];
        var tripPage = await http.GetAsync(tripUrl + "&embedded=true");
        Assert.Equal(HttpStatusCode.OK, tripPage.StatusCode);
        var tripHtml = await tripPage.Content.ReadAsStringAsync();
        Assert.Contains("data-map-history-value=\"true\"", tripHtml);
        Assert.Contains("41.85", tripHtml);
        var tripR = tripUrl.Substring(tripUrl.IndexOf("?r=") + 3);
        var history = await http.PostAsync($"{AppSettings.Website}/Map/History", new StringContent($"{{\"r\":\"{tripR}\"}}", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal("[]", await history.Content.ReadAsStringAsync());
        // ссылка без груза — истории нет
        Assert.Contains("data-map-history-value=\"false\"", await (await http.GetAsync(mapUrl)).Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Forbidden, (await http.GetAsync($"{AppSettings.Website}{AppSettings.MapOrderStopsPath}?r={tripR}")).StatusCode);

        // трейлер с грузами AtPickUp/InTransit и порядком остановок на Carrier-борде админа — остановки на странице
        var request = new EventRequest { Name = "LoadLoads", Data = new LoadLoadsRequestData(userId) { Types = new[] { DispatchBoardType.Carrier } } };
        var loads = DispatchJson.Deserialize<EventResponse>(await (await http.PostAsync($"{AppSettings.Website}/Api/Dispatch/Request",
            new StringContent(DispatchJson.Serialize(request), System.Text.Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync());
        _output.WriteLine($"LoadLoads: {loads.Status} {loads.ErrorMessage} {(loads.Data as LoadLoadsResponseData)?.Loads.Count}");
        var withStops = (loads.Data as LoadLoadsResponseData)?.Loads
            .FirstOrDefault(l => l.TrailerId != null && (l.Status == LoadStatus.AtPickUp || l.Status == LoadStatus.InTransit) && l.DeliveryOrder != null);
        if (withStops != null) {
            var stopsLink = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/OrderStopsLink", new MapOrderStopsLinkRequest { TrailerId = withStops.TrailerId.Value });
            var stopsUrl = (string)JObject.Parse(await stopsLink.Content.ReadAsStringAsync())["url"];
            var stopsHtml = await http.GetStringAsync(stopsUrl + "&embedded=true");
            var stopsJson = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(stopsHtml, "data-order-stops-stops-value=\"([^\"]*)\"").Groups[1].Value);
            _output.WriteLine($"Trailer {withStops.TrailerId}: {stopsJson}");
            var stops = JArray.Parse(stopsJson);
            Assert.NotEmpty(stops);
            Assert.Equal(withStops.IndexString, stops.First(s => (Guid)s["loadId"] == withStops.EntityId && (int)s["type"] == 1)["index"]);
            Assert.True(stops.Select(s => (int)s["order"]).SequenceEqual(stops.Select(s => (int)s["order"]).OrderBy(o => o)));
        } else {
            _output.WriteLine("No trailer with ordered stops on the admin's Carrier board");
        }

        // маршрут по двум точкам (Safer) — только статус в лог
        var route = await http.PostAsJsonAsync($"{AppSettings.Website}/Map/OrderStops/Route", new MapStopsRouteRequest {
            R = r,
            Points = new List<MapLatLngDto> {
                new() { Latitude = 41.878113m, Longitude = -87.629799m },
                new() { Latitude = 41.657989m, Longitude = -88.174127m }
            }
        });
        _output.WriteLine($"Route: {route.StatusCode} {(await route.Content.ReadAsStringAsync()).Truncate(200)}");
        var tooMany = await http.PostAsJsonAsync($"{AppSettings.Website}/Map/OrderStops/Route", new MapStopsRouteRequest {
            R = r,
            Points = Enumerable.Range(0, AppSettings.MapMaxStops + 1).Select(i => new MapLatLngDto { Latitude = 41 + i * 0.01m, Longitude = -87 }).ToList()
        });
        Assert.NotEqual(HttpStatusCode.OK, tooMany.StatusCode);
    }

    /// <summary>
    ///     Без груза — ошибка страницей (200, как V1), без LoadId — 400; реальный груз компании админа, если есть — rate confirmation </summary>
    [Fact]
    public async Task Test03_Confirmation() {
        using var http = Http();
        var noId = await http.GetAsync($"{AppSettings.Website}/Confirmation");
        Assert.Equal(HttpStatusCode.BadRequest, noId.StatusCode);

        var unknown = await http.GetAsync($"{AppSettings.Website}/Confirmation/?LoadId={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Contains("Something went wrong", await unknown.Content.ReadAsStringAsync());

        var download = await http.GetAsync($"{AppSettings.Website}/Confirmation/Download/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Contains("Something went wrong", await download.Content.ReadAsStringAsync());

        // подпись без antiforgery-токена отбивается
        var post = await http.PostAsync($"{AppSettings.Website}/Confirmation",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["LoadId"] = Guid.NewGuid().ToString(), ["Name"] = "John Doe" }));
        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        // груз с карриером и компанией из Carrier-борда админа
        var (_, userId) = await SignInAsync(http);
        var request = new EventRequest { Name = "LoadLoads", Data = new LoadLoadsRequestData(userId) { Types = new[] { DispatchBoardType.Carrier } } };
        var response = await http.PostAsync($"{AppSettings.Website}/Api/Dispatch/Request",
            new StringContent(DispatchJson.Serialize(request), System.Text.Encoding.UTF8, "application/json"));
        var result = DispatchJson.Deserialize<EventResponse>(await response.Content.ReadAsStringAsync());
        _output.WriteLine($"LoadLoads: {result.Status} {result.ErrorMessage} {(result.Data as LoadLoadsResponseData)?.Loads.Count}");
        var load = (result.Data as LoadLoadsResponseData)?.Loads.FirstOrDefault(l => l.DatCompanyId != null && l.ActualCompanyId != null);
        if (load == null) {
            _output.WriteLine("No load with carrier on the admin's Carrier board — page with data not checked");
            return;
        }
        var page = await http.GetAsync($"{AppSettings.Website}/Confirmation/?LoadId={load.EntityId}");
        var html = await page.Content.ReadAsStringAsync();
        _output.WriteLine($"Load {load.IndexString}: {page.StatusCode}, confirmed {load.ConfirmedDate != null}, {html.Length} chars");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        if (load.ConfirmedDate != null) {
            Assert.Contains("was signed", html);
        } else {
            Assert.Contains("Total Carrier Pay", html);
            Assert.Contains("Sign Confirmation", html);
        }
    }
}
