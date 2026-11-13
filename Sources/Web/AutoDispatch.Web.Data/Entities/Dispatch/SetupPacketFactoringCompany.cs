using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Справочник факторинговых компаний для setup packet; StateId/CountryId — ключи справочника в коде, без FK </summary>
public class SetupPacketFactoringCompany {

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int EntityId { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; }

    [MaxLength(64)]
    public string Address { get; set; }

    [MaxLength(64)]
    public string Name { get; set; }

    [MaxLength(32)]
    public string City { get; set; }

    [MaxLength(16)]
    public string ZipCode { get; set; }

    [MaxLength(64)]
    public string Email { get; set; }

    /// <summary>
    ///     Identifier of state in current db </summary>
    public byte StateId { get; set; }

    /// <summary>
    ///     Country ID </summary>
    public byte CountryId { get; set; }
}
