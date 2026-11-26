using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Data.SqlClient;
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
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Аудит D1: запрос App по id получает только записи своей компании. Против поднятого сервера и его БД (ConnectionStrings:DefaultConnection);
///     вход мастер-паролем (Secrets:System:Password) </summary>
[Collection("LocalData")] // последовательно с 15, 16, 19: берёт последний груз DDM, а они создают и удаляют временные
public class CompanyIsolation_Test {

    private readonly ITestOutputHelper _output;
    private readonly string _password;
    private readonly string _connectionString;

    public CompanyIsolation_Test(ITestOutputHelper output) {
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
    ///     Админ (своя компания — системная) по id груза другой компании получает отказ; пользователь компании груза — как раньше </summary>
    [Fact]
    public async Task Test01_ForeignIdsDenied() {
        using var admin = await SignInAsync(AppSettings.AdminEmail);
        var companyId = await ScalarAsync<Guid>("SELECT CompanyId FROM IdentityUsers WHERE Id = @p", admin.UserId);
        var load = await LoadAsync("SELECT TOP 1 EntityId, HistoryId, CompanyId FROM [Load] WHERE CompanyId <> @p AND IsDeleted = 0 ORDER BY CreatedDate DESC", companyId);
        Assert.NotNull(load);
        var (loadId, historyId, loadCompanyId) = load.Value;
        _output.WriteLine($"Admin company {companyId}; load {loadId} of company {loadCompanyId}");

        // чтение и запись по чужому id — Access denied
        async Task Denied(string name, BaseEventRequestData data) {
            var response = await Request(admin.Http, name, data);
            _output.WriteLine($"{name}: {response.Status} {response.ErrorMessage}");
            Assert.Equal(ResponseStatus.Failed, response.Status);
            Assert.Equal("Access denied", response.ErrorMessage);
        }
        var histories = await ScalarAsync<int>("SELECT COUNT(*) FROM HistoryObject WHERE ReferenceId = @p", loadId);
        await Denied("LoadHistoryChangesLoad", new LoadHistoryChangesLoadRequestData(admin.UserId) { LoadId = loadId });
        await Denied("LoadHistoryLocationLoad", new LoadHistoryLocationLoadRequestData(admin.UserId) { LoadId = loadId });
        await Denied("FilesLoad", new FilesLoadRequestData(admin.UserId) { EntityId = loadId });
        await Denied("FileSave", new FileSaveRequestData(admin.UserId) { File = new FileDto { EntityId = loadId, Name = "d1.txt", Data = Encoding.ASCII.GetBytes("d1") } });
        await Denied("DeleteLoad", new DeleteLoadRequestData(admin.UserId) { LoadId = loadId });
        Assert.False(await ScalarAsync<bool>("SELECT IsDeleted FROM [Load] WHERE EntityId = @p", loadId));
        // отказ — до снимка истории
        Assert.Equal(histories, await ScalarAsync<int>("SELECT COUNT(*) FROM HistoryObject WHERE ReferenceId = @p", loadId));

        // перебор int-id снимков: чужие не возвращаются
        var foreignHistory = await Request(admin.Http, "LoadHistoryObjects", new LoadHistoryObjectsRequestData(admin.UserId) { Ids = new List<int> { historyId } });
        Assert.Equal(ResponseStatus.Success, foreignHistory.Status);
        Assert.Empty(((LoadHistoryObjectsResponseData)foreignHistory.Data).Histories);

        // пользователь компании груза — как раньше
        var email = await ScalarAsync<string>("SELECT TOP 1 Email FROM IdentityUsers WHERE CompanyId = @p AND IsDeleted = 0", loadCompanyId);
        using var owner = await SignInAsync(email);
        _output.WriteLine($"Owner {email}");
        Assert.Equal(ResponseStatus.Success, (await Request(owner.Http, "LoadHistoryChangesLoad", new LoadHistoryChangesLoadRequestData(owner.UserId) { LoadId = loadId })).Status);
        Assert.Equal(ResponseStatus.Success, (await Request(owner.Http, "LoadHistoryLocationLoad", new LoadHistoryLocationLoadRequestData(owner.UserId) { LoadId = loadId })).Status);
        Assert.Equal(ResponseStatus.Success, (await Request(owner.Http, "FilesLoad", new FilesLoadRequestData(owner.UserId) { EntityId = loadId })).Status);
        var ownHistory = await Request(owner.Http, "LoadHistoryObjects", new LoadHistoryObjectsRequestData(owner.UserId) { Ids = new List<int> { historyId } });
        Assert.Equal(ResponseStatus.Success, ownHistory.Status);
        Assert.Equal(historyId, Assert.Single(((LoadHistoryObjectsResponseData)ownHistory.Data).Histories).Id);

        // CompanyId из DTO не принимается: новая запись — в компании пользователя. Пишет владелец, не админ: события площадки —
        // в его компании, long-polling компании админа (DispatchApi_Test.Test03) их не видит
        var yard = new YardDto { EntityId = Guid.NewGuid(), CompanyId = companyId, Name = "D1 test yard", City = "", Address = "", Location = new LocationDto() };
        var saved = await Request(owner.Http, "SaveYard", new SaveYardRequestData(owner.UserId) { Yard = yard });
        _output.WriteLine($"SaveYard: {saved.Status} {saved.ErrorMessage}");
        Assert.Equal(ResponseStatus.Success, saved.Status);
        try {
            Assert.Equal(loadCompanyId, await ScalarAsync<Guid>("SELECT CompanyId FROM Yard WHERE EntityId = @p", yard.EntityId));
        } finally {
            await Request(owner.Http, "DeleteYard", new DeleteYardRequestData(owner.UserId) { YardId = yard.EntityId });
        }
    }

    #region [ private ]

    private sealed record Session(HttpClient Http, Guid UserId) : IDisposable {
        public void Dispose() => Http.Dispose();
    }

    /// <summary>
    ///     Secrets:System:Password — мастер-пароль: вход под любым пользователем </summary>
    private async Task<Session> SignInAsync(string email) {
        var http = new HttpClient(new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
        });
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

    private async Task<T> ScalarAsync<T>(string sql, object parameter) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@p", parameter);
        return (T)await command.ExecuteScalarAsync();
    }

    private async Task<(Guid id, int historyId, Guid companyId)?> LoadAsync(string sql, Guid companyId) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@p", companyId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? (reader.GetGuid(0), reader.GetInt32(1), reader.GetGuid(2)) : null;
    }

    #endregion [ private ]
}
