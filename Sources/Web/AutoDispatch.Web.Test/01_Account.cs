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

    /// <summary>
    ///     Аудит A3: удалённый пользователь со старым JWT получает 401 на всех API App, не только в Api/Dispatch.
    ///     Пользователь AUTOCALL_TEST_EMAIL (вход мастер-паролем) на время теста помечается IsDeleted в БД сервера </summary>
    [Fact]
    public async Task Test03_DeletedUserUnauthorized() {
        var email = Environment.GetEnvironmentVariable("AUTOCALL_TEST_EMAIL");
        if (String.IsNullOrEmpty(email)) {
            _output.WriteLine("Skipped: AUTOCALL_TEST_EMAIL is not set");
            return;
        }
        using var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn", new SignInViewModel { Email = email, Password = _password });
        var session = Newtonsoft.Json.Linq.JObject.Parse(await signIn.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", (string)session["token"]);

        var requests = new Func<Task<HttpResponseMessage>>[] {
            () => http.GetAsync($"{AppSettings.Website}/Api/ActualCompany"),
            () => http.GetAsync($"{AppSettings.Website}/Api/Settings"),
            () => http.PostAsJsonAsync($"{AppSettings.Website}/Api/Census/Search", new { }),
            () => http.GetAsync($"{AppSettings.Website}/Api/Dispatch/Helpers/PlaceAutocomplete?input=Chicago"),
            () => http.PostAsJsonAsync($"{AppSettings.Website}/Api/Maps/Link", new { }),
            () => http.PostAsync($"{AppSettings.Website}/Api/Dispatch/Request", new StringContent("{}")),
        };
        async Task<int[]> CodesAsync() {
            var codes = new List<int>();
            foreach (var request in requests) {
                codes.Add((int)(await request()).StatusCode);
            }
            _output.WriteLine(String.Join(" ", codes));
            return codes.ToArray();
        }
        async Task SetDeletedAsync(bool deleted) {
            var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
            var connectionString = new ConfigurationBuilder().AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
                .AddEnvironmentVariables().Build().GetConnectionString("DefaultConnection");
            await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new Microsoft.Data.SqlClient.SqlCommand("update IdentityUsers set IsDeleted = @d where NormalizedEmail = upper(@e)", connection);
            command.Parameters.AddWithValue("@d", deleted);
            command.Parameters.AddWithValue("@e", email);
            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        Assert.DoesNotContain(401, await CodesAsync());
        await SetDeletedAsync(true);
        try {
            Assert.All(await CodesAsync(), code => Assert.Equal(401, code));
        } finally {
            await SetDeletedAsync(false);
        }
        Assert.DoesNotContain(401, await CodesAsync());
    }

    /// <summary>
    ///     Аудит B14: вошедший без нужной роли попадает на страницу AccessDenied (403), а не на 404.
    ///     У AdminEmail на время теста снимается роль Admin в БД сервера, вход — до восстановления роли </summary>
    [Fact]
    public async Task Test04_AccessDeniedPage() {
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var connectionString = new ConfigurationBuilder().AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables().Build().GetConnectionString("DefaultConnection");
        async Task<int> ExecAsync(string sql) {
            await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@e", AppSettings.AdminEmail);
            return await command.ExecuteNonQueryAsync();
        }
        const string userRole = "FROM IdentityUserRoles WHERE UserId = (SELECT Id FROM IdentityUsers WHERE NormalizedEmail = UPPER(@e)) " +
            "AND RoleId = (SELECT Id FROM IdentityRoles WHERE Name = 'Admin')";

        using var http = new HttpClient(new HttpClientHandler {
            AllowAutoRedirect = false,
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
        Assert.Equal(1, await ExecAsync("DELETE " + userRole));
        try {
            var loginPage = await http.GetStringAsync($"{AppSettings.Website}/Account/LogIn");
            var token = System.Text.RegularExpressions.Regex.Match(loginPage, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
            var login = await http.PostAsync($"{AppSettings.Website}/Account/LogIn", new FormUrlEncodedContent(new Dictionary<string, string> {
                ["__RequestVerificationToken"] = token, ["Email"] = AppSettings.AdminEmail, ["Password"] = _password, ["RememberMe"] = "false"
            }));
            Assert.Equal(System.Net.HttpStatusCode.Redirect, login.StatusCode);
        } finally {
            await ExecAsync("INSERT INTO IdentityUserRoles (UserId, RoleId) SELECT u.Id, r.Id FROM IdentityUsers u, IdentityRoles r " +
                "WHERE u.NormalizedEmail = UPPER(@e) AND r.Name = 'Admin' AND NOT EXISTS (SELECT 1 " + userRole + ")");
        }

        // роли — в cookie на момент входа: у этой сессии Admin нет
        var admin = await http.GetAsync($"{AppSettings.Website}/Admin/Users");
        Assert.Equal(System.Net.HttpStatusCode.Redirect, admin.StatusCode);
        var location = admin.Headers.Location!.ToString();
        Assert.Contains("/Account/AccessDenied", location);

        var denied = await http.GetAsync(location.StartsWith("http") ? location : AppSettings.Website + location);
        var body = await denied.Content.ReadAsStringAsync();
        _output.WriteLine($"{(int)denied.StatusCode} {location}");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, denied.StatusCode);
        Assert.Contains("Access denied", body);
    }
}
