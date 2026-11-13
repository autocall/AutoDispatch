using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class DefectRepair : BaseEntity {

    public Guid CompanyId { get; set; }

    public DateTime Date { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }

    public string Comment { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }
}
