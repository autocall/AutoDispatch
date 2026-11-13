using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class Defect : BaseEntity {

    public Guid CompanyId { get; set; }

    public Guid? TrailerId { get; set; }

    public Guid? TruckId { get; set; }

    public Guid? RepairId { get; set; }

    public int DefectId { get; set; }

    public string Description { get; set; }

    public bool ServiceNeeded { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }

    [ForeignKey("TrailerId")]
    public Trailer Trailer { get; set; }

    [ForeignKey("TruckId")]
    public Truck Truck { get; set; }

    [ForeignKey("RepairId")]
    public DefectRepair Repair { get; set; }
}
