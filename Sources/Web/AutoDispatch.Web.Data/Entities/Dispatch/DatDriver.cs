using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Водитель карриера (брокерская доска). UserId — пользователь V1 без FK: может быть не перенесён </summary>
public class DatDriver : BaseDispatchEntity {

    public Guid? UserId { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }

    [MaxLength(64)]
    public string TruckNumber { get; set; }

    [MaxLength(64)]
    public string TrailerNumber { get; set; }

    [MaxLength(64)]
    public string ContactPhone { get; set; }

    public string Comments { get; set; }

    public int? CarrierViewId { get; set; }

    [MaxLength(64)]
    public string CarrierViewLoadId { get; set; }

    public string CarrierViewLink { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? CarrierViewLastUpdatedDate { get; set; }

    [MaxLength(64)]
    public string CarrierViewStatus { get; set; }

    [MaxLength(256)]
    public string CarrierViewStatusesJson { get; set; }

    public bool CarrierViewLate { get; set; }

    public int? CarrierViewPickupId { get; set; }

    public int? CarrierViewDeliveryId { get; set; }

    public string CarrierViewPickupComment { get; set; }

    public string CarrierViewDeliveryComment { get; set; }

    [InverseProperty("DatDriver")]
    public ICollection<Load> Loads { get; set; }
}
