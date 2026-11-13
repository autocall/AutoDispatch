using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Карриер / брокер (контрагент) с флагами setup packet </summary>
[Index(nameof(McNumber))]
public class DatCompany : BaseObjectEntity {

    public Guid? DispatchCompanyId { get; set; }

    public int? McNumber { get; set; }

    public int? DotNumber { get; set; }

    [Required, MaxLength(64)]
    public string Name { get; set; }

    public byte EntityType { get; set; }

    public string Contacts { get; set; }

    public bool Phy_UpsStore { get; set; }

    public bool Mai_UpsStore { get; set; }

    public bool Phy_MailBox { get; set; }

    public bool Mai_MailBox { get; set; }

    [Column(TypeName = "date")]
    public DateTime? InsuranceExpiration { get; set; }

    [MaxLength(256)]
    public string InsuranceFile { get; set; }

    public string Comments { get; set; }

    public byte Rating { get; set; }

    [MaxLength(256)]
    public string HyperlinkFile { get; set; }

    public bool DoNotUse { get; set; }

    #region [ setup packet ]

    public Guid? SetupPacketId { get; set; }

    /// <summary>
    ///     JSON of emails </summary>
    [MaxLength(256)]
    public string SetupPacketEmailSent { get; set; }

    public bool SetupPacketSent { get; set; }

    public bool SetupPacketCompleted { get; set; }

    public bool SetupPacketAccept { get; set; }

    [MaxLength(256)]
    public string SetupPacketFile { get; set; }

    [MaxLength(256)]
    public string W9File { get; set; }

    #endregion [ setup packet ]

    [ForeignKey("DispatchCompanyId")]
    public DispatchCompany DispatchCompany1 { get; set; }

    [ForeignKey("SetupPacketId")]
    public SetupPacket SetupPacket1 { get; set; }

    [InverseProperty("DatCompany")]
    public ICollection<Load> Loads { get; set; }
}
