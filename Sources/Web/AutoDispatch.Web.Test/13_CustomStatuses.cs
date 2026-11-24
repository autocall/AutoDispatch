using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Custom Statuses (V1 CustomStatus/Index) против поднятого сервера: без логина — на LogIn, статусы своей компании (Admin — системной),
///     добавление, правка, вход App отдаёт статус в customStatuses, чужой статус не открывается, удаление. Тестовые строки удаляются в конце </summary>
public class CustomStatuses_Test {

    private const string TestName = "ZZ TEST STATUS (autotest)";
    private readonly ITestOutputHelper _output;
    private readonly string _password;
    private readonly string _connectionString;

    public CustomStatuses_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
        this._connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    private static HttpClient Http() => new(new HttpClientHandler {
        ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true,
        CookieContainer = new CookieContainer(),
        AllowAutoRedirect = false
    }) { Timeout = TimeSpan.FromSeconds(60) };

    private static string Token(string html) => Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

    private async Task<object> ScalarAsync(string sql) {
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@name", TestName);
        return await command.ExecuteScalarAsync();
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient http, string url, string token, Dictionary<string, string> fields) {
        fields["__RequestVerificationToken"] = token;
        var request = new HttpRequestMessage(HttpMethod.Post, $"{AppSettings.Website}{url}") { Content = new FormUrlEncodedContent(fields) };
        request.Headers.Add("HX-Request", "true");
        return http.SendAsync(request);
    }

    [Fact]
    public async Task Test01_Crud() {
        using var http = Http();
        var anonymous = await http.GetAsync($"{AppSettings.Website}/Admin/CustomStatuses");
        Assert.Equal(HttpStatusCode.Redirect, anonymous.StatusCode);
        Assert.Contains("/Account/LogIn", anonymous.Headers.Location?.ToString());

        var loginPage = await http.GetStringAsync($"{AppSettings.Website}/Account/LogIn");
        var login = await http.PostAsync($"{AppSettings.Website}/Account/LogIn", new FormUrlEncodedContent(new Dictionary<string, string> {
            ["__RequestVerificationToken"] = Token(loginPage), ["Email"] = AppSettings.AdminEmail, ["Password"] = _password, ["RememberMe"] = "false"
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        await ScalarAsync("DELETE FROM CompanyCustomStatus WHERE Name LIKE @name + '%'");
        try {
            var html = await http.GetStringAsync($"{AppSettings.Website}/Admin/CustomStatuses");
            Assert.Contains("Dispatch Custom Load Statuses", html);
            Assert.Contains("Custom Statuses</a>", html); // пункт меню

            // пустое имя — ошибка в форме
            var form = await http.GetStringAsync($"{AppSettings.Website}/Admin/CustomStatuses/Create");
            var invalid = await PostAsync(http, "/Admin/CustomStatuses/Save", Token(form), new() { ["EntityId"] = Guid.Empty.ToString(), ["Name"] = "" });
            Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
            Assert.Contains("The Name field is required.", await invalid.Content.ReadAsStringAsync());

            // добавление — в компанию пользователя (Admin — системная)
            var created = await PostAsync(http, "/Admin/CustomStatuses/Save", Token(form), new() { ["EntityId"] = Guid.Empty.ToString(), ["Name"] = TestName });
            Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            Assert.Equal("/Admin/CustomStatuses", created.Headers.GetValues("HX-Redirect").First());
            var id = (Guid)await ScalarAsync("SELECT EntityId FROM CompanyCustomStatus WHERE Name = @name");
            var companyId = (Guid)await ScalarAsync($"SELECT CompanyId FROM IdentityUsers WHERE Email = '{AppSettings.AdminEmail}'");
            Assert.Equal(companyId, await ScalarAsync("SELECT CompanyId FROM CompanyCustomStatus WHERE Name = @name"));
            Assert.Contains(TestName, await http.GetStringAsync($"{AppSettings.Website}/Admin/CustomStatuses"));

            // правка
            var edit = await http.GetStringAsync($"{AppSettings.Website}/Admin/CustomStatuses/Edit/{id}");
            Assert.Contains(TestName, edit);
            var renamed = TestName + " 2";
            Assert.Equal(HttpStatusCode.OK, (await PostAsync(http, "/Admin/CustomStatuses/Save", Token(edit),
                new() { ["EntityId"] = id.ToString(), ["Name"] = renamed })).StatusCode);
            Assert.Equal(renamed, await ScalarAsync($"SELECT Name FROM CompanyCustomStatus WHERE EntityId = '{id}'"));

            // App получает статус при входе — SessionDto.CustomStatuses
            var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
                new SignInViewModel { Email = AppSettings.AdminEmail, Password = _password });
            var statuses = JObject.Parse(await signIn.Content.ReadAsStringAsync())["customStatuses"]?.ToString() ?? "";
            _output.WriteLine($"SignIn customStatuses: {statuses}");
            Assert.Contains(renamed, statuses);

            // статус чужой компании не открывается и не удаляется
            var otherCompany = (Guid)await ScalarAsync($"SELECT TOP 1 Id FROM Company WHERE Id <> '{companyId}'");
            var otherId = Guid.NewGuid();
            await ScalarAsync($"INSERT INTO CompanyCustomStatus (EntityId, Name, CompanyId, CreatedDate, CreatedById, LastModifiedDate, LastModifiedById, IsDeleted) " +
                $"SELECT '{otherId}', @name + ' other', '{otherCompany}', SYSUTCDATETIME(), CreatedById, SYSUTCDATETIME(), CreatedById, 0 FROM CompanyCustomStatus WHERE EntityId = '{id}'");
            var foreign = await http.GetAsync($"{AppSettings.Website}/Admin/CustomStatuses/Edit/{otherId}");
            _output.WriteLine($"Foreign status: {foreign.StatusCode}");
            Assert.NotEqual(HttpStatusCode.OK, foreign.StatusCode);
            await PostAsync(http, $"/Admin/CustomStatuses/Delete/{otherId}", Token(edit), new());
            Assert.NotNull(await ScalarAsync($"SELECT EntityId FROM CompanyCustomStatus WHERE EntityId = '{otherId}'"));

            // удаление своего
            Assert.Equal(HttpStatusCode.OK, (await PostAsync(http, $"/Admin/CustomStatuses/Delete/{id}", Token(edit), new())).StatusCode);
            Assert.Null(await ScalarAsync($"SELECT EntityId FROM CompanyCustomStatus WHERE EntityId = '{id}'"));
        } finally {
            await ScalarAsync("DELETE FROM CompanyCustomStatus WHERE Name LIKE @name + '%'");
        }
    }
}
