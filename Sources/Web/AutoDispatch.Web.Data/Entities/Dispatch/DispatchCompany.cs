using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Профиль компании в dispatch (V1 d_ActualCompany, переименовано 2026-11-13): реквизиты, SMTP, нумерация, BNSF, CarrierView </summary>
public class DispatchCompany : BaseEntity {

    [Required]
    public string LogoSmall { get; set; }

    [Required]
    public string LogoBig { get; set; }

    public int McNumber { get; set; }

    public Guid CompanyId { get; set; }

    [MaxLength(64)]
    public string LegalName { get; set; }

    [MaxLength(128)]
    public string PhysicalAddress { get; set; }

    [MaxLength(128)]
    public string MailingAddress { get; set; }

    [MaxLength(32)]
    public string ShippersPhone { get; set; }

    [MaxLength(32)]
    public string CellPhone { get; set; }

    [MaxLength(32)]
    public string WorkPhone { get; set; }

    [MaxLength(32)]
    public string WorkFax { get; set; }

    [MaxLength(64)]
    public string WorkEmail { get; set; }

    public string PaymentInstructions { get; set; }

    [MaxLength(64)]
    public string SMTP_Email { get; set; }

    [MaxLength(32)]
    public string SMTP_Password { get; set; }

    [MaxLength(64)]
    public string SMTP_Host { get; set; }

    public int SMTP_Port { get; set; }

    [MaxLength(16)]
    public string SMS_Phone { get; set; }

    [MaxLength(256)]
    public string ConfirmationFile { get; set; }

    [MaxLength(256)]
    public string BOLFile { get; set; }

    [MaxLength(256)]
    public string InvoiceFile { get; set; }

    [MaxLength(64)]
    public string BnsfLogin { get; set; }

    [MaxLength(64)]
    public string BnsfPassword { get; set; }

    public string BnsfLastMessage { get; set; }

    public DateTime? BnsfLastDone { get; set; }

    public string BnsfCookies { get; set; }

    public bool SubmitRates { get; set; }

    [MaxLength(64)]
    public string SMTP_SchemeEmail { get; set; }

    [MaxLength(32)]
    public string SMTP_SchemePassword { get; set; }

    [MaxLength(64)]
    public string SMTP_SchemeHost { get; set; }

    public int SMTP_SchemePort { get; set; }

    /// <summary>
    ///     Carrier or Broker. </summary>
    /// <remarks> The Value takes from Selector and doesn't WEB-service, because WEB-service has mix types </remarks>
    [Required]
    public byte BoardType { get; set; }

    public int NextConfirmationNumber { get; set; }

    public int NextInvoiceNumber { get; set; }

    public int NextLoadNumber { get; set; }

    public bool InvoiceNumberAsLoadNumber { get; set; }

    [MaxLength(128)]
    public byte[] NextLoadExtra { get; set; }

    [MaxLength(32)]
    public string CarrierViewToken { get; set; }

    public string FactoringCompanyJson { get; set; }

    [ForeignKey("CompanyId")]
    public Company Company { get; set; }

    [InverseProperty("DispatchCompany")]
    public ICollection<DispatchCompanyCustom> Customizes { get; set; }
}
