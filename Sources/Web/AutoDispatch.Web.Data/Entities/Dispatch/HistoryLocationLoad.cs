using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

[Index(nameof(EventId), Name = "IX_HistoryLocationLoad_EventId")]
public class HistoryLocationLoad : BaseHistoryLocationEntity {

    [Required]
    public Guid LoadId { get; set; }

    public int? HistoryLocationTrailerId { get; set; }

    public int? HistoryLocationDatDriverId { get; set; }

    public int? HistoryDatCompanyId { get; set; }

    public int? HistoryShipmentId { get; set; }

    public int? HistoryWarehouseId { get; set; }

    [Required]
    public override int EventId { get; set; }

    [ForeignKey("LoadId")]
    public Load Load { get; set; }

    [ForeignKey("HistoryLocationTrailerId")]
    public HistoryLocationTrailer HistoryLocationTrailer { get; set; }

    [ForeignKey("HistoryLocationDatDriverId")]
    public HistoryLocationDatDriver HistoryLocationDatDriver { get; set; }

    [ForeignKey("HistoryDatCompanyId")]
    public HistoryObject HistoryDatCompany { get; set; }

    [ForeignKey("HistoryWarehouseId")]
    public HistoryObject HistoryWarehouse { get; set; }
}
