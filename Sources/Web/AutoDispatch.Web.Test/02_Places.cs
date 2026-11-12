using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     HTTP-тесты Api/Places против поднятого сервера: подсказки по строке, детали по placeId </summary>
public class Places_Test {

    private readonly ITestOutputHelper _output;
    private readonly string _password;

    public Places_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
    }

    [Fact]
    public async Task Test01_SearchAndDetails() {
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = _password });
        var token = (string)JObject.Parse(await signIn.Content.ReadAsStringAsync())["token"];
        Assert.False(String.IsNullOrEmpty(token));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var search = await http.GetStringAsync($"{AppSettings.Website}/Api/Places?search=Dalla");
        _output.WriteLine($"Search: {search}");
        var places = JArray.Parse(search);
        Assert.NotNull(places);
        Assert.Contains(places, p => (string)p["name"] == "Dallas" && (string)p["state"] == "TX");

        var placeId = (string)places.First(p => (string)p["name"] == "Dallas" && (string)p["state"] == "TX")["placeId"];
        var details = await http.GetStringAsync($"{AppSettings.Website}/Api/Places/{placeId}");
        _output.WriteLine($"Details: {details}");
        var place = JObject.Parse(details);
        Assert.Equal("Dallas", (string)place["name"]);
        Assert.Equal("TX", (string)place["state"]);
        Assert.NotEqual(0, (float)place["latitude"]);

        // повторный запрос — из кэша Safer
        var second = await http.GetStringAsync($"{AppSettings.Website}/Api/Places?search=Dalla");
        Assert.Equal(search, second);
    }
}
