namespace AutoDispatch.Web.Common;

// Общие настройки, одинаковые на всех окружениях. Окружение — AppSettings.Environment.*.cs, секреты — AppSettings.Secrets.cs.
public partial class AppSettings {

    public const string ApplicationName = "AutoDispatch.Web";

    #region [ Identity ]

    public const string AdminUserName = "Admin";
    public const string AdminEmail = "admin@myautocall.net";
    public const string SystemUserName = "System";
    public const string SystemEmail = "system@myautocall.net";

    public const int PasswordMinLength = 6;

    public static readonly TimeSpan JwtLifetime = TimeSpan.FromDays(30);
    public static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(30);
    public const string CookieName = "AutoDispatch.Auth";
    public const string LogInPath = "/Account/LogIn";

    #endregion [ Identity ]

    #region [ Email ]

    public const string MailgunApiUrl = "https://api.mailgun.net";
    /// <summary>
    ///     Страница сайта, куда ведёт ссылка из письма сброса пароля </summary>
    public const string ResetPasswordPath = "/Account/ResetPassword";

    #endregion [ Email ]

    #region [ Download ]

    /// <summary>
    ///     Загрузки из DownloadPath (как /Download WebV1): ClickOnce-публикация AutoDispatch App в корне </summary>
    public const string DownloadRequestPath = "/Download";
    public const string DownloadApplicationFile = "AutoDispatch.application";
    public const string DownloadSetupFile = "setup.exe";

    #endregion [ Download ]

    #region [ Places ]

    /// <summary>
    ///     Геокодинг Safer (автокомплит городов и координаты по place_id), замена прокси keeppingin </summary>
    public const string BrokersnapshotApiUrl = "https://brokersnapshot.com";
    public static readonly TimeSpan BrokersnapshotTimeout = TimeSpan.FromSeconds(5);
    public const int PlaceSearchMaxLength = 100;
    public const int PlaceSearchMinLength = 2;

    #endregion [ Places ]

    #region [ Map ]

    /// <summary>
    ///     Карта маршрута /Map?r=…: ссылку выдаёт Api/Maps/Link, точки зашифрованы в r (AES-GCM, Secrets:Map:Key) — подменить
    ///     координаты в ссылке нельзя; r же служит пропуском на страницу и её запросы, cookie-логин не нужен.
    ///     Срока действия у ссылки нет </summary>
    public const string MapPath = "/Map";
    /// <summary>
    ///     Остановок, добавляемых кликом по карте — лимит waypoints Google Directions в Safer </summary>
    public const int MapMaxStops = 25;

    #endregion [ Map ]

    #region [ Limits ]

    public static readonly TimeSpan SqlTimeout = TimeSpan.FromSeconds(30);

    #endregion [ Limits ]

    #region [ Logging ]

    /// <summary>
    ///     Имя свойства Serilog, которым помечается область логов: using (_l.Scope("auth")) { ... } </summary>
    public const string LogScopeProperty = "Scope";
    /// <summary>
    ///     Свойство с деталями записи (JSON, короткий текст исключения) — колонка Details таблицы Log </summary>
    public const string LogDetailsProperty = "Details";
    /// <summary>
    ///     Свойство с id запуска App — колонка Session таблицы Log </summary>
    public const string LogSessionProperty = "Session";
    /// <summary>
    ///     Таблица Serilog в LogDataBaseName (создаёт сам sink; существующую не меняет — колонки через миграции AcLogDbContext) </summary>
    public const string LogTableName = "Log";
    /// <summary>
    ///     Клиентское время записи из Api/Logs — в LogEvent; TimeStamp — время получения сервером </summary>
    public const string LogClientTimeProperty = "ClientTime";
    public const int LogRetainedDays = 30;
    public const int LogFileRetainedDays = 7;
    public const int LogPageSize = 200;
    /// <summary>
    ///     Запись каждого HTTP-запроса (UseSerilogRequestLogging) — основной объём таблицы Log; выключено (решение 2026-10-26) </summary>
    public static readonly bool LogRequests = false;

    /// <summary>
    ///     Api/Logs: записей в пачке (лишние отбрасываются), длины Message и Details, квота на пользователя за окно </summary>
    public const int ClientLogBatchLimit = 100;
    public const int ClientLogMessageMaxLength = 500;
    public const int ClientLogDetailsMaxLength = 4000;
    public const int ClientLogQuota = 1000;
    public static readonly TimeSpan ClientLogQuotaWindow = TimeSpan.FromMinutes(10);

    #endregion [ Logging ]

    #region [ Monitoring ]

    /// <summary>
    ///     Пользователь считается онлайн, если был запрос за это время </summary>
    public static readonly TimeSpan OnlineTimeout = TimeSpan.FromMinutes(5);
    /// <summary>
    ///     Как часто онлайн-пользователи сбрасываются в UserActivity </summary>
    public static readonly TimeSpan ActivityFlushInterval = TimeSpan.FromMinutes(5);
    public const int UserLoginRetainedDays = 365;
    public const int UserActivityRetainedDays = 365;

    #endregion [ Monitoring ]

    #region [ Metrics ]

    /// <summary>
    ///     Как часто снимается CPU и сохраняются закрытые минуты (как MetricBackgroundService в Safer) </summary>
    public static readonly TimeSpan MetricFlushInterval = TimeSpan.FromSeconds(20);
    /// <summary>
    ///     Сколько последних периодов показывают вкладки Days / Hours / Minutes (дней не больше MetricRetainedDays).
    ///     День и час, открытые переходом по строке, показываются целиком: 24 часа и 60 минут </summary>
    public const int MetricRecentPeriods = 10;
    /// <summary>
    ///     ~25 строк в минуту: 90 дней — около 3,2 млн строк </summary>
    public const int MetricRetainedDays = 90;

    #endregion [ Metrics ]
}
