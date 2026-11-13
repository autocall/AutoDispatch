using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using AutoMapper;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.MSSqlServer;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Identity;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Mapping;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Server.BackgroundServices;
using AutoDispatch.Web.Server.Identity;
using AutoDispatch.Web.Server.Infrastructure;
using AutoDispatch.Web.Server.Mapping;

namespace AutoDispatch.Web.Server;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        AppSettings.Init(key => builder.Configuration[key]);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var logConnectionString = builder.Configuration.GetConnectionString("LogConnection");

        ConfigureSerilog(logConnectionString);
        builder.Host.UseSerilog();
        _l.Init(new CommonLogger(Log.Logger));

        var services = builder.Services;

        services.AddDbContext<AdDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.CommandTimeout((int)AppSettings.SqlTimeout.TotalSeconds)));
        services.AddDbContext<AdLogDbContext>(options =>
            options.UseSqlServer(logConnectionString, sql => sql.CommandTimeout((int)AppSettings.SqlTimeout.TotalSeconds)));
        services.AddLinqToDBContext<LinqToDbContext>((provider, options) => options.UseSqlServer(connectionString));
        services.AddLinqToDBContext<LogLinqToDbContext>((provider, options) => options.UseSqlServer(logConnectionString));

        #region [ Identity: cookie для сайта, Bearer для Api/* ]

        services.AddIdentity<AdUser, AdRole>()
            .AddEntityFrameworkStores<AdDbContext>()
            .AddDefaultTokenProviders();
        services.AddTransient<IUserStore<AdUser>, AdUserStore>();
        services.AddTransient<IRoleStore<AdRole>, AdRoleStore>();
        // в cookie добавляются CompanyId и TimeZoneId — те же claims, что в JWT
        services.AddScoped<IUserClaimsPrincipalFactory<AdUser>, AdClaimsPrincipalFactory>();

        services.Configure<IdentityOptions>(options => {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = AppSettings.PasswordMinLength;
            options.SignIn.RequireConfirmedEmail = false;
            options.User.RequireUniqueEmail = true;
        });
        services.ConfigureApplicationCookie(options => {
            options.Cookie.Name = AppSettings.CookieName;
            options.LoginPath = AppSettings.LogInPath;
            options.ExpireTimeSpan = AppSettings.CookieLifetime;
            options.SlidingExpiration = true;
        });
        // ключи cookie переживают пересоздание контейнера — volume /keys (AI/wiki/deploy.md)
        services.AddDataProtection()
            .SetApplicationName(AppSettings.ApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(AppSettings.KeysPath));

        // схема по умолчанию остаётся cookie (AddIdentity); API-контроллеры помечены [Authorize(AuthenticationSchemes = "Bearer")]
        services.AddAuthentication().AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.JwtPrivateKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
        services.AddAuthorization();

        #endregion [ Identity: cookie для сайта, Bearer для Api/* ]

        // за nginx: реальный IP и схема клиента из X-Forwarded-*; приложение доступно только через прокси
        services.Configure<ForwardedHeadersOptions>(options => {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddControllersWithViews().AddNewtonsoftJson();
        // API: невалидная модель отвечает JsonFail(GetErrors()), а не автоматическим ProblemDetails
        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        services.AddExceptionHandler<ExceptionHandler>();

        var mappingConfig = new MapperConfiguration(conf => {
            conf.AddProfile(new LogicMappingProfile());
            conf.AddProfile(new ViewMappingProfile());
            conf.AllowNullCollections = true;
        });
        services.AddSingleton(mappingConfig.CreateMapper());
        services.AddScoped<UnitOfWork>();
        services.AddScoped<ServicesHost>();
        services.AddSingleton<OnlineTracker>();
        services.AddSingleton<ClientLogQuota>();
        services.AddSingleton(new MetricCollector());
        services.AddHostedService<ActivityFlushService>();
        services.AddHostedService<RetentionService>();
        services.AddHostedService<MetricBackgroundService>();

        var app = builder.Build();

        app.UseForwardedHeaders();
        // до обработчика исключений: в лог запроса попадает итоговый статус (400 для InfoException), а не 500
        if (AppSettings.LogRequests) app.UseSerilogRequestLogging(options => {
            // статика — Verbose, остальное Information, ошибки — Error
            options.GetLevel = (context, elapsed, exception) =>
                exception != null || context.Response.StatusCode >= 500 ? LogEventLevel.Error
                : context.Request.Path.StartsWithSegments("/build") ? LogEventLevel.Verbose
                : LogEventLevel.Information;
            // X-Request-Id ставит App и пишет в свои ошибки — связь записи запроса с логом клиента;
            // RequestId занят trace id ASP.NET, он связывает записи одного запроса на сервере
            options.EnrichDiagnosticContext = (diagnosticContext, context) => {
                var requestId = context.Request.Headers["X-Request-Id"].ToString();
                if (requestId.Length > 0) {
                    diagnosticContext.Set("ClientRequestId", requestId.Truncate(64));
                }
            };
        });
        if (app.Environment.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        } else {
            app.UseHttpsRedirection();
        }
        app.UseExceptionHandler(options => { });
        app.UseStaticFiles();
        // без авторизации, как /Download WebV1: установщик и автообновление ClickOnce cookie не передают
        Directory.CreateDirectory(AppSettings.DownloadPath);
        app.UseStaticFiles(new StaticFileOptions {
            FileProvider = new PhysicalFileProvider(AppSettings.DownloadPath),
            RequestPath = AppSettings.DownloadRequestPath
        });
        app.UseAuthentication();
        app.UseMiddleware<RequestContextMiddleware>();
        app.UseAuthorization();
        app.MapControllers();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        try {
            MigrateAndSeed(app);
            app.Run();
        } catch (Exception e) {
            _l.f("Host terminated", e);
            throw;
        } finally {
            Log.CloseAndFlush();
        }
    }

    #region [ private ]

    private static void ConfigureSerilog(string logConnectionString) {
        Directory.CreateDirectory(AppSettings.LogsPath);
        Directory.CreateDirectory(AppSettings.KeysPath);

        // сбои самих синков (недоступна БД логов, нет колонки) иначе теряются молча
        var selfLog = File.AppendText(Path.Combine(AppSettings.LogsPath, "serilog-selflog.txt"));
        selfLog.AutoFlush = true;
        SelfLog.Enable(TextWriter.Synchronized(selfLog));

        // свойства, вынесенные в колонки; остальное — в LogEvent (JSON). Exception и MessageTemplate не хранятся:
        // исключение в коротком виде кладёт в Details DetailsEnricher, шаблон у CommonLogger всегда один
        var columnOptions = new ColumnOptions {
            AdditionalColumns = new Collection<SqlColumn> {
                new SqlColumn("Scope", SqlDbType.VarChar, dataLength: 50),
                new SqlColumn("UserName", SqlDbType.VarChar, dataLength: 256),
                new SqlColumn("RequestPath", SqlDbType.VarChar, dataLength: 256),
                new SqlColumn("SourceContext", SqlDbType.VarChar, dataLength: 256),
                new SqlColumn(AppSettings.LogDetailsProperty, SqlDbType.VarChar, dataLength: -1),
                new SqlColumn(AppSettings.LogSessionProperty, SqlDbType.VarChar, dataLength: 32),
            }
        };
        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);
        columnOptions.Store.Remove(StandardColumn.Exception);
        columnOptions.Store.Add(StandardColumn.LogEvent);
        columnOptions.LogEvent.ExcludeAdditionalProperties = true;
        columnOptions.LogEvent.ExcludeStandardColumns = true;
        columnOptions.TimeStamp.ConvertToUtc = true;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new DetailsEnricher())
            .WriteTo.Console()
            // файл — резерв на случай недоступной БД логов, без дедупликации
            .WriteTo.File(new CompactJsonFormatter(), Path.Combine(AppSettings.LogsPath, "log-.json"),
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: AppSettings.LogFileRetainedDays)
            // БД: повторы одного Warning+ режутся до LogDeduplicator.MaxPerWindow в минуту
            .WriteTo.Logger(db => db
                .Filter.With(new LogDeduplicator())
                .WriteTo.MSSqlServer(
                    connectionString: logConnectionString,
                    sinkOptions: new MSSqlServerSinkOptions {
                        TableName = AppSettings.LogTableName,
                        AutoCreateSqlTable = true,
                        BatchPostingLimit = 100,
                        BatchPeriod = TimeSpan.FromSeconds(5)
                    },
                    columnOptions: columnOptions))
            .CreateLogger();
    }

    private static void MigrateAndSeed(WebApplication app) {
        ServiceLocator.Initialize(app.Services);
        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider;

        Migrate(provider.GetRequiredService<AdLogDbContext>(), "AdLogDbContext");
        if (Migrate(provider.GetRequiredService<AdDbContext>(), "AdDbContext") == false) {
            return;
        }
        _l.i("Seeding database");
        var host = provider.GetRequiredService<ServicesHost>();
        AdDbContextSeed.SeedAsync(provider.GetRequiredService<LinqToDbContext>(), host.UserManager, host.RoleManager).Wait();
    }

    /// <summary>
    ///     false — миграций в сборке нет (контекст ещё не инициализирован пользователем) </summary>
    private static bool Migrate(DbContext dbContext, string name) {
        if (dbContext.Database.GetMigrations().Any() == false) {
            _l.w($"No migrations found for {name}: run `dotnet ef migrations add Init --context {name}` in AutoDispatch.Web.Data");
            return false;
        }
        if (dbContext.Database.GetPendingMigrations().Any()) {
            _l.i($"Migrating {name}");
            dbContext.Database.Migrate();
        }
        return true;
    }

    #endregion [ private ]
}
