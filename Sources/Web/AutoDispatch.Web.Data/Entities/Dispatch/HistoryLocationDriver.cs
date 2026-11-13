using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

[Index(nameof(EventId), Name = "IX_HistoryLocationDriver_EventId")]
public class HistoryLocationDriver : BaseHistoryLocationEntity {

    [Required]
    public Guid DriverId { get; set; }

    public int? HistoryLocationTruckId { get; set; }

    [Required]
    public override int EventId { get; set; }

    [ForeignKey("DriverId")]
    public Driver Driver { get; set; }

    [ForeignKey("HistoryLocationTruckId")]
    public HistoryLocationTruck HistoryLocationTruck { get; set; }
}
