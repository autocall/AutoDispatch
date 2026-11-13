using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Setup packet карриера (15 шагов CarrierRegistration). StateId/CountryId — byte-ключи справочника в коде,
///     таблиц State/Country и FK нет (решено 2026-11-13, AI/wiki/plan.md) </summary>
public class SetupPacket : BaseObjectEntity {

    public byte Version { get; set; } // version in SetupPacket_DtoToModel.cs

    [MaxLength(64)]
    public string LegalName { get; set; }

    [MaxLength(64)]
    public string DBAName { get; set; }

    public byte Step { get; set; }

    #region [ Physical Address ]

    [MaxLength(64)]
    public string PhysicalAddress { get; set; }

    [MaxLength(32)]
    public string PhysicalCity { get; set; }

    public byte? PhysicalStateId { get; set; }

    [MaxLength(16)]
    public string PhysicalZipCode { get; set; }

    public byte? PhysicalCountryId { get; set; }

    #endregion [ Physical Address ]

    #region [ Mailing Address ]

    [MaxLength(64)]
    public string MailingCompanyLegalName { get; set; }

    [MaxLength(64)]
    public string MailingAddress { get; set; }

    [MaxLength(32)]
    public string MailingCity { get; set; }

    public byte? MailingStateId { get; set; }

    [MaxLength(16)]
    public string MailingZipCode { get; set; }

    public byte? MailingCountryId { get; set; }

    [MaxLength(64)]
    public string MailingEmail { get; set; }

    [MaxLength(16)]
    public string DunsNumber { get; set; }

    #endregion [ Mailing Address ]

    #region [ Factoring Company ]

    public bool UseFactoringCompany { get; set; }

    public short? FactoringCompanyId { get; set; }

    #endregion [ Factoring Company ]

    #region [ Contact IDs ]

    public Guid? ConfidantContactId { get; set; }

    public Guid? CorporateContactId { get; set; }

    public Guid? SalesContactId { get; set; }

    public Guid? DispatchContactId { get; set; }

    public Guid? AfterHourContactId { get; set; }

    #endregion [ Contact IDs ]

    #region [ Step2 ]

    public string SalesSameAs { get; set; }

    public string DispatchSameAs { get; set; }

    public string AfterHourSameAs { get; set; }

    #endregion [ Step2 ]

    #region [ Step3 ]

    [MaxLength(16)]
    public string CVORNumber { get; set; }

    [MaxLength(16)]
    public string NIRNumber { get; set; }

    [MaxLength(16)]
    public string NSCNumber { get; set; }

    public byte? CountryServiceType { get; set; }

    public bool? AuthorizedToHaulElectronics { get; set; }

    public bool? HasReferBreakdown { get; set; }

    public bool? HasNonOwnedAuto { get; set; }

    public bool? HasActiveShipmentAtTimeOfReg { get; set; }

    public bool AllowAcquirePolicyExclusions { get; set; }

    #endregion [ Step3 ]

    #region [ Step4 ]

    /// <summary>
    ///     v.1 </summary>
    public bool AgreementAgree { get; set; }

    /// <summary>
    ///     v.1 </summary>
    [Column(TypeName = "datetime2")]
    public DateTime? AgreementDate { get; set; }

    /// <summary>
    ///     v.2 </summary>
    [MaxLength(64)]
    public string Agreement_02Signature { get; set; }

    /// <summary>
    ///     v.2 </summary>
    [Column(TypeName = "datetime2")]
    public DateTime? Agreement_02SignatureDate { get; set; }

    /// <summary>
    ///     v.2 </summary>
    [Column(TypeName = "datetime2")]
    public DateTime? Agreement_02Date { get; set; }

    /// <summary>
    ///     v.2 </summary>
    [MaxLength(64)]
    public string Agreement2_02Signature { get; set; }

    #endregion [ Step4 ]

    #region [ Step6 ]

    public string AutoSameAs { get; set; }

    public string CargoSameAs { get; set; }

    public string WorkersCompensationSameAs { get; set; }

    public Guid? GeneralLiabilityInsuranceAgencyId { get; set; }

    public Guid? AutoInsuranceAgencyId { get; set; }

    public Guid? CargoInsuranceAgencyId { get; set; }

    public Guid? WorkersCompensationInsuranceAgencyId { get; set; }

    [MaxLength(64)]
    public string WorkersCompensationSignature { get; set; }

    public DateTime? WorkersCompensationSignatureDate { get; set; }

    #endregion [ Step6 ]

    #region [ Step7 ]

    [MaxLength(64)]
    public string W9Name { get; set; }

    [MaxLength(64)]
    public string W9BusinessName { get; set; }

