using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Объект с текущей позицией (CarrierView/BNSF). В V1 — geography CurrentLocation; здесь пара координат
///     (решено 2026-11-13, AI/wiki/plan.md) </summary>
public class BaseDispatchEntity : BaseObjectEntity {

    [Column(TypeName = "decimal(9,6)")]
    public decimal? CurrentLatitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? CurrentLongitude { get; set; }
}
