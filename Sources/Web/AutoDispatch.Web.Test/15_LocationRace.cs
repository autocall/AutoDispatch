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
///     Аудит B1: смена положения трейлера не должна затирать правку груза, сохранённую параллельно.
///     V1 (_modifyLocation) менял у сущности только CurrentLocation и LastModified*. Тест останавливает сервер между
///     записью положения первого и второго груза трейлера (блокировка HistoryLocationLoad), в этот момент меняет LineHaul
///     второго груза (как сохранение диспетчера) и проверяет, что после записи положения LineHaul остался.
///     Против поднятого сервера (AppSettings.Website) и его БД (ConnectionStrings:DefaultConnection); данные — клон
///     трейлера DDMK53960 и груза 13173/2 локальной копии DDM с фиксированными Id </summary>
[Collection("LocalData")] // последовательно с 16, 17, 19: временные грузы и трейлер в локальной БД
public class LocationRace_Test {

    private static readonly Guid TrailerId = Guid.Parse("E0000000-0000-0000-0000-000000000001");
    private static readonly Guid Load1Id = Guid.Parse("E0000000-0000-0000-0000-000000000011");
    private static readonly Guid Load2Id = Guid.Parse("E0000000-0000-0000-0000-000000000012");

    private readonly ITestOutputHelper _output;
    private readonly string _password;
    private readonly string _connectionString;

    public LocationRace_Test(ITestOutputHelper output) {
        this._output = output;
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();
        this._password = configuration["Secrets:System:Password"];
        this._connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    private const string CleanupSql = @"
delete from HistoryLocationLoad where LoadId in (@l1, @l2);
delete from HistoryLocationTrailer where TrailerId = @t;
delete from Load where EntityId in (@l1, @l2);
delete from Trailer where EntityId = @t;";

    private const string SetupSql = CleanupSql + @"
select * into #t from Trailer where Number = 'DDMK53960';
update #t set EntityId = @t, Number = 'TEST-B1';
insert into Trailer select * from #t;
select top 1 * into #l from Load where [Index] = 13173 and IndexExtra = '/2';
update #l set EntityId = @l1, TrailerId = @t, ParentId = null, ShipmentId = null, Board = 0, IsDeleted = 0, LineHaul = 100, IndexExtra = '/B1-1';
insert into Load select * from #l;
update #l set EntityId = @l2, IndexExtra = '/B1-2';
insert into Load select * from #l;";

    private async Task ExecAsync(string sql) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@t", TrailerId);
        command.Parameters.AddWithValue("@l1", Load1Id);
        command.Parameters.AddWithValue("@l2", Load2Id);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<(decimal? latitude, decimal? lineHaul)> ReadLoadAsync(Guid id) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("select CurrentLatitude, LineHaul from Load where EntityId = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        return (reader.IsDBNull(0) ? null : reader.GetDecimal(0), reader.IsDBNull(1) ? null : Convert.ToDecimal(reader.GetValue(1)));
    }

    private async Task<string> TrailerCompanyUserAsync() {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            "select top 1 Email from IdentityUsers where IsDeleted = 0 and CompanyId = (select CompanyId from Trailer where EntityId = @t)", connection);
        command.Parameters.AddWithValue("@t", TrailerId);
        return (string)await command.ExecuteScalarAsync();
    }

    [Fact]
    public async Task Test01_TrailerLocationKeepsParallelLoadSave() {
        await ExecAsync(SetupSql);
        try {
            using var http = new HttpClient(new HttpClientHandler {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true
            }) { Timeout = TimeSpan.FromSeconds(90) };
            // аудит D1: запрос App — только к записям своей компании, вход мастер-паролем под пользователем компании трейлера
            var signIn = await http.PostAsJsonAsync($"{AppSettings.Website}/Api/Account/SignIn",
                new SignInViewModel { Email = await TrailerCompanyUserAsync(), Password = _password });
            var session = JObject.Parse(await signIn.Content.ReadAsStringAsync());
            var userId = (Guid)session["id"];
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)session["token"]);

            // новая точка — Чикаго со случайным сдвигом (трейлер-образец — в Hodgkins), чтобы CheckChanges всегда видел изменение
            var original = (await ReadLoadAsync(Load1Id)).latitude;
            var location = new LocationDto { Latitude = 41.87f + (float)Random.Shared.NextDouble() / 100f, Longitude = -87.6298f };

            // блокировка HistoryLocationLoad: сервер запишет положение первого груза и встанет на его истории
            await using var lockConnection = new SqlConnection(_connectionString);
            await lockConnection.OpenAsync();
            var transaction = lockConnection.BeginTransaction();
            await using (var command = new SqlCommand("select top 0 * from HistoryLocationLoad with (tablockx, holdlock)", lockConnection, transaction)) {
                await command.ExecuteNonQueryAsync();
            }

            var request = new EventRequest {
                Name = "LocationTrailerChanged",
                Data = new LocationTrailerChangedRequestData(userId) { TrailerId = TrailerId, Location = location, DateTime = DateTime.UtcNow }
            };
            var content = new StringContent(DispatchJson.Serialize(request), Encoding.UTF8, "application/x-www-form-urlencoded");
            var pending = http.PostAsync($"{AppSettings.Website}/Api/Dispatch/Request", content);

            // ждём, пока положение получит ровно один груз — второй прочитан сервером, но ещё не записан
            Guid? second = null;
            for (int i = 0; i < 300 && second == null; i++) {
                await Task.Delay(100);
                Assert.False(pending.IsCompleted, "сервер не остановился на блокировке: " + (pending.IsCompleted ? await (await pending).Content.ReadAsStringAsync() : null));
                var l1 = await ReadLoadAsync(Load1Id);
                var l2 = await ReadLoadAsync(Load2Id);
                if (l1.latitude != original && l2.latitude == original) second = Load2Id;
                if (l2.latitude != original && l1.latitude == original) second = Load1Id;
            }
            Assert.NotNull(second);
            await Task.Delay(500);
            _output.WriteLine($"Сервер ждёт блокировку; второй груз {second}");

            // «диспетчер» сохраняет второй груз, пока сервер держит его старую копию
            await using (var connection = new SqlConnection(_connectionString)) {
                await connection.OpenAsync();
                await using var command = new SqlCommand("update Load set LineHaul = 777 where EntityId = @id", connection);
                command.Parameters.AddWithValue("@id", second.Value);
                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();

            var response = await pending;
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode}: {body}");
            var result = DispatchJson.Deserialize<EventResponse>(body);
            Assert.True(result.Status == ResponseStatus.Success, result.ErrorMessage);

            var after = await ReadLoadAsync(second.Value);
            _output.WriteLine($"После: CurrentLatitude {after.latitude}, LineHaul {after.lineHaul}");
            Assert.NotEqual(original, after.latitude);
            Assert.Equal(777m, after.lineHaul);
        } finally {
            await ExecAsync(CleanupSql);
        }
    }
}
