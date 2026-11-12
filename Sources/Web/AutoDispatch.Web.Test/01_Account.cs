using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     HTTP-тесты против поднятого сервера (AppSettings.Website). Пароль — Secrets:System:Password
///     из appsettings.Development.json проекта Server либо переменной Secrets__System__Password </summary>
public class Account_Test {

    private readonly ITestOutputHelper _output;
    private readonly string _password;

    public Account_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
    }

    [Fact]
    public async Task Test01_SignIn() {
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        var response = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = AppSettings.AdminEmail, Password = _password });
        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"StatusCode: {response.StatusCode}");
        _output.WriteLine($"Response: {body}");
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("\"token\"", body);
    }

    /// <summary>
    ///     Secrets:System:Password как мастер-пароль: вход под обычным пользователем из AUTOCALL_TEST_EMAIL (без неё тест пропускается) </summary>
    [Fact]
    public async Task Test02_SignIn_MasterPassword() {
        var email = Environment.GetEnvironmentVariable("AUTOCALL_TEST_EMAIL");
        if (String.IsNullOrEmpty(email)) {
            _output.WriteLine("Skipped: AUTOCALL_TEST_EMAIL is not set");
            return;
        }
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        var response = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
            new SignInViewModel { Email = email, Password = _password });
        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"StatusCode: {response.StatusCode}");
        Assert.True(response.IsSuccessStatusCode, body);
        Assert.Contains("\"token\"", body);
        Assert.DoesNotContain("\"Admin\"", body);
    }
}
