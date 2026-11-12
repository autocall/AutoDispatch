using System.ComponentModel.DataAnnotations.Schema;
using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Data.Entities.Log;

/// <summary>
///     Значение метрики за минуту, как Metric в Safer. Составной ключ (DateTime, MetricId) </summary>
public class Metric {

    /// <summary>
    ///     Начало минуты, UTC </summary>
    [Column(TypeName = "datetime2(0)")]
    public DateTime DateTime { get; set; }

    public MetricTypeEnum MetricId { get; set; }

    /// <summary>
    ///     CPU — %, задержка — мс, остальное — штуки </summary>
    public int Value { get; set; }
}
