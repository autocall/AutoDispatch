using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

[Index(nameof(EventId), Name = "IX_HistoryLocationTruck_EventId")]
public class HistoryLocationTruck : BaseHistoryLocationEntity {

    [Required]
    public Guid TruckId { get; set; }

    public int? HistoryLocationTrailerId { get; set; }

    [Required]
    public override int EventId { get; set; }

    [ForeignKey("TruckId")]
    public Truck Truck { get; set; }

    [ForeignKey("HistoryLocationTrailerId")]
    public HistoryLocationTrailer HistoryLocationTrailer { get; set; }

    [InverseProperty("HistoryLocationTruck")]
    public ICollection<HistoryLocationDriver> HistoryLocationDrivers { get; set; }
}
