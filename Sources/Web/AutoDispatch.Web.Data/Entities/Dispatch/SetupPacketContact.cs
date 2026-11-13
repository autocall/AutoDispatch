using System.ComponentModel.DataAnnotations;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class SetupPacketContact {

    [Key]
    public Guid EntityId { get; set; }

    [MaxLength(32)]
    public string Contact { get; set; }

    [MaxLength(32)]
    public string Phone { get; set; }

    [MaxLength(64)]
    public string Email { get; set; }

    [MaxLength(32)]
    public string Title { get; set; }

    [MaxLength(32)]
    public string Cell { get; set; }

    [MaxLength(32)]
    public string Fax { get; set; }
}
