using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Запись истории с координатами (в V1 — geography Location) </summary>
public class BaseHistoryLocationEntity : BaseHistoryEntity {

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }
}
