using AutoCall.Common;
using AutoCall.Common.Loggers;
using AutoCall.Logic.Exceptions;
using AutoCall.Logic.Helpers;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Dialogs;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoCall.WPF.Mappings;
using AutoDispatch.Data.Api;
using AutoDispatch.Data.Metrics;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.Managers;
using AutoDispatch.Logic.Mappings;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using MlSampleWindowCS.Services;
using Polly;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders.Xaml;

namespace AutoCall.WPF {
    /// <summary>
    ///     Interaction logic for App.xaml.
    ///     Как AutoCall V3 (AutoCall.View/App.xaml.cs): MS DI — все регистрации здесь при старте (V1 — Unity, Bootstrapper и поздняя
    ///     регистрация AutoDispatch при логине), Serilog в файл и на сервер (ServerLogSink), глобальные обработчики исключений.
    ///     Из V1 сохранено: культура en-US, тема Vista, удаление старых сканов, прогрев окна груза, обработка исключений Dispatcher </summary>
    public partial class App : Application {

        public static IServiceProvider Services { get; private set; } = default!;

        /// <summary>
        ///     Visibility for debug mode </summary>
        public static Visibility IsDebug {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
        }

        /// <summary>
        ///     Каталог логов в профиле пользователя, как V1 (%AppData%\AutoCall\logs) и V3 </summary>
        public static string LogDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoDispatch", "logs");

        private const string LogTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        ///     Логи Information+ на сервер (Api/Logs): создаётся вместе с логгером до DI, отправка начинается после входа (MainWindow) </summary>
        private static readonly ServerLogSink ServerLog = new ServerLogSink();

        /// <summary>
        ///     Причина выхода для записи «AutoDispatch exited» </summary>
        private static bool _crashed;

        public static bool DispatcherIsAlive() {
            return
                Application.Current != null &&
                Application.Current.Dispatcher != null &&
                Application.Current.Dispatcher.Thread != null &&
                Application.Current.Dispatcher.Thread.IsAlive;
        }

