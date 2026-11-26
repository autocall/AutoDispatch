using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using AutoMapper;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Mapping;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Mapping;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Ошибки V1, перенесённые как есть ([[audit]] D2, D5, D6). Против БД сервера (ConnectionStrings:DefaultConnection), D5 и D6 — и против
///     поднятого сервера; вход мастер-паролем под пользователем компании DDM (единственная с грузами в локальной копии) </summary>
[Collection("LocalData")] // последовательно с 15, 16, 17: ServiceLocator статический, тесты создают и удаляют грузы в локальной БД
public class V1Bugs_Test {

    private static readonly Guid DdmCompanyId = Guid.Parse("DF578A7C-71C4-426A-9CC1-A578D61AD492");

    private readonly ITestOutputHelper _output;
    private readonly string _password;
    private readonly string _connectionString;

    public V1Bugs_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
        this._connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    /// <summary>
    ///     D2: строка кастомизации другой компании dispatch, подставленная в форму, не перезаписывается и не переезжает.
    ///     DispatchCompanyService.SaveAsync — как страница Admin/DispatchCompanies после проверки доступа к своей компании </summary>
    [Fact]
    public async Task Test01_ForeignCustomizeNotHijacked() {
        var dispatchCompanies = await ListAsync<Guid>("SELECT TOP 2 EntityId FROM DispatchCompany WHERE CompanyId = @p AND IsDeleted = 0 ORDER BY EntityId", DdmCompanyId);
        Assert.Equal(2, dispatchCompanies.Count);
        var (own, foreign) = (dispatchCompanies[0], dispatchCompanies[1]);
        var rowId = Guid.NewGuid();
        await ExecAsync(@"INSERT INTO DispatchCompanyCustom (EntityId, DispatchCompanyId, Type, Name, Params, IsDeleted, CreatedById, CreatedDate, LastModifiedById, LastModifiedDate)
VALUES (@id, @foreign, 0, 'D2 foreign', 'F', 0, @user, GETUTCDATE(), @user, GETUTCDATE())",
            ("@id", rowId), ("@foreign", foreign), ("@user", AdUser.SystemUserId));
        try {
            var services = new ServiceCollection();
            services.AddLinqToDBContext<LinqToDbContext>((provider, options) => options.UseSqlServer(_connectionString));
            services.AddLinqToDBContext<LogLinqToDbContext>((provider, options) => options.UseSqlServer(_connectionString));
            services.AddScoped<UnitOfWork>();
            var mapper = new MapperConfiguration(conf => {
                conf.AddProfile(new LogicMappingProfile());
                conf.AddProfile(new ViewMappingProfile());
                conf.AddProfile(new DispatchMappingProfile());
            }).CreateMapper();
            services.AddScoped(provider => new ServicesHost(provider.GetRequiredService<UnitOfWork>(), mapper, provider));
            await using var provider = services.BuildServiceProvider();
            ServiceLocator.Initialize(provider); // кэши имён (LoadCompanyNames и т. п.)
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ServicesHost>().GetService<DispatchCompanyService>();

            var dto = await service.GetByIdAsync(own);
            dto.Customizes = new List<ActualCompanyCustomDto> {
                new() { EntityId = rowId, ActualCompanyId = own, Type = 0, Name = "D2 hijacked", Params = "H" }
            };
            await service.SaveAsync(dto, AdUser.SystemUserId);

            var owner = await ListAsync<Guid>("SELECT DispatchCompanyId FROM DispatchCompanyCustom WHERE EntityId = @p", rowId);
            var name = await ListAsync<string>("SELECT Name FROM DispatchCompanyCustom WHERE EntityId = @p", rowId);
            _output.WriteLine($"Row {rowId}: company {String.Join(",", owner)} (foreign {foreign}, own {own}), name {String.Join(",", name)}");
            Assert.Equal(new[] { foreign }, owner);
            Assert.Equal(new[] { "D2 foreign" }, name);
        } finally {
            await ExecAsync("DELETE FROM DispatchCompanyCustom WHERE EntityId = @id OR (DispatchCompanyId = @own AND Name LIKE 'D2 %')", ("@id", rowId), ("@own", own));
        }
    }

    /// <summary>
    ///     D5: новый подгруз без IndexExtra у нового груза получает «/1», а не падает (V1: фильтр `!=` брал сам груз с пустым IndexExtra) </summary>
    [Fact]
    public async Task Test02_SubloadIndexExtra() {
        using var owner = await SignInDdmAsync();
        // полный LoadDto — из текущего снимка истории груза DDM
        var historyId = (await ListAsync<int>("SELECT TOP 1 HistoryId FROM [Load] WHERE CompanyId = @p AND IsDeleted = 0 AND ParentId IS NULL AND Board = 0 " +
            "AND OriginLatitude IS NOT NULL AND DestinationLatitude IS NOT NULL ORDER BY CreatedDate DESC", DdmCompanyId)).Single();
        var histories = await Request(owner.Http, "LoadHistoryObjects", new LoadHistoryObjectsRequestData(owner.UserId) { Ids = new List<int> { historyId } });
        var data = Assert.Single(((LoadHistoryObjectsResponseData)histories.Data).Histories).Data;
        LoadDto Clone() {
            var dto = ZipExtension.DeserializeObject<LoadDto>(data);
            dto.EntityId = Guid.NewGuid();
            dto.HistoryId = 0;
            dto.TrackerId = null;
            dto.ShipmentId = null;
            dto.Index = null;
            dto.IndexPrefix = null;
            return dto;
        }
        var parent = Clone();
        parent.ParentId = null;
        parent.IndexExtra = $"Z{Random.Shared.Next(1000000, 9999999)}"; // номер из IndexExtra — счётчик компании не трогается
        var child = Clone();
        child.ParentId = parent.EntityId;
        child.IndexExtra = null;
        try {
            var saved = await Request(owner.Http, "SaveLoads", new SaveLoadsRequestData(owner.UserId) { Loads = new List<LoadDto> { parent, child } });
            _output.WriteLine($"SaveLoads: {saved.Status} {saved.ErrorMessage}");
            Assert.Equal(ResponseStatus.Success, saved.Status);
            var savedChild = ((SaveLoadsResponseData)saved.Data).Loads.First(e => e.EntityId == child.EntityId);
            _output.WriteLine($"Subload: {savedChild.Index}{savedChild.IndexExtra}");
            Assert.Equal("/1", savedChild.IndexExtra);
        } finally {
            await ExecAsync("DELETE FROM HistoryLocationLoad WHERE LoadId IN (@p, @c); DELETE FROM [Load] WHERE EntityId IN (@p, @c)",
                ("@p", parent.EntityId), ("@c", child.EntityId));
        }
    }

