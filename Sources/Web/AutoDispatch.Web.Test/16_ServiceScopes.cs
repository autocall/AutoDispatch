using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using AutoDispatch.Web.Data;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Аудит B7 (ServicesHost.GetService из параллельных задач) и B8 (UserNameCache не освобождал scope и соединение).
///     В процессе теста, без поднятого сервера; B8 — локальная БД из appsettings.Development.json проекта Server </summary>
public class ServiceScopes_Test {

    private readonly ITestOutputHelper _output;

    public ServiceScopes_Test(ITestOutputHelper output) {
        this._output = output;
    }

    /// <summary>
    ///     Сервис с медленным конструктором — окно гонки «ContainsKey → создать → записать» </summary>
    public class ProbeService : BaseService {
        public static int Created;

        public ProbeService(ServicesHost host) : base(host) {
            Interlocked.Increment(ref Created);
            Thread.Sleep(5);
        }
    }

    /// <summary>
    ///     B7: 32 потока одновременно запрашивают один сервис у одного хоста — экземпляр один, исключений нет </summary>
    [Fact]
    public void Test01_GetServiceFromParallelTasks() {
        for (var round = 0; round < 20; round++) {
            var host = new ServicesHost(null, null, null);
            ProbeService.Created = 0;
            using var start = new Barrier(32);
            var results = new ProbeService[32];
            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            // свои потоки, не пул: барьер на 32 в пуле ждёт, пока пул медленно добавит потоки
            var threads = Enumerable.Range(0, 32).Select(i => new Thread(() => {
                start.SignalAndWait();
                try {
                    results[i] = host.GetService<ProbeService>();
                } catch (Exception e) {
                    errors.Add(e);
                }
            })).ToList();
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
            Assert.Empty(errors);
            Assert.Equal(1, ProbeService.Created);
            Assert.All(results, r => Assert.Same(results[0], r));
        }
    }

    /// <summary>
    ///     B8: промахи кэша имён не оставляют открытых соединений — сессии SQL Server с уникальным Application Name
    ///     после 20 промахов: одна (соединение вернулось в пул), а не 20 </summary>
    [Fact]
    public void Test02_UserNameCacheReleasesConnections() {
        var serverDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AutoDispatch.Web.Server"));
        var connectionString = new ConfigurationBuilder().AddJsonFile(Path.Combine(serverDir, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables().Build().GetConnectionString("DefaultConnection");
        var appName = "B8Test-" + Guid.NewGuid().ToString("N");
        var testConnectionString = new SqlConnectionStringBuilder(connectionString) { ApplicationName = appName }.ConnectionString;

        var services = new ServiceCollection();
        services.AddLinqToDBContext<LinqToDbContext>((provider, options) => options.UseSqlServer(testConnectionString));
        services.AddLinqToDBContext<LogLinqToDbContext>((provider, options) => options.UseSqlServer(testConnectionString));
        services.AddScoped<UnitOfWork>();
        services.AddScoped(provider => new ServicesHost(provider.GetRequiredService<UnitOfWork>(), null, provider));
        using var provider = services.BuildServiceProvider();
        ServiceLocator.Initialize(provider);

        try {
            for (var i = 0; i < 20; i++) {
                Assert.Null(UserNameCache.Get(Guid.NewGuid()));
            }
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var command = new SqlCommand("select count(*) from sys.dm_exec_sessions where program_name = @app", connection);
            command.Parameters.AddWithValue("@app", appName);
            var sessions = (int)command.ExecuteScalar();
            _output.WriteLine($"sessions: {sessions}");
            Assert.True(sessions <= 1, $"{sessions} connections left open");
        } finally {
            SqlConnection.ClearAllPools();
        }
    }
}
