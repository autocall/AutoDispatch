using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class Warehouse : BaseObjectEntity {

    [MaxLength(32), Required]
    public string Name { get; set; }

    [MaxLength(256), Required]
    public string Address { get; set; }

    [MaxLength(128), Required]
    public string City { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    [InverseProperty("Warehouse")]
    public ICollection<Load> Loads { get; set; }
}
