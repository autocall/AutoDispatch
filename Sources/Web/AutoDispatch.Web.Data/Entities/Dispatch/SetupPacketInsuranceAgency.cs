using System.ComponentModel.DataAnnotations;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class SetupPacketInsuranceAgency {

    [Key]
    public Guid EntityId { get; set; }

    [MaxLength(32)]
    public string CompanyName { get; set; }

    [MaxLength(32)]
    public string Phone { get; set; }

    [MaxLength(32)]
    public string Fax { get; set; }

    [MaxLength(64)]
    public string Email { get; set; }

    [MaxLength(32)]
    public string Number { get; set; }
}
