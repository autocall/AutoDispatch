using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Web.Logic.Dtos.Dispatch;

/// <summary>
///     Режим страницы setup packet (V1 PageModeEnum): User — карриер по ссылке из письма, View/Edit — агент с сайта </summary>
public enum PageModeEnum {
    User,
    View,
    Edit
}

/// <summary>
///     Модели шагов CarrierRegistration (V1 AutoCall.Logic.Contracts/Models/SetupPacketStep*Model): поля формы шага, маппятся
///     в SetupPacketDto и обратно (DispatchMappingProfile). Шаг 13 (файлы) — multipart, см. CarrierRegistrationController </summary>
public abstract class SetupPacketStepModel {
    /// <summary>
    ///     Шаг страницы (ViewStep V1); следующий шаг считает SetupPacketViewModel.Next </summary>
    public byte Step { get; set; }
    public PageModeEnum Mode { get; set; }
}

public class SetupPacketStep00Model : SetupPacketStepModel {
    public Guid? ConfidantContactId { get; set; }
    public SetupPacketContactDto ConfidantContact { get; set; }
}

public class SetupPacketStep01Model : SetupPacketStepModel { }

public class SetupPacketStep02Model : SetupPacketStepModel {
    public string LegalName { get; set; }

    #region [ Physical Address ]
    public string PhysicalAddress { get; set; }
    public string PhysicalCity { get; set; }
    public byte? PhysicalStateId { get; set; }
    public string PhysicalZipCode { get; set; }
    public byte? PhysicalCountryId { get; set; }
    #endregion [ Physical Address ]

    #region [ Mailing Address ]
    public string MailingCompanyLegalName { get; set; }
    public string MailingAddress { get; set; }
    public string MailingCity { get; set; }
    public byte? MailingStateId { get; set; }
    public string MailingZipCode { get; set; }
    public byte? MailingCountryId { get; set; }
    public string MailingEmail { get; set; }
    public string DunsNumber { get; set; }
    #endregion [ Mailing Address ]

    #region [ Factoring Company ]
    public bool UseFactoringCompany { get; set; }
    public short? FactoringCompanyId { get; set; }
    #endregion [ Factoring Company ]

    #region [ Contacts ]
    public string SalesSameAs { get; set; }
    public string DispatchSameAs { get; set; }
    public string AfterHourSameAs { get; set; }
    public Guid? CorporateContactId { get; set; }
    public Guid? SalesContactId { get; set; }
    public Guid? DispatchContactId { get; set; }
    public Guid? AfterHourContactId { get; set; }
    public SetupPacketContactDto CorporateContact { get; set; }
    public SetupPacketContactDto SalesContact { get; set; }
    public SetupPacketContactDto DispatchContact { get; set; }
    public SetupPacketContactDto AfterHourContact { get; set; }
    #endregion [ Contacts ]
}

public class SetupPacketStep04Model : SetupPacketStepModel {
    public bool AgreementAgree { get; set; }
    public DateTime? AgreementDate { get; set; }
    public string Agreement_02Signature { get; set; }
    public DateTime? Agreement_02SignatureDate { get; set; }
    public DateTime? Agreement_02Date { get; set; }
    public string Agreement2_02Signature { get; set; }
}

public class SetupPacketStep05Model : SetupPacketStepModel { }

public class SetupPacketStep06Model : SetupPacketStepModel {
    public string AutoSameAs { get; set; }
    public string CargoSameAs { get; set; }
    public string WorkersCompensationSameAs { get; set; }
    public Guid? GeneralLiabilityInsuranceAgencyId { get; set; }
    public Guid? AutoInsuranceAgencyId { get; set; }
    public Guid? CargoInsuranceAgencyId { get; set; }
    public Guid? WorkersCompensationInsuranceAgencyId { get; set; }
    public string WorkersCompensationSignature { get; set; }
    public DateTime? WorkersCompensationSignatureDate { get; set; }
    public SetupPacketInsuranceAgencyDto GeneralLiabilityInsuranceAgency { get; set; }
    public SetupPacketInsuranceAgencyDto AutoInsuranceAgency { get; set; }
    public SetupPacketInsuranceAgencyDto CargoInsuranceAgency { get; set; }
    public SetupPacketInsuranceAgencyDto WorkersCompensationInsuranceAgency { get; set; }
}

public class SetupPacketStep07Model : SetupPacketStepModel {
    public string W9Name { get; set; }
    public string W9BusinessName { get; set; }
    public byte? W9CompanyType { get; set; }
    public byte? W9TaxClassification { get; set; }
    public string W9OtherTaxClassification { get; set; }
    public bool W9ForeignPartnersHas { get; set; }
    public byte? W9ExemptPayeeCode { get; set; }
    public char? W9FATCACode { get; set; }
    public string W9Address { get; set; }
    public string W9City { get; set; }
    public byte? W9StateId { get; set; }
    public string W9ZipCode { get; set; }
    public string W9NamePerson { get; set; }
    public string W9AccountNumber { get; set; }
    public string W9SocialSecurityNumber { get; set; }
    public string W9EmployerIdentificationNumber { get; set; }
    public bool W9Agree { get; set; }
    public DateTime? W9AgreeDate { get; set; }
    public string W9Signature { get; set; }
}

/// <summary>
///     Файлы MC и COI приходят multipart (IFormFile) и сохраняются SetupPacketService.SaveStepFileAsync; в DTO — только имена </summary>
public class SetupPacketStep13Model : SetupPacketStepModel { }

public class SetupPacketStep14Model : SetupPacketStepModel { }
