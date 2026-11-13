using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

[Index(nameof(EventId), Name = "IX_HistoryLocationTrailer_EventId")]
public class HistoryLocationTrailer : BaseHistoryLocationEntity {

    [Required]
    public Guid TrailerId { get; set; }

    [Required]
    public override int EventId { get; set; }

    [ForeignKey("TrailerId")]
    public Trailer Trailer { get; set; }

    [InverseProperty("HistoryLocationTrailer")]
    public ICollection<HistoryLocationLoad> HistoryLocationLoads { get; set; }
}
