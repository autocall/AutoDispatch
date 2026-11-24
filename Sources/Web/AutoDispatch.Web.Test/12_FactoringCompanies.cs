using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Factoring Companies (V1 FactoringCompany/Index) против поднятого сервера: без логина — на LogIn, список с поиском и сортировкой,
///     добавление (новый Id = max + 1), проверка обязательных полей, правка, удаление. Тестовая запись удаляется в конце </summary>
public class FactoringCompanies_Test {

    private const string TestName = "ZZ TEST FACTORING (autotest)";
    private readonly ITestOutputHelper _output;
    private readonly string _password;
    private readonly string _connectionString;

    public FactoringCompanies_Test(ITestOutputHelper output) {
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
        var anonymous = await http.GetAsync($"{AppSettings.Website}/Admin/FactoringCompanies");
        Assert.Equal(HttpStatusCode.Redirect, anonymous.StatusCode);
        Assert.Contains("/Account/LogIn", anonymous.Headers.Location?.ToString());

        var loginPage = await http.GetStringAsync($"{AppSettings.Website}/Account/LogIn");
        var login = await http.PostAsync($"{AppSettings.Website}/Account/LogIn", new FormUrlEncodedContent(new Dictionary<string, string> {
            ["__RequestVerificationToken"] = Token(loginPage), ["Email"] = AppSettings.AdminEmail, ["Password"] = _password, ["RememberMe"] = "false"
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        await ScalarAsync("DELETE FROM SetupPacketFactoringCompany WHERE Name = @name");
        try {
            // список, поиск по штату (код), сортировка
            var html = await http.GetStringAsync($"{AppSettings.Website}/Admin/FactoringCompanies");
            var total = (int)await ScalarAsync("SELECT COUNT(*) FROM SetupPacketFactoringCompany");
            _output.WriteLine($"Factoring companies: {total}");
            Assert.Equal(total, Regex.Matches(html, "hx-get=\"/Admin/FactoringCompanies/Edit/").Count);
            Assert.Contains("Factoring Companies</a>", html); // пункт меню у Admin
            var sorted = await http.GetStringAsync($"{AppSettings.Website}/Admin/FactoringCompanies?sort=city&desc=true");
            Assert.Contains("City ▼", WebUtility.HtmlDecode(sorted));

            // форма: без штата — ошибка, форма возвращается
            var form = await http.GetStringAsync($"{AppSettings.Website}/Admin/FactoringCompanies/Create");
            Assert.Contains("Select State...", form);
            var fields = new Dictionary<string, string> {
                ["EntityId"] = "0", ["CountryId"] = "0", ["Name"] = TestName, ["Address"] = "1 Test St", ["City"] = "Chicago", ["StateId"] = "",
                ["ZipCode"] = "60604", ["Email"] = "pay@example.com"
            };
            var invalid = await PostAsync(http, "/Admin/FactoringCompanies/Save", Token(form), new Dictionary<string, string>(fields));
            Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
            Assert.Contains("The State field is required.", await invalid.Content.ReadAsStringAsync());

            // добавление: Id = max + 1
            var maxId = (int)await ScalarAsync("SELECT MAX(EntityId) FROM SetupPacketFactoringCompany");
            fields["StateId"] = "32";
            var created = await PostAsync(http, "/Admin/FactoringCompanies/Save", Token(form), new Dictionary<string, string>(fields));
            Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            Assert.Equal("/Admin/FactoringCompanies", created.Headers.GetValues("HX-Redirect").First());
            var id = (int)await ScalarAsync("SELECT EntityId FROM SetupPacketFactoringCompany WHERE Name = @name");
            Assert.Equal(maxId + 1, id);

            // поиск находит её, правка меняет город
            var found = await http.GetStringAsync($"{AppSettings.Website}/Admin/FactoringCompanies?search=autotest");
            Assert.Single(Regex.Matches(found, "hx-get=\"/Admin/FactoringCompanies/Edit/"));
            var edit = await http.GetStringAsync($"{AppSettings.Website}/Admin/FactoringCompanies/Edit/{id}");
            Assert.Contains("1 Test St", edit);
            fields["EntityId"] = id.ToString();
            fields["City"] = "Hodgkins";
            Assert.Equal(HttpStatusCode.OK, (await PostAsync(http, "/Admin/FactoringCompanies/Save", Token(edit), fields)).StatusCode);
            Assert.Equal("Hodgkins", await ScalarAsync("SELECT City FROM SetupPacketFactoringCompany WHERE Name = @name"));

            // удаление
            var deleted = await PostAsync(http, $"/Admin/FactoringCompanies/Delete/{id}", Token(found), new Dictionary<string, string>());
            Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
            Assert.Null(await ScalarAsync("SELECT EntityId FROM SetupPacketFactoringCompany WHERE Name = @name"));
        } finally {
            await ScalarAsync("DELETE FROM SetupPacketFactoringCompany WHERE Name = @name");
        }
    }
}
