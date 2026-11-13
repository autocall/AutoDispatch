namespace AutoDispatch.Web.Common;

// Ключи и пароли — только в конфигурации хоста (секция "Secrets"), в коде лишь имена.
public partial class AppSettings {

    /// <summary>
    ///     Ключ подписи JWT (HS256, не короче 32 байт). Обязателен в любой сборке. </summary>
    public static string JwtPrivateKey { get; private set; }

    /// <summary>
    ///     Пароль пользователей System и Admin при первом заполнении БД; мастер-пароль Api/Account/SignIn (любой пользователь). </summary>
    public static string SystemPassword { get; private set; }

    /// <summary>
    ///     IP-адреса, с которых принимается мастер-пароль (JSON-массив, точное совпадение). Пусто — мастер-пароль отключён. </summary>
    public static IReadOnlyList<string> SystemAllowedIps { get; private set; } = [];

    /// <summary>
    ///     Mailgun — системные письма (сброс пароля): ключ API, домен отправки, адрес From </summary>
    public static string MailgunApiKey { get; private set; }
    public static string MailgunDomain { get; private set; }
    public static string MailgunFrom { get; private set; }

    /// <summary>
    ///     brokersnapshot.com — геокодинг Safer, api/v2/geo (Bearer, токен плана с фичей ApiGeocoding) </summary>
    public static string BrokersnapshotApiToken { get; private set; }

    /// <summary>
    ///     Google Maps JavaScript API — подложка страницы /Map (ключ V1, ограничен доменом) </summary>
    public static string GoogleMapJsKey { get; private set; }

    /// <summary>
    ///     AerisWeather — слой погоды (радар) на /Map, ключи те же, что у GpsV4 </summary>
    public static string AerisWeatherId { get; private set; }
    public static string AerisWeatherSecret { get; private set; }

    /// <summary>
    ///     Ключ AES-256-GCM ссылок карты /Map?r=… — 32 случайных байта в base64. Смена ключа делает выданные ссылки недействительными </summary>
    public static string MapKey { get; private set; }

    /// <summary>
    ///     Bearer-токен Api/Metrics/Add — скрипт на сервере БД шлёт DB CPU (Deploy/metric_cpu.sh) </summary>
    public static string MetricToken { get; private set; }

    /// <summary>
    ///     Bandwidth — SMS складу и водителям (V1 Bandwidth*), Voxbone в V1 помечен как не работающий и не переносится </summary>
    public static string BandwidthApplicationId { get; private set; }
    public static string BandwidthUrl { get; private set; }
    public static string BandwidthUser { get; private set; }
    public static string BandwidthPassword { get; private set; }

    /// <summary>
    ///     OpenAI — разбор PDF rate confirmation в груз (V1 OPENAI_API_KEY) </summary>
    public static string OpenAiApiKey { get; private set; }

    /// <summary>
    ///     keeppingin.com Api/GoogleMap/TimeZone_Get — часовые пояса груза (V1 KeepPinginGoogleMapApiKey) </summary>
    public static string KeeppinginApiKey { get; private set; }

    /// <summary>
    ///     Google Maps Distance Matrix (V1 GoogleMapApiKey, серверный ключ) </summary>
    public static string GoogleMapApiKey { get; private set; }

    /// <summary>
    ///     Вызывать первым в Program: значения читаются из секции "Secrets" конфигурации
    ///     (appsettings.{Environment}.json или переменные Secrets__Jwt__PrivateKey).
    ///     Пустой ключ — исключение в Release, предупреждение в Debug. </summary>
    public static void Init(Func<string, string> get) {
        var missing = new List<string>();
        string Read(string key) {
            var value = get($"Secrets:{key}");
            if (String.IsNullOrEmpty(value) == false) return value;
            missing.Add(key);
            return null; // не "": пустой SystemPassword совпал бы с пустым паролем при входе
        }

        JwtPrivateKey = Read("Jwt:PrivateKey");
        SystemPassword = Read("System:Password");
        // JSON-массив: элементы приходят ключами AllowedIps:0, AllowedIps:1 … Не через Read: отсутствие списка —
        // штатный способ отключить мастер-пароль, не ошибка конфигурации
        var ips = new List<string>();
        for (var i = 0; get($"Secrets:System:AllowedIps:{i}") is { } ip; i++) {
            if (String.IsNullOrWhiteSpace(ip) == false) ips.Add(ip.Trim());
        }
        SystemAllowedIps = ips;
        MailgunApiKey = Read("Mailgun:ApiKey");
        MailgunDomain = Read("Mailgun:Domain");
        MailgunFrom = Read("Mailgun:From");
        BrokersnapshotApiToken = Read("Brokersnapshot:ApiToken");
        GoogleMapJsKey = Read("GoogleMap:JsKey");
        AerisWeatherId = Read("AerisWeather:Id");
        AerisWeatherSecret = Read("AerisWeather:Secret");
        MapKey = Read("Map:Key");
        MetricToken = Read("Metric:Token");
        BandwidthApplicationId = Read("Sms:Bandwidth:ApplicationId");
        BandwidthUrl = Read("Sms:Bandwidth:Url");
        BandwidthUser = Read("Sms:Bandwidth:User");
        BandwidthPassword = Read("Sms:Bandwidth:Password");
        OpenAiApiKey = Read("OpenAI:ApiKey");
        KeeppinginApiKey = Read("Keeppingin:ApiKey");
        GoogleMapApiKey = Read("GoogleMap:Api");

        if (missing.Count > 0) {
#if DEBUG
            Console.WriteLine($"WARNING: Missing secrets in configuration: {String.Join(", ", missing)}");
#else
            throw new InvalidOperationException($"Missing secrets in configuration: {String.Join(", ", missing)}");
#endif
        }
        if (JwtPrivateKey == null) {
            throw new InvalidOperationException("Secrets:Jwt:PrivateKey is required to sign tokens");
        }
    }
}