    /// <summary>
    ///     D6: история грузов (LoadHistory2) — Total без удалённых, страница полная, номер страницы за концом — последняя страница </summary>
    [Fact]
    public async Task Test03_LoadHistoryPaging() {
        using var owner = await SignInDdmAsync();
        const string roots = "SELECT COUNT(*) FROM [Load] WHERE CompanyId = @p AND (Board = 2 OR Board = 0) AND ParentId IS NULL";
        var alive = (await ListAsync<int>(roots + " AND IsDeleted = 0", DdmCompanyId)).Single();
        var all = (await ListAsync<int>(roots, DdmCompanyId)).Single();
        _output.WriteLine($"Roots: alive {alive}, with deleted {all}");
        Assert.True(all > alive, "в локальной копии нет удалённых грузов — D6 не проверить");

        const int pageSize = 50;
        async Task<LoadHistory2ResponseData> Page(int index) {
            var response = await Request(owner.Http, "LoadHistory2", new LoadHistory2RequestData(owner.UserId) {
                Request = new RequestHistory2Dto { PageIndex = index, PageSize = pageSize }
            });
            Assert.Equal(ResponseStatus.Success, response.Status);
            return (LoadHistory2ResponseData)response.Data;
        }
        var first = await Page(0);
        var firstRoots = first.Loads.Count(e => e.ParentId == null);
        _output.WriteLine($"Total {first.Total}, page 0 roots {firstRoots}");
        Assert.Equal(alive, first.Total);
        Assert.Equal(Math.Min(pageSize, alive), firstRoots);

        var lastPage = (alive - 1) / pageSize;
        var beyond = await Page(lastPage + 10);
        var beyondRoots = beyond.Loads.Count(e => e.ParentId == null);
        _output.WriteLine($"Page {lastPage + 10} → PageIndex {beyond.PageIndex}, roots {beyondRoots}");
        Assert.Equal(lastPage, beyond.PageIndex);
        Assert.Equal(alive - lastPage * pageSize, beyondRoots);
    }

    /// <summary>
    ///     D22: SMTP, который принял соединение и молчит, не держит отправку бесконечно — в .NET 10 SmtpClient.Timeout (SmtpSettings.Timeout)
    ///     действует и на SendMailAsync: SmtpException «timed out». Без сервера: локальный приёмник в тесте </summary>
    [Fact]
    public async Task Test04_SmtpTimeout() {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync(); // соединение принимается, ответа нет
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try {
            var email = new AutoDispatch.Web.Logic.Email.Email("carrier@example.com", "company@example.com", "Timeout", "test");
            var settings = new AutoDispatch.Web.Logic.Email.SmtpSettings("user", "password", "127.0.0.1", port) { EnableSsl = false, Timeout = 1000 };
            var e = await Assert.ThrowsAsync<System.Net.Mail.SmtpException>(() => AutoDispatch.Web.Logic.Email.EmailUtils.SendEmailAsync(settings, email));
            _output.WriteLine($"{stopwatch.ElapsedMilliseconds} ms: {e.Message}");
            Assert.Contains("timed out", e.Message);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
        } finally {
            listener.Stop();
            (await accepted).Dispose();
        }
    }

    #region [ private ]

    private sealed record Session(HttpClient Http, Guid UserId) : IDisposable {
        public void Dispose() => Http.Dispose();
    }

    private async Task<Session> SignInDdmAsync() {
        var email = (await ListAsync<string>("SELECT TOP 1 Email FROM IdentityUsers WHERE CompanyId = @p AND IsDeleted = 0", DdmCompanyId)).Single();
        var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        }) { Timeout = TimeSpan.FromSeconds(120) };
        var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn", new SignInViewModel { Email = email, Password = _password });
        var session = JObject.Parse(await signIn.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)session["token"]);
        return new Session(http, (Guid)session["id"]);
    }

    private static async Task<EventResponse> Request(HttpClient http, string name, BaseEventRequestData data) {
        var content = new StringContent(DispatchJson.Serialize(new EventRequest { Name = name, Data = data }), Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await http.PostAsync($"{AppSettings.Website}/Api/Dispatch/Request", content);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode}: {body}");
        return DispatchJson.Deserialize<EventResponse>(body);
    }

    private async Task<List<T>> ListAsync<T>(string sql, object parameter) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@p", parameter);
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<T>();
        while (await reader.ReadAsync()) {
            result.Add((T)reader.GetValue(0));
        }
        return result;
    }

    private async Task ExecAsync(string sql, params (string name, object value)[] parameters) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters) {
            command.Parameters.AddWithValue(name, value);
        }
        await command.ExecuteNonQueryAsync();
    }

    #endregion [ private ]
}
