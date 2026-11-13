#if !DEBUG
namespace AutoDispatch.Web.Common;

// Окружение Release-сборки (контейнер autodispatch.web на 67.220.74.190). DEBUG-аналог — AppSettings.Environment.Local.cs.
public partial class AppSettings {

    public const string Website = "https://dispatch.myautocall.net";
    public const string DataBaseName = "autodispatch";
    /// <summary>
    ///     Логи Serilog, журнал входов, активность — отдельная БД </summary>
    public const string LogDataBaseName = "autodispatch_log";

    #region [ Paths ]

    // volume контейнера, см. AI/wiki/deploy.md
    public const string LogsPath = "/logs/";
    public const string KeysPath = "/keys/";
    public const string DownloadPath = "/download/";
    /// <summary>
    ///     Документы dispatch: каталог {EntityId} груза, карриера или setup packet (V1 FilePath, E:\Files) </summary>
    public const string FilesPath = "/files/";
    /// <summary>
    ///     Удалённые через FileDelete документы (V1 RecyclePath, E:Recycle) — подкаталог FilesPath, второй volume не нужен </summary>
    public const string RecyclePath = FilesPath + "_recycle/";

    #endregion [ Paths ]
}
#endif
