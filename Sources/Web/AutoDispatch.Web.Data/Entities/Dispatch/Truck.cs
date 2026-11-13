using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class Truck : BaseDispatchEntity {

    [MaxLength(32), Required]
    public string Number { get; set; }

    public Guid? TrailerId { get; set; }

    [ForeignKey("TrailerId")]
    public Trailer Trailer { get; set; }

    #region [ Files ]

    [MaxLength(256)]
    public string AnnualInspection_File { get; set; }

    [Column(TypeName = "date")]
    public DateTime? AnnualInspection_Expiration { get; set; }

    [MaxLength(256)]
    public string Registration_File { get; set; }

    [Column(TypeName = "date")]
    public DateTime? Registration_Expiration { get; set; }

    #endregion [ Files ]
}
