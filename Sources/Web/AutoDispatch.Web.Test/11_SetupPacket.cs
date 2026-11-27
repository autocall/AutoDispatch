using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Этап 6, против поднятого сервера: мастер CarrierRegistration без логина по Guid карриера — все включённые шаги по порядку
///     (0 → 1 → 2 → 4 → 5 → 6 → 7 → 13 → 14), формы с antiforgery и редиректом на Index, ошибки валидации, страницы агента под логином,
///     печать. Нужен карриер C0000000-…0001 с реальным MC (Safer: карточка FMCSA и сертификат) — см. AI/wiki/public-site.md </summary>
public class SetupPacket_Test {

    private const string CarrierId = "c0000000-0000-0000-0000-000000000001";
    private readonly ITestOutputHelper _output;
    private readonly string _password;
    private readonly string _connectionString;

    public SetupPacket_Test(ITestOutputHelper output) {
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
    }) { Timeout = TimeSpan.FromSeconds(120) };

    private static string Token(string html) {
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, "antiforgery token not found");
        return match.Groups[1].Value;
    }

    private static string Index => $"{AppSettings.Website}/CarrierRegistration/?CarrierId={CarrierId}";

    /// <summary>
    ///     Тест не идемпотентен без сброса: у карриера снимается packet (строка packet'а удаляется, контакты и история остаются) </summary>
    private async Task ResetAsync() {
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DECLARE @packet uniqueidentifier = (SELECT SetupPacketId FROM DatCompany WHERE EntityId = @id);" +
            " UPDATE DatCompany SET SetupPacketId = NULL, SetupPacketCompleted = 0, SetupPacketAccept = 0, W9File = NULL, SetupPacketFile = NULL WHERE EntityId = @id;" +
            " DELETE FROM SetupPacketReference WHERE SetupPacketId = @packet; DELETE FROM SetupPacket WHERE EntityId = @packet;";
        command.Parameters.AddWithValue("@id", Guid.Parse(CarrierId));
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///     Значение из БД по тестовому карриеру (@id) </summary>
    private async Task<object> ScalarAsync(string sql) {
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@id", Guid.Parse(CarrierId));
        return await command.ExecuteScalarAsync();
    }

    /// <summary>
    ///     Страница текущего шага карриера: 200 и текст, по которому шаг узнаётся </summary>
    private async Task<string> PageAsync(HttpClient http, string expected) {
        var response = await http.GetAsync(Index);
        var html = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode}");
        Assert.True(html.Contains(expected), $"expected '{expected}' on page: {Regex.Match(html, "<h2[^>]*>([^<]*)</h2>").Groups[1].Value}");
        return html;
    }

    /// <summary>
    ///     POST шага: поля + Step/Mode + antiforgery со страницы; успех — 302 на Index </summary>
    private async Task<HttpResponseMessage> PostAsync(HttpClient http, string step, string html, Dictionary<string, string> fields, MultipartFormDataContent multipart = null) {
        fields["__RequestVerificationToken"] = Token(html);
        fields.TryAdd("Step", Regex.Match(html, "name=\"Step\" value=\"(\\d+)\"").Groups[1].Value);
        fields.TryAdd("Mode", "0");
        HttpContent content;
        if (multipart != null) {
            foreach (var (key, value) in fields) multipart.Add(new StringContent(value), key);
            content = multipart;
        } else {
            content = new FormUrlEncodedContent(fields);
        }
        var response = await http.PostAsync($"{AppSettings.Website}/CarrierRegistration/{step}?CarrierId={CarrierId}", content);
        _output.WriteLine($"{step}: {response.StatusCode} {response.Headers.Location}");
        return response;
    }

    private static Dictionary<string, string> Contact(string prefix, string name, bool has = true) => new() {
        [$"{prefix}.EntityId"] = Guid.NewGuid().ToString(),
        [$"{prefix}.HasContact"] = has ? "true" : "false",
        [$"{prefix}.Contact"] = name,
        [$"{prefix}.Title"] = "Owner",
        [$"{prefix}.Phone"] = "(312) 555-0100",
        [$"{prefix}.Email"] = "test@example.com"
    };

    [Fact]
    public async Task Test01_Wizard() {
        await ResetAsync();
        using var http = Http();

        // неизвестный карриер — Error страницей, без LoadId — 400 (Guid обязателен)
        var unknown = await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/?CarrierId={Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.OK, unknown.StatusCode);
        Assert.Contains("Something went wrong", await unknown.Content.ReadAsStringAsync());

        // шаг 0: Confidant — создаёт packet (Safer: карточка FMCSA, сертификат MC)
        var html = await PageAsync(http, "Confidant");
        var invalid = await PostAsync(http, "Step00", html, Contact("ConfidantContact", ""));
        Assert.Equal(HttpStatusCode.OK, invalid.StatusCode); // ошибка валидации — та же страница
        Assert.Contains("This field is required", await invalid.Content.ReadAsStringAsync());
        // как браузер: Id контакта и ConfidantContactId — скрытые поля страницы (без ConfidantContactId ссылка в packet'е обнулялась,
        // письма при Accept шли бы в никуда — AI/wiki/app-testing.md 6.22)
        var confidant = Contact("ConfidantContact", "John Doe");
        confidant["ConfidantContact.EntityId"] = Regex.Match(html, "name=\"ConfidantContact.EntityId\" value=\"([^\"]*)\"").Groups[1].Value;
        confidant["ConfidantContactId"] = Regex.Match(html, "name=\"ConfidantContactId\" value=\"([^\"]*)\"").Groups[1].Value;
        Assert.NotEqual("", confidant["ConfidantContactId"]);
        Assert.Equal(confidant["ConfidantContact.EntityId"], confidant["ConfidantContactId"]);
        var step00 = await PostAsync(http, "Step00", html, confidant);
        if (step00.StatusCode == HttpStatusCode.OK) {
            var body = await step00.Content.ReadAsStringAsync();
            _output.WriteLine("Step00 error: " + Regex.Match(body, "alert-error[^>]*>([^<]*)<").Groups[1].Value);
        }
        Assert.Equal(HttpStatusCode.Redirect, step00.StatusCode);
        Assert.Equal(Guid.Parse(confidant["ConfidantContactId"]),
            await ScalarAsync("SELECT sp.ConfidantContactId FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));

        // шаг 1: текст, Next
        html = await PageAsync(http, "Carrier Registration Service");
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step01", html, new())).StatusCode);

        // шаг 2: адреса и контакты; previous step возвращает на 1
        html = await PageAsync(http, "Contact Information");
        _output.WriteLine(html.Contains("CARPATHIAN") ? "LegalName prefilled from FMCSA" : "LegalName not prefilled (Safer unavailable)");
        var prev = await PostAsync(http, "Prev02", html, new());
        Assert.Equal(HttpStatusCode.Redirect, prev.StatusCode);
        html = await PageAsync(http, "Carrier Registration Service");
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step01", html, new())).StatusCode);
        html = await PageAsync(http, "Contact Information");
        var step02 = new Dictionary<string, string> {
            ["LegalName"] = "Carpathian Eagle Inc", ["PhysicalAddress"] = "1 Main St", ["PhysicalCity"] = "Chicago", ["PhysicalStateId"] = "32",
            ["PhysicalZipCode"] = "60604", ["PhysicalCountryId"] = "0",
            ["UseFactoringCompany"] = "false",
            ["MailingCompanyLegalName"] = "Carpathian Eagle Inc", ["MailingAddress"] = "1 Main St", ["MailingCity"] = "Chicago", ["MailingStateId"] = "32",
            ["MailingZipCode"] = "60604", ["MailingCountryId"] = "0", ["MailingEmail"] = "pay@example.com"
        };
        foreach (var pair in Contact("CorporateContact", "John Doe")) step02[pair.Key] = pair.Value;
        foreach (var pair in Contact("SalesContact", "", has: false)) step02[pair.Key] = pair.Value;
        foreach (var pair in Contact("DispatchContact", "Jane Roe")) step02[pair.Key] = pair.Value;
        foreach (var pair in Contact("AfterHourContact", "", has: false)) step02[pair.Key] = pair.Value;
        step02["DispatchSameAs"] = "Corporate";
        // как браузер: EntityId контакта и ссылка packet'а на него (скрытые {name}Id, V1 SetupPacketStep02Model.ts) — со страницы;
        // без {name}Id ссылка обнулялась и в setup_packet.pdf оставались «SetupPacket.CorporateContact.Contact» (2026-11-24)
        foreach (var name in new[] { "CorporateContact", "SalesContact", "DispatchContact", "AfterHourContact" }) {
            var id = Regex.Match(html, $"name=\"{name}.EntityId\" value=\"([^\"]*)\"").Groups[1].Value;
            Assert.Equal(id, Regex.Match(html, $"name=\"{name}Id\" value=\"([^\"]*)\"").Groups[1].Value);
            step02[$"{name}.EntityId"] = id;
            step02[$"{name}Id"] = id;
        }
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step02", html, step02)).StatusCode);
        Assert.Equal(Guid.Parse(step02["CorporateContactId"]),
            await ScalarAsync("SELECT sp.CorporateContactId FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));
        Assert.Equal(Guid.Parse(step02["DispatchContactId"]),
            await ScalarAsync("SELECT sp.DispatchContactId FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));

        // шаг 4 (версия 2): две подписи
        html = await PageAsync(http, "BROKER - CARRIER AGREEMENT");
        // дата подписи — сегодня с первого показа шага (V1 moment(null)), не только после Next
        Assert.Equal(2, Regex.Matches(html, "value=\"[^\"]+\" placeholder=\"Signature Date\"").Count);
        var unsigned = await PostAsync(http, "Step04", html, new() { ["Agreement_02Signature"] = "" });
        Assert.Equal(HttpStatusCode.OK, unsigned.StatusCode);
        Assert.Contains("You must sign the agreement", await unsigned.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step04", html, new() { ["Agreement_02Signature"] = "John Doe", ["Agreement2_02Signature"] = "John Doe" })).StatusCode);

        // шаг 5: insurance instructions
        html = await PageAsync(http, "Insurance Instructions");
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step05", html, new())).StatusCode);

        // шаг 6: агентства; workers compensation без страховки — отказ с подписью
        html = await PageAsync(http, "Producer Requirements");
        var step06 = new Dictionary<string, string> { ["WorkersCompensationSignature"] = "John Doe",
            ["WorkersCompensationSignatureDate"] = Regex.Match(html, "name=\"WorkersCompensationSignatureDate\" value=\"([^\"]*)\"").Groups[1].Value };
        foreach (var (prefix, has) in new[] { ("GeneralLiabilityInsuranceAgency", true), ("AutoInsuranceAgency", true), ("CargoInsuranceAgency", true), ("WorkersCompensationInsuranceAgency", false) }) {
            // как браузер: EntityId агентства и ссылка packet'а на него ({prefix}Id) — со страницы
            var id = Regex.Match(html, $"name=\"{prefix}.EntityId\" value=\"([^\"]*)\"").Groups[1].Value;
            Assert.Equal(id, Regex.Match(html, $"name=\"{prefix}Id\" value=\"([^\"]*)\"").Groups[1].Value);
            step06[$"{prefix}.EntityId"] = id;
            step06[$"{prefix}Id"] = id;
            step06[$"{prefix}.HasInsurance"] = has ? "true" : "false";
            step06[$"{prefix}.CompanyName"] = has ? "Acme Insurance" : "";
            step06[$"{prefix}.Phone"] = has ? "(312) 555-0101" : "";
            step06[$"{prefix}.Number"] = has ? "POL-1" : "";
        }
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step06", html, step06)).StatusCode);
        Assert.Equal(Guid.Parse(step06["AutoInsuranceAgencyId"]),
            await ScalarAsync("SELECT sp.AutoInsuranceAgencyId FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));

        // шаг 7: W-9 — EIN в формате, согласие, подпись
        html = await PageAsync(http, "Request for Taxpayer");
        var step07 = new Dictionary<string, string> {
            ["W9Name"] = "Carpathian Eagle Inc", ["W9BusinessName"] = "Carpathian Eagle", ["W9CompanyType"] = "6", ["W9TaxClassification"] = "2",
            ["W9Address"] = "1 Main St", ["W9City"] = "Chicago", ["W9StateId"] = "32", ["W9ZipCode"] = "60604",
            ["W9EmployerIdentificationNumber"] = "12-3456789", ["W9Agree"] = "true", ["W9Signature"] = "John Doe",
            ["W9ForeignPartnersHas"] = "true", // 3b W-9 Rev. March 2024
            ["W9AgreeDate"] = Regex.Match(html, "name=\"W9AgreeDate\" value=\"([^\"]*)\"").Groups[1].Value // скрытое поле, дата в W-9
        };
        Assert.NotEqual("", step07["W9AgreeDate"]);
        // незаполненное поле — как в V1, красным «This field is required.» под полем: форма novalidate, проверяет сервер
        // (подсказка браузера была на языке браузера — замечание пользователя 2026-11-23)
        Assert.Contains("<form method=\"post\" novalidate", html);
        var noBusinessName = await PostAsync(http, "Step07", html, new Dictionary<string, string>(step07) { ["W9BusinessName"] = "" });
        Assert.Equal(HttpStatusCode.OK, noBusinessName.StatusCode);
        Assert.Contains("This field is required", await noBusinessName.Content.ReadAsStringAsync());
        // SSN и EIN вместе — ошибка (W-9 Part I: один TIN; как Safer, отступление от V1)
        var bothTin = await PostAsync(http, "Step07", html, new Dictionary<string, string>(step07) { ["W9SocialSecurityNumber"] = "123-45-6789" });
        Assert.Equal(HttpStatusCode.OK, bothTin.StatusCode);
        Assert.Contains("not both", await bothTin.Content.ReadAsStringAsync());
        var badEin = new Dictionary<string, string>(step07) { ["W9EmployerIdentificationNumber"] = "123" };
        var badEinResponse = await PostAsync(http, "Step07", html, badEin);
        Assert.Equal(HttpStatusCode.OK, badEinResponse.StatusCode);
        Assert.Contains("Must be ##-#######", await badEinResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step07", html, step07)).StatusCode);
        Assert.Equal(true, await ScalarAsync("SELECT sp.W9ForeignPartnersHas FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));
        Assert.IsType<DateTime>(await ScalarAsync("SELECT sp.W9AgreeDate FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));

        // шаг 13: документы — multipart с COI
        html = await PageAsync(http, "Upload your documents");
        var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf") } }, "fileCOI", "coi.pdf");
        Assert.Equal(HttpStatusCode.Redirect, (await PostAsync(http, "Step13", html, new(), multipart)).StatusCode);
        var coi = await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/DownloadFileCOI?CarrierId={CarrierId}");
        Assert.Equal(HttpStatusCode.OK, coi.StatusCode);
        Assert.Equal("application/pdf", coi.Content.Headers.ContentType?.MediaType);

        // шаг 14: страница шлёт форму сама; письмо (SMTP компании) локально не уходит — ошибка показывается на странице
        html = await PageAsync(http, "Thank You!");
        Assert.Contains("Processing email", html);
        var send = await PostAsync(http, "Step14", html, new());
        var sendBody = await send.Content.ReadAsStringAsync();
        _output.WriteLine($"Step14: {send.StatusCode} {Regex.Match(sendBody, "alert-error[^>]*>([^<]*)<").Groups[1].Value}");
        Assert.True(send.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.OK);
        // письма на шаге 14 нет (оно при Accept в App) — packet должен завершиться; раньше падал AccessDenied, если создателя карриера
        // не было среди пользователей (перенос берёт не всех — AI/wiki/app-testing.md 6.20)
        Assert.Equal(true, await ScalarAsync("SELECT SetupPacketCompleted FROM DatCompany WHERE EntityId = @id"));
        // контакты шага 2 пережили шаги 4–14 (их формы контактов не несут)
        Assert.Equal(Guid.Parse(step02["CorporateContactId"]),
            await ScalarAsync("SELECT sp.CorporateContactId FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id"));
        // снимок события, из которого App обновляет packet, — с контактами и агентствами, как в V1 (без агентства Accept в App давал «error»)
        var snapshot = BinaryHelper.GetString(ZipExtension.Decompress((byte[])await ScalarAsync(
            "SELECT h.Data FROM HistoryObject h JOIN SetupPacket sp ON sp.HistoryId = h.Id JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id")));
        Assert.Contains("\"CorporateContact\":{", snapshot);
        Assert.Contains("\"AutoInsuranceAgency\":{", snapshot);

        // печать без логина
        var agreement = await http.GetAsync($"{AppSettings.Website}/Print/SetupPacket/Agreement?CarrierId={CarrierId}&version=2");
        Assert.Equal(HttpStatusCode.OK, agreement.StatusCode);
        Assert.Contains("BROKER - CARRIER AGREEMENT", await agreement.Content.ReadAsStringAsync());
        var instructions = await http.GetAsync($"{AppSettings.Website}/Print/SetupPacket/InsuranceInstructions?CarrierId={CarrierId}");
        Assert.Equal(HttpStatusCode.OK, instructions.StatusCode);

        // аудит A4: завершённый packet без логина не меняется — ни шагом с поддельным Mode=Edit (2), ни Prev, ни повторным Step14
        // (раньше Mode=Edit из скрытого поля обходил проверку «завершён»); ответ — редирект на Index карриера (Finish)
        const string packetSql = "SELECT {0} FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id";
        var w9Before = await ScalarAsync(String.Format(packetSql, "sp.W9Name"));
        var packetModified = await ScalarAsync(String.Format(packetSql, "sp.LastModifiedDate"));
        var carrierModified = await ScalarAsync("SELECT LastModifiedDate FROM DatCompany WHERE EntityId = @id");
        var tokenPage = await http.GetStringAsync($"{AppSettings.Website}/Account/LogIn");
        foreach (var (action, step, mode) in new[] { ("Prev07", "7", "2"), ("Prev07", "7", "0"), ("Step07", "7", "2"), ("Step14", "14", "2") }) {
            var forged = await PostAsync(http, action, tokenPage, new() { ["Step"] = step, ["Mode"] = mode, ["W9Name"] = "HACKED" });
            Assert.Equal(HttpStatusCode.Redirect, forged.StatusCode);
            Assert.Equal($"/CarrierRegistration/?CarrierId={CarrierId}", forged.Headers.Location?.ToString());
        }
        Assert.Equal(w9Before, await ScalarAsync(String.Format(packetSql, "sp.W9Name")));
        Assert.Equal(packetModified, await ScalarAsync(String.Format(packetSql, "sp.LastModifiedDate")));
        Assert.Equal(carrierModified, await ScalarAsync("SELECT LastModifiedDate FROM DatCompany WHERE EntityId = @id"));
    }

    /// <summary>
    ///     View/Edit — только под логином сайта: без cookie — редирект на LogIn; с логином — навигация по шагам, поля View выключены </summary>
    [Fact]
    public async Task Test02_AgentPages() {
        using var http = Http();
        var anonymous = await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/View?CarrierId={CarrierId}&step=2");
        Assert.Equal(HttpStatusCode.Redirect, anonymous.StatusCode);
        Assert.Contains("/Account/LogIn", anonymous.Headers.Location?.ToString());

        var loginPage = await http.GetStringAsync($"{AppSettings.Website}/Account/LogIn");
        var login = await http.PostAsync($"{AppSettings.Website}/Account/LogIn", new FormUrlEncodedContent(new Dictionary<string, string> {
            ["__RequestVerificationToken"] = Token(loginPage), ["Email"] = AppSettings.AdminEmail, ["Password"] = _password, ["RememberMe"] = "false"
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        var view = await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/View?CarrierId={CarrierId}&step=2");
        var viewHtml = await view.Content.ReadAsStringAsync();
        _output.WriteLine($"View: {view.StatusCode}, {viewHtml.Length} chars");
        if (view.StatusCode == HttpStatusCode.OK && viewHtml.Contains("Something went wrong")) {
            _output.WriteLine("No setup packet for the test carrier — run Test01 first");
            return;
        }
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);
        Assert.Contains("<fieldset disabled", viewHtml);
        Assert.Contains("menu-active", viewHtml);
        Assert.DoesNotContain("previous step", viewHtml);

        var edit = await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/Edit?CarrierId={CarrierId}&step=2");
        var editHtml = await edit.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, edit.StatusCode);
        Assert.Contains(">Save<", editHtml);
        Assert.DoesNotContain("<fieldset disabled", editHtml);

        // шаг 13 в просмотре — без полей выбора файла, только загруженные документы; в Edit поля есть (замечание пользователя 2026-11-23)
        var view13 = await (await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/View?CarrierId={CarrierId}&step=13")).Content.ReadAsStringAsync();
        Assert.Contains("Uploaded documents", view13);
        Assert.DoesNotContain("type=\"file\"", view13);
        var edit13 = await (await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/Edit?CarrierId={CarrierId}&step=13")).Content.ReadAsStringAsync();
        Assert.Contains("type=\"file\"", edit13);

        // W-9 по версии packet'а: версия 3 — Rev. March 2024 с 3b, до 3 — Rev. November 2017, которую карриер заполнял (решено 2026-11-23)
        // версия задаётся явно: packet тестового карриера мог остаться от прогона до версии 3 (порядок Test01/Test02 не гарантирован)
        await ScalarAsync($"UPDATE sp SET Version = {AppSettings.SetupPacketVersion} FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id");
        var view7 = await (await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/View?CarrierId={CarrierId}&step=7")).Content.ReadAsStringAsync();
        Assert.Contains("Rev. March 2024", view7);
        Assert.Contains("W9ForeignPartnersHas", view7);
        await ScalarAsync("UPDATE sp SET Version = 2 FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id");
        try {
            var view7Old = await (await http.GetAsync($"{AppSettings.Website}/CarrierRegistration/View?CarrierId={CarrierId}&step=7")).Content.ReadAsStringAsync();
            Assert.Contains("Rev. November 2017", view7Old);
            Assert.DoesNotContain("W9ForeignPartnersHas", view7Old);
        } finally {
            await ScalarAsync($"UPDATE sp SET Version = {AppSettings.SetupPacketVersion} FROM SetupPacket sp JOIN DatCompany d ON d.SetupPacketId = sp.EntityId WHERE d.EntityId = @id");
        }
    }
}
