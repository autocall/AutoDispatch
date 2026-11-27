namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Карточка Server на /Admin/Monitor: память процесса и раздел с документами (MetricService.GetServerStatsAsync) </summary>
public class ServerStatsDto {

    /// <summary>
    ///     Память процесса сейчас, МБ </summary>
    public int MemoryMb { get; set; }

    /// <summary>
    ///     Пик за 24 часа в текущем запуске (с начала суток или со старта), МБ; null — замеров ещё не было </summary>
    public int? MemoryPeakMb { get; set; }

    /// <summary>
    ///     24 часа назад, МБ; null — процесс тогда не работал или замера нет </summary>
    public int? MemoryDayAgoMb { get; set; }

    /// <summary>
    ///     Раздел с документами; 0 — не определяется </summary>
    public long DiskTotalMb { get; set; }
    public long DiskFreeMb { get; set; }
    public long DiskUsedMb => DiskTotalMb - DiskFreeMb;
    public int DiskPercent => DiskTotalMb > 0 ? (int)(100 * DiskUsedMb / DiskTotalMb) : 0;

    /// <summary>
    ///     Прирост занятого за 24 часа, МБ; null — замера сутки назад нет </summary>
    public long? DiskLastDayMb { get; set; }

    /// <summary>
    ///     Средний прирост в день за 7 дней (или с первого замера, если он не раньше суток назад), МБ; null — данных меньше суток </summary>
    public double? DiskDayAverageMb { get; set; }

    /// <summary>
    ///     Дней до заполнения при среднем приросте; null — прироста нет или он не известен </summary>
    public int? DaysUntilFull => DiskDayAverageMb > 0 ? (int)(DiskFreeMb / DiskDayAverageMb.Value) : null;
}
