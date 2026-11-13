using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Индекс город/компания по грузам для FixLocationLoadsService </summary>
[Index(nameof(City), Name = "IX_CacheLoadLink_City")]
[Index(nameof(Company), Name = "IX_CacheLoadLink_Company")]
public class CacheLoadIndex {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    ///     Default getdate() — CacheLoadIndexMap </summary>
    [Required, Column(TypeName = "datetime2"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedDate { get; set; }

    public Guid CompanyId { get; set; }

    public Guid LoadId { get; set; }

    [MaxLength(16), Required]
    public string City { get; set; }

    [MaxLength(16), Required]
    public string Company { get; set; }

    /// <summary>
    ///     Origin / Destination </summary>
    public byte Type { get; set; }

    [ForeignKey("LoadId")]
    public Load Load { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company1 { get; set; }
}