    public byte? W9CompanyType { get; set; }

    public byte? W9TaxClassification { get; set; }

    public string W9OtherTaxClassification { get; set; }

    public byte? W9ExemptPayeeCode { get; set; }

    public byte? W9FATCACode { get; set; }

    [MaxLength(64)]
    public string W9Address { get; set; }

    [MaxLength(32)]
    public string W9City { get; set; }

    public byte? W9StateId { get; set; }

    [MaxLength(16)]
    public string W9ZipCode { get; set; }

    public string W9NamePerson { get; set; }

    public string W9AccountNumber { get; set; }

    [MaxLength(16)]
    public string W9SocialSecurityNumber { get; set; }

    [MaxLength(16)]
    public string W9EmployerIdentificationNumber { get; set; }

    public bool W9Agree { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? W9AgreeDate { get; set; }

    [MaxLength(64)]
    public string W9Signature { get; set; }

    #endregion [ Step7 ]

    #region [ Step8 ]

    [MaxLength(2)]
    public byte[] Step8Modes { get; set; }

    [MaxLength(4)]
    public byte[] Step8Commodities { get; set; }

    public short TractorsCount { get; set; }

    [MaxLength(4)]
    public string SCACCode { get; set; }

    public short? DriversCount { get; set; }

    public short? TeamsCount { get; set; }

    public short? OperatorsCount { get; set; }

    [MaxLength(32)]
    public string OwnerName { get; set; }

    [MaxLength(32)]
    public string OwnerPhone { get; set; }

    [MaxLength(64)]
    public string OwnerEmail { get; set; }

    public bool HasMexico { get; set; }

    public bool HasCanadian { get; set; }

    public bool HasEmailCarrier { get; set; }

    public bool? CertificationSmartWay { get; set; }

    public bool? CertificationC_TPAT { get; set; }

    public bool? CertificationFAST { get; set; }

    public bool? CertificationTWIC { get; set; }

    public bool? CertificationCARB { get; set; }

    public bool? CertificationHazMatCertification { get; set; }

    public bool? CertificationSafetyPermit { get; set; }

    public bool SafetyAgree { get; set; }

    [DefaultValue(1)]
    public byte LoadUrgency { get; set; }

    #endregion [ Step8 ]

    #region [ Step9 ]

    [MaxLength(80)]
    public byte[] NumberOfUnits { get; set; }

    #endregion [ Step9 ]

    #region [ Step10 ]

    [InverseProperty("SetupPacket")]
    public ICollection<SetupPacketReference> References { get; set; }

    #endregion [ Step10 ]

    #region [ Step11 ]

    public byte[] W8States { get; set; }

    #endregion [ Step11 ]

    #region [ Step12 ]

    public bool? Diversity1M_WBE { get; set; }

    public byte? Diversity1 { get; set; }

    public byte? Diversity2 { get; set; }

    [MaxLength(256)]
    public string Diversity2Other { get; set; }

    public bool? Diversity3Certified { get; set; }

    public byte? Diversity3 { get; set; }

    [MaxLength(256)]
    public string Diversity3Other { get; set; }

    #endregion [ Step12 ]

    #region [ Step13 ]

    public string FileMC { get; set; }

    public string FileCOI { get; set; }

    #endregion [ Step13 ]

    #region [ Last ]

    public DateTime? EmailSent { get; set; }

    #endregion [ Last ]

    #region [ references ]

    [ForeignKey("ConfidantContactId")]
    public SetupPacketContact ConfidantContact { get; set; }

    [ForeignKey("CorporateContactId")]
    public SetupPacketContact CorporateContact { get; set; }

    [ForeignKey("SalesContactId")]
    public SetupPacketContact SalesContact { get; set; }

    [ForeignKey("DispatchContactId")]
    public SetupPacketContact DispatchContact { get; set; }

    [ForeignKey("AfterHourContactId")]
    public SetupPacketContact AfterHourContact { get; set; }

    [ForeignKey("GeneralLiabilityInsuranceAgencyId")]
    public SetupPacketInsuranceAgency GeneralLiabilityInsuranceAgency { get; set; }

    [ForeignKey("AutoInsuranceAgencyId")]
    public SetupPacketInsuranceAgency AutoInsuranceAgency { get; set; }

    [ForeignKey("CargoInsuranceAgencyId")]
    public SetupPacketInsuranceAgency CargoInsuranceAgency { get; set; }

    [ForeignKey("WorkersCompensationInsuranceAgencyId")]
    public SetupPacketInsuranceAgency WorkersCompensationInsuranceAgency { get; set; }

    #endregion [ references ]
}