        private static Serilog.ILogger CreateLogger() {
            var configuration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                // HttpClient сам по себе шумит на каждом запросе
                .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
                .WriteTo.Sink(ServerLog)
                .WriteTo.File(
                    Path.Combine(LogDirectory, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    // V1 DeleteOldLogs — файлы старше 7 дней
                    retainedFileCountLimit: 7,
                    outputTemplate: LogTemplate,
                    shared: true);
#if DEBUG
            configuration = configuration
                .MinimumLevel.Debug()
                .WriteTo.Debug(outputTemplate: LogTemplate);
#else
            configuration = configuration.MinimumLevel.Information();
#endif
            return configuration.CreateLogger();
        }

        /// <summary>
        ///     On application startup </summary>
        private void Application_Startup(object sender, StartupEventArgs args) {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();

            Directory.CreateDirectory(LogDirectory);
            Serilog.ILogger logger = CreateLogger();
            Serilog.Log.Logger = logger;
            services.AddSingleton<Serilog.ILogger>(logger);

            RegisterGlobalExceptionHandlers();

            // одна запись на запуск с окружением в Details — по ней на сервере видно версию и машину пользователя
            var version = VersionHelper.GetDisplayVersion();
            Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Session)
                .ForContext(LogProperties.Details, String.Join(Environment.NewLine,
                    $"Version {version}",
                    $"OS {Environment.OSVersion.VersionString}, {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}",
                    $"Machine {Environment.MachineName}, user {Environment.UserName}",
                    $"Session {AppSession.Id}",
                    $"Logs at {LogDirectory}"))
                .Information("AutoDispatch started. Version {Version}, logs at {LogDirectory}", version, LogDirectory);

            XamlDataProvider provider = new XamlDataProvider();
            ScanFileHelper.DeleteOldTempFiles();

            // AutoMapper 12 — только автоматические пары; конвертеры — статические XConverter (AI/wiki/plan.md «DI и AutoMapper в App»)
            services.AddAutoMapper(cfg => {
                cfg.AddProfile<BllMappingProfile>();
                cfg.AddProfile<ViewMappingProfile>();
            });

            // HTTP-клиенты сервера: Settings.WebUrl, Bearer и X-Request-Id — BaseApi (как V3); V1 — HttpWebRequest на UrlApi из app.config
            AddApi<LoginDataService>(services);
            AddApi<ActualCompanyDataService>(services);
            AddApi<DatCompanyDataService>(services);
            AddApi<HelperDataService>(services);
            AddApi<SettingsApi>(services);
            AddApi<MapApi>(services);
            // Request/Response протокола dispatch: без повторов Polly (Request не идемпотентен) — повторяет сам EngineManager и только
            // сбои сети, таймаут и 5xx. Таймаут 10 минут — на весь Request вместе с отправкой файла (FileSave, base64 в JSON);
            // у long-polling свой, 100 с (EngineDataService.ResponseTimeout) — решено 2026-11-25
            services.AddHttpClient<EngineDataService>(client => {
                client.BaseAddress = new Uri(Settings.WebUrl);
                client.Timeout = TimeSpan.FromMinutes(10);
            });
            // Метрики и логи App — раз в 30 с, без повторов
            services.AddHttpClient<MetricsApi>(client => {
                client.BaseAddress = new Uri(Settings.WebUrl);
                client.Timeout = TimeSpan.FromSeconds(15);
            });
            services.AddSingleton<MetricsReporter>();
            services.AddHttpClient<LogsApi>(client => {
                client.BaseAddress = new Uri(Settings.WebUrl);
                client.Timeout = TimeSpan.FromSeconds(15);
            });
            services.AddSingleton(ServerLog);

            // Менеджеры и сервисы dispatch — конкретные классы без интерфейсов (V1 — Unity + ServicesHost)
            services.AddSingleton<EngineManager>();
            services.AddSingleton<LoginManager>();
            services.AddSingleton<DispatchService>();
            services.AddSingleton<LoginService>();
            services.AddSingleton<ReportService>();
            services.AddSingleton<SettingsService>();

            // Софтфоны (AI/wiki/plan.md «Софтфоны в App»)
            services.AddSingleton<AutoPhoneV2CallService>();
            services.AddSingleton<MainManager>();

            services.AddSingleton<LoginDialog>();
            services.AddSingleton<MainWindow>();

            // Sets US culture
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            // Set Theme
            StyleManager.ApplicationTheme = new VistaTheme();

            Services = services.BuildServiceProvider();

            // preload forms
            var thread = new Thread(() => {
                try {
                    var dialog1 = new EditLoadDialog();
                    dialog1.InitializeComponent();
                    var dialog2 = new RichTextEditor();
                    dialog2.InitializeComponent();
                    // V1: WebView.Preload("https://myautocall.net/Dispatch/InitializeRouteLocation") — прогрев EO.WebBrowser, с WebView2 не нужен
                } catch (Exception) {
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();

            var g = Graphics.FromHwnd(IntPtr.Zero);
            Debug.WriteLine($"DPI: {g.DpiX}x{g.DpiY}, Scale: {g.DpiX / 96f:F2}x{g.DpiY / 96f:F2}");

            // Disable shutdown when the dialog closes
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            // Show login dialog
            var loginDialog = Services.GetRequiredService<LoginDialog>();
            if (loginDialog.ShowDialog() == true) {
                // Show Main window
                var mainWindow = Services.GetRequiredService<MainWindow>();

                // Re-enable normal shutdown mode.
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Current.MainWindow = mainWindow;
                mainWindow.Show();

                var settings = Services.GetRequiredService<SettingsService>();
                MicroSIPHelper.StartProcess(settings);
                AbtoPhoneHelper.StartProcess(settings, Services.GetRequiredService<AutoPhoneV2CallService>());
            } else {
                Current.Shutdown(0);
            }
        }

        private static void AddApi<TApi>(IServiceCollection services) where TApi : class {
            services.AddHttpClient<TApi>(client => {
                client.BaseAddress = new Uri(Settings.WebUrl);
                // версию сервер пишет в журнал входов — по ней столбец Client Version в /Admin/Users (V3)
                client.DefaultRequestHeaders.UserAgent.Add(
                    new System.Net.Http.Headers.ProductInfoHeaderValue("AutoDispatch", VersionHelper.GetDisplayVersion()));
            })
            .AddTransientHttpErrorPolicy(policy => policy.RetryAsync(2));
        }

        /// <summary>
        ///     Перехватывает то, что не долетает до Dispatcher: фоновые потоки и «съеденные» задачи (V3) </summary>
        private void RegisterGlobalExceptionHandlers() {
            AppDomain.CurrentDomain.UnhandledException += (_, args) => {
                ClientMetrics.Exception();
                _crashed = true;
                Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Session).Fatal(args.ExceptionObject as Exception,
                    "Unhandled domain exception. Terminating: {IsTerminating}", args.IsTerminating);
                // CloseAndFlush закрывает и ServerLogSink: остаток уходит на сервер синхронно
                Serilog.Log.CloseAndFlush();
            };

            TaskScheduler.UnobservedTaskException += (_, args) => {
                ClientMetrics.Exception();
                Serilog.Log.Error(args.Exception, "Unobserved task exception");
                args.SetObserved();
            };
        }

        private void Application_Exit(object sender, ExitEventArgs args) {
            Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Session)
                .Information("AutoDispatch exited: {Reason}, code {Code}", _crashed ? "crash" : "closed", args.ApplicationExitCode);
            Serilog.Log.CloseAndFlush();
        }

        public void valueBindableTextBlock_Comment_OnMouseUp(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                RadContextMenu.GetContextMenu((FrameworkElement)sender).PlacementTarget = (UIElement)sender;
                RadContextMenu.GetContextMenu((FrameworkElement)sender).IsOpen = true;
                e.Handled = true;
            }
        }

