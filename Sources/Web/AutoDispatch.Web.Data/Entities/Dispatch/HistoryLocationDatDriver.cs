using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

[Index(nameof(EventId), Name = "IX_HistoryLocationDatDriver_EventId")]
public class HistoryLocationDatDriver : BaseHistoryLocationEntity {

    [Required]
    public Guid DatDriverId { get; set; }

    [Required]
    public override int EventId { get; set; }

    [ForeignKey("DatDriverId")]
    public DatDriver DatDriver { get; set; }

    [InverseProperty("HistoryLocationDatDriver")]
    public ICollection<HistoryLocationLoad> HistoryLocationLoads { get; set; }
}
