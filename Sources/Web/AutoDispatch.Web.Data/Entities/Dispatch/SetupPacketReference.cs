using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

public class SetupPacketReference {

    [Key]
    public Guid EntityId { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; }

    [MaxLength(32)]
    public string Contact { get; set; }

    [MaxLength(32)]
    public string Company { get; set; }

    [MaxLength(32)]
    public string Phone { get; set; }

    [MaxLength(64)]
    public string Email { get; set; }

    public Guid SetupPacketId { get; set; }

    [ForeignKey("SetupPacketId")]
    public SetupPacket SetupPacket { get; set; }
}