        private string GetMemoryInfo() {
            var memory = GC.GetGCMemoryInfo();
            var available = memory.TotalAvailableMemoryBytes / 1024 / 1024 / 1024;
            Process currentProcess = Process.GetCurrentProcess();
            long totalBytesOfMemoryUsed = currentProcess.WorkingSet64 / 1024 / 1024;
            // V1: Microsoft.VisualBasic.Devices.ComputerInfo (свободная/вся память) — в .NET нет, доступная процессу — GC
            return $"Memory (Available): {available}Gb. AutoDispatch: {totalBytesOfMemoryUsed}Mb. OS: {Environment.OSVersion}";
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            ClientMetrics.Exception();
            var exception = e.Exception.GetBaseException();
            if (exception is ArgumentOutOfRangeException { ParamName: "count" }) {
                // Telerik CellRegionsPartitioner при вставке в грид (V1, борды DatPower). V1 сравнивал текст сообщения
                // "…\r\nParameter name: count" — в .NET он другой ("… (Parameter 'count')"), поэтому по типу и имени параметра
                e.Handled = true;
                return;
            }
            if (exception is AutoCallException) {
                MessageBoxEx.Error(exception.Message);
                e.Handled = true;
                return;
            }
            // V1: DataException — ошибки DAT Power (модуль AutoCall), в App не возникают
            if (exception is OutOfMemoryException) {
                var memoryInfo = GetMemoryInfo();
                var stackCalls = StackLogger.PopAll();
                var message = $"{exception.ToString()}\n{memoryInfo}\n{stackCalls}";
                _webl.e(message);
                MessageBoxEx.Error(exception.Message, "App Error");
                return;
            }
            if (exception is COMException) {
                // System.Runtime.InteropServices.COMException (0x88980406): UCEERR_RENDERTHREADFAILURE (Exception from HRESULT: 0x88980406)
                try {
                    var memoryInfo = GetMemoryInfo();
                    var stackCalls = StackLogger.PopAll();
                    var message = $"{exception.Message}\n{memoryInfo}\n{stackCalls}";
                    _webl.e(message, exception);
                } catch (Exception ex) {
                    _webl.e(exception.ToString());
                }
                MessageBoxEx.Error(exception.Message, "App Error");
                return;
            } else {
                var memoryInfo = GetMemoryInfo();
                var stackCalls = StackLogger.PopAll();
                var message = exception != e.Exception
                    ? $"Unhandled Exception\n{memoryInfo}\n{stackCalls}\n{e.Exception}\n** BASE EXCEPTION **\n{exception}"
                    : $"Unhandled Exception\n{memoryInfo}\n{stackCalls}\n{exception}";
                Serilog.Log.Error(e.Exception, message);
                MessageBoxEx.Error($"{exception.Message}\n\nDetails: {LogDirectory}", "App Error");
                e.Handled = true;
            }

            if (this.Windows.Count == 0) {
                Environment.Exit(0);
            }
        }

        private void CarrierViewImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var datDriver = ((FrameworkElement)sender).DataContext as DatDriverViewModel;
            if (datDriver == null) {
                throw new AutoCallException("Driver was not defined");
            }
            if (datDriver.CarrierViewId == null) {
                throw new AutoCallException("CarrierView# was not defined");
            }
            if (datDriver.CarrierViewLink == null) {
                throw new AutoCallException("CarrierView Link was not defined");
            }
            // .NET: UseShellExecute по умолчанию false — ссылку открывает только оболочка
            Process.Start(new ProcessStartInfo(datDriver.CarrierViewLink) { UseShellExecute = true });
        }

        private void RadMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e) {
            GridViewExtensions.RadMenuItem_Click(sender, e);
        }
    }
}
