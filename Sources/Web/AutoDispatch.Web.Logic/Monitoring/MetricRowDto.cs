using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Градация таблицы метрик: дни → часы → минуты </summary>
public enum MetricLevelEnum : byte {
    Unknown = 0,
    Minutes = 1,
    Hours = 2,
    Days = 3
}

/// <summary>
///     Строка таблицы метрик: минута, час или день, свёрнутые из минутных значений </summary>
public class MetricRowDto {

    /// <summary>
    ///     Начало периода, UTC </summary>
    public DateTime Start { get; set; }

    /// <summary>
    ///     Что показывать: события — сумма, CPU и задержка — среднее, пользователи — пик </summary>
    public Dictionary<MetricTypeEnum, int> Values { get; set; } = new();

    /// <summary>
    ///     По чему красить: CPU и задержка — максимум за период, остальное — как Values </summary>
    public Dictionary<MetricTypeEnum, int> StatusValues { get; set; } = new();
}
