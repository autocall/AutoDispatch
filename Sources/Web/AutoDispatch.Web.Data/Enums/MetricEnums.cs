using System.ComponentModel;

namespace AutoDispatch.Web.Data.Enums;

/// <summary>
///     Метрика в таблице Metric (БД логов): одно значение за минуту. Серверные — 1..29, клиентские (отчёт App) — 30..99,
///     внешние сервисы — с 100 (тройка задержка · успешные вызовы · ошибки, MetricConverter.Externals).
///     Как агрегируются значения минуты — MetricConverter.TypeToKind </summary>
public enum MetricTypeEnum : byte {
    Unknown = 0,

    [Description("Web CPU")]
    WebCpu = 1,
    /// <summary> Скрипт metric_cpu.sh стоит на веб-хосте, как у V3, — фактически CPU веб-хоста (AI/wiki/deploy.md) </summary>
    [Description("DB CPU")]
    DbCpu = 2,
    /// <summary> BrokersnapshotWebService api/v2: геокодинг (autocomplete, latlng, place_id) и маршруты; компании — CompanyLatency </summary>
    [Description("Geocoding")]
    GeoLatency = 3,
    [Description("Geocoding")]
    GeoCounter = 4,
    [Description("Geocoding")]
    GeoErrorCounter = 5,

    // dispatch: Api/Dispatch/Request и long-polling Response
    [Description("Request")]
    DispatchRequestLatency = 6,
    [Description("Request")]
    DispatchRequestCounter = 7,
    /// <summary> Ответ Status Failed, кроме InfoException (ошибка пользователя) </summary>
    [Description("Request")]
    DispatchRequestErrorCounter = 8,
    /// <summary> Дольше AppSettings.DispatchSlowRequest </summary>
    [Description("Request")]
    DispatchRequestSlowCounter = 9,
    /// <summary> Пользователи, ждущие событий в Api/Dispatch/Response (замер раз в MetricFlushInterval) </summary>
    [Description("Polling")]
    DispatchPollUsers = 10,

    // процесс и диск веб-сервера — на /Admin/Monitor (сейчас, пик, прирост), не колонками /Admin/Metrics
    /// <summary> Память процесса сайта (working set), МБ </summary>
    [Description("Memory")]
    WebMemory = 11,
    /// <summary> Занято на разделе с документами (AppSettings.FilesPath), МБ: процент слишком груб для суточного прироста </summary>
    [Description("Disk")]
    WebDiskUsedMb = 12,

    // активность, как «Active patterns» / «Active records» AutoCall V3: кто работает и идут ли данные извне
    /// <summary> Пользователи, у которых за минуту прошёл изменяющий запрос App (DispatchRequestHandler.IsChange) </summary>
    [Description("Active")]
    ActiveUsers = 13,
    /// <summary> Грузы, по которым за минуту пришла позиция водителя от CarrierView (webhook /Dispatch/UpdateLocation) </summary>
    [Description("Tracking")]
    TrackedLoads = 14,

    /// <summary> App открыт: от пользователя за минуту пришёл отчёт </summary>
    [Description("Online")]
    OnlineUsers = 30,

    // телефония App (как AutoCall V3, номера те же; DAT/Truckstop 31–41 к AutoDispatch не относятся)
    [Description("MicroSIP")]
    MicroSipCallCounter = 42,
    [Description("MicroSIP")]
    MicroSipCallErrorCounter = 43,
    [Description("AbtoPhone")]
    AbtoPhoneCallCounter = 44,
    [Description("AbtoPhone")]
    AbtoPhoneCallErrorCounter = 45,
    /// <summary> Пользователи, у которых AbtoPhone не зарегистрирован на PBX (последний отчёт минуты); на странице — в ошибках AbtoPhone </summary>
    [Description("AbtoPhone")]
    AbtoPhoneUnregisteredUsers = 46,

    [Description("App exceptions")]
    AppExceptions = 47,
    // 48–49 (разрывы связи и повторные входы App) не используются: в V3 их нет, оба события пишутся в лог App (2026-11-27)
    /// <summary> Запросы App к серверу дольше App-порога (EngineManager.SlowRequest, 5 с) </summary>
    [Description("App slow")]
    AppSlowRequests = 50,

    // внешние сервисы: задержка (мс, медиана), успешные вызовы, ошибки
    [Description("CarrierView")]
    CarrierViewLatency = 100,
    [Description("CarrierView")]
    CarrierViewCounter = 101,
    [Description("CarrierView")]
    CarrierViewErrorCounter = 102,
    [Description("SMS")]
    SmsLatency = 103,
    [Description("SMS")]
    SmsCounter = 104,
    [Description("SMS")]
    SmsErrorCounter = 105,
    // 106–108 (Mailgun), 115–117 (Safer FMCSA — запасной путь при сбое Companies) и 118–120 (Vodia) не используются: редки, сбои — в логе (2026-11-27)
    /// <summary> SMTP компаний (EmailUtils): инвойсы, confirmation, setup packet </summary>
    [Description("SMTP")]
    SmtpLatency = 109,
    [Description("SMTP")]
    SmtpCounter = 110,
    [Description("SMTP")]
    SmtpErrorCounter = 111,
    [Description("OpenAI")]
    OpenAiLatency = 112,
    [Description("OpenAI")]
    OpenAiCounter = 113,
    [Description("OpenAI")]
    OpenAiErrorCounter = 114,
    /// <summary> Часовой пояс через keeppingin.com </summary>
    [Description("Keeppingin")]
    TimeZoneLatency = 121,
    [Description("Keeppingin")]
    TimeZoneCounter = 122,
    [Description("Keeppingin")]
    TimeZoneErrorCounter = 123,
    /// <summary> Google Distance Matrix </summary>
    [Description("Google Distance")]
    DistanceLatency = 124,
    [Description("Google Distance")]
    DistanceCounter = 125,
    [Description("Google Distance")]
    DistanceErrorCounter = 126,
    /// <summary> BrokersnapshotWebService api/v3: карточка карриера по DOT/MC и поиск компаний (census) </summary>
    [Description("Companies")]
    CompanyLatency = 127,
    [Description("Companies")]
    CompanyCounter = 128,
    [Description("Companies")]
    CompanyErrorCounter = 129
}

/// <summary>
///     Как значения одной минуты сводятся в одно </summary>
public enum MetricKindEnum : byte {
    Unknown = 0,
    /// <summary> Медиана замеров (CPU, задержка) </summary>
    Median = 1,
    /// <summary> Сумма за минуту (счётчики: отчёты клиента покрывают непересекающиеся 30 с) </summary>
    Sum = 2,
    /// <summary> Различные пользователи, у которых признак был хотя бы в одном отчёте минуты: два отчёта не удваивают (TrackedLoads — различные грузы) </summary>
    DistinctUsers = 3,
    /// <summary> Различные пользователи, у которых признак есть в последнем отчёте минуты (состояние, а не событие) </summary>
    LastPerUser = 4
}
