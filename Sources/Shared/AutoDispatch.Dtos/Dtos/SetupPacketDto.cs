using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class SetupPacketDto : BaseObjectDto, IBaseEventDto {

        [DataMember] public byte Version { get; set; }
      
        [DataMember] public int EventId { get; set; }

        [DataMember] public string LegalName { get; set; }

        [DataMember] public string DBAName { get; set; }

        [DataMember] public byte Step { get; set; }

        #region [ Physical Address ]

        [DataMember] public string PhysicalAddress { get; set; }

        [DataMember] public string PhysicalCity { get; set; }

        [DataMember] public byte? PhysicalStateId { get; set; }


        /// <summary>
        ///     Extra </summary>
        [DataMember] public string PhysicalState { get; set; }

        [DataMember] public string PhysicalZipCode { get; set; }

        [DataMember] public byte? PhysicalCountryId { get; set; }

        #endregion [ Physical Address ]

        #region [ Mailing Address ]

        [DataMember] public string MailingCompanyLegalName { get; set; }

        [DataMember] public string MailingAddress { get; set; }

        [DataMember] public string MailingCity { get; set; }

        [DataMember] public byte? MailingStateId { get; set; }

        [DataMember] public string MailingZipCode { get; set; }

        [DataMember] public byte? MailingCountryId { get; set; }

        [DataMember] public string MailingEmail { get; set; }

        [DataMember] public string DunsNumber { get; set; }

        #endregion [ Mailing Address ]

        #region [ Factoring Company ]

        [DataMember] public bool UseFactoringCompany { get; set; }

        [DataMember] public short? FactoringCompanyId { get; set; }

        #endregion [ Factoring Company ]

        #region [ Contacts ]

        [DataMember] public Guid? ConfidantContactId { get; set; }
        [DataMember] public Guid? CorporateContactId { get; set; }
        [DataMember] public Guid? SalesContactId { get; set; }
        [DataMember] public Guid? DispatchContactId { get; set; }
        [DataMember] public Guid? AfterHourContactId { get; set; }

        [DataMember] public SetupPacketContactDto ConfidantContact { get; set; }
        [DataMember] public SetupPacketContactDto CorporateContact { get; set; }
        [DataMember] public SetupPacketContactDto SalesContact { get; set; }
        [DataMember] public SetupPacketContactDto DispatchContact { get; set; }
        [DataMember] public SetupPacketContactDto AfterHourContact { get; set; }

        #endregion [ Contacts ]

        #region [ Step2 ]

        [DataMember] public string SalesSameAs { get; set; }
        [DataMember] public string DispatchSameAs { get; set; }
        [DataMember] public string AfterHourSameAs { get; set; }

        #endregion [ Step2 ]

        #region [ Step3 ]

        [DataMember] public string CVORNumber { get; set; }
        [DataMember] public string NIRNumber { get; set; }
        [DataMember] public string NSCNumber1 { get; set; }
        [DataMember] public string NSCNumber2 { get; set; }

        [DataMember] public byte? CountryServiceType { get; set; }

        [DataMember] public bool? AuthorizedToHaulElectronics { get; set; }
        [DataMember] public bool? HasReferBreakdown { get; set; }
        [DataMember] public bool? HasNonOwnedAuto { get; set; }
        [DataMember] public bool? HasActiveShipmentAtTimeOfReg { get; set; }

        [DataMember] public bool AllowAcquirePolicyExclusions { get; set; }

        #endregion [ Step3 ]

        #region [ Step4 ]

        [DataMember] public bool AgreementAgree { get; set; }
        [DataMember] public DateTime? AgreementDate { get; set; }

        [DataMember] public string Agreement_02Signature { get; set; }
        [DataMember] public DateTime? Agreement_02SignatureDate { get; set; }
        [DataMember] public DateTime? Agreement_02Date { get; set; }
        [DataMember] public string Agreement2_02Signature { get; set; }

        #endregion [ Step4 ]

        #region [ Step6 ]

        [DataMember] public string AutoSameAs { get; set; }
        [DataMember] public string CargoSameAs { get; set; }
        [DataMember] public string WorkersCompensationSameAs { get; set; }

        [DataMember] public Guid? GeneralLiabilityInsuranceAgencyId { get; set; }
        [DataMember] public Guid? AutoInsuranceAgencyId { get; set; }
        [DataMember] public Guid? CargoInsuranceAgencyId { get; set; }
        [DataMember] public Guid? WorkersCompensationInsuranceAgencyId { get; set; }

        [DataMember] public SetupPacketInsuranceAgencyDto GeneralLiabilityInsuranceAgency { get; set; }
        [DataMember] public SetupPacketInsuranceAgencyDto AutoInsuranceAgency { get; set; }
        [DataMember] public SetupPacketInsuranceAgencyDto CargoInsuranceAgency { get; set; }
        [DataMember] public SetupPacketInsuranceAgencyDto WorkersCompensationInsuranceAgency { get; set; }
        [DataMember] public string WorkersCompensationSignature { get; set; }
        [DataMember] public DateTime? WorkersCompensationSignatureDate { get; set; }

        #endregion [ Step6 ]

        #region [ Step7 ]

        [DataMember] public string W9Name { get; set; }

        [DataMember] public string W9BusinessName { get; set; }

        [DataMember] public byte? W9CompanyType { get; set; }
        [DataMember] public byte? W9TaxClassification { get; set; }
        [DataMember] public string W9OtherTaxClassification { get; set; }

        [DataMember] public byte? W9ExemptPayeeCode { get; set; }
        [DataMember] public char? W9FATCACode { get; set; }

        [DataMember] public string W9Address { get; set; }
        [DataMember] public string W9City { get; set; }
        [DataMember] public byte? W9StateId { get; set; }
        [DataMember] public string W9ZipCode { get; set; }
        [DataMember] public string W9NamePerson { get; set; }
        [DataMember] public string W9AccountNumber { get; set; }
        [DataMember] public string W9SocialSecurityNumber { get; set; }
        [DataMember] public string W9EmployerIdentificationNumber { get; set; }

        [DataMember] public bool W9Agree { get; set; }
        [DataMember] public DateTime? W9AgreeDate { get; set; }
        [DataMember] public string W9Signature { get; set; }

        #endregion [ Step7 ]

        #region [ Step8 ]

        [DataMember] public List<bool> Step8Modes { get; set; }
        [DataMember] public List<bool> Step8Commodities { get; set; }

        [DataMember] public short TractorsCount { get; set; }
        [DataMember] public string SCACCode { get; set; }

        [DataMember] public short? DriversCount { get; set; }
        [DataMember] public short? TeamsCount { get; set; }
        [DataMember] public short? OperatorsCount { get; set; }

        [DataMember] public string OwnerName { get; set; }
        [DataMember] public string OwnerPhone { get; set; }
        [DataMember] public string OwnerEmail { get; set; }

        [DataMember] public bool HasMexico { get; set; }
        [DataMember] public bool HasCanadian { get; set; }
        [DataMember] public bool HasEmailCarrier { get; set; }

        [DataMember] public bool? CertificationSmartWay { get; set; }
        [DataMember] public bool? CertificationC_TPAT { get; set; }
        [DataMember] public bool? CertificationFAST { get; set; }
        [DataMember] public bool? CertificationTWIC { get; set; }
        [DataMember] public bool? CertificationCARB { get; set; }
        [DataMember] public bool? CertificationHazMatCertification { get; set; }
        [DataMember] public bool? CertificationSafetyPermit { get; set; }

        [DataMember] public bool SafetyAgree { get; set; }

        [DataMember] public byte LoadUrgency { get; set; }

        #endregion [ Step8 ]

        #region [ Step9 ]

        [DataMember] public List<short> NumberOfUnits { get; set; }

        #endregion [ Step9 ]

        #region [ Step10 ]

        [DataMember] public List<SetupPacketReferenceDto> References { get; set; }

        #endregion [ Step10 ]

        #region [ Step11 ]

        [DataMember] public List<short> W8States { get; set; }

        #endregion [ Step11 ]

        #region [ Step12 ]

        [DataMember] public bool? Diversity1M_WBE { get; set; }
        [DataMember] public byte? Diversity1 { get; set; }

        [DataMember] public byte? Diversity2 { get; set; }
        [DataMember] public string Diversity2Other { get; set; }

        [DataMember] public bool? Diversity3Certified { get; set; }
        [DataMember] public byte? Diversity3 { get; set; }
        [DataMember] public string Diversity3Other { get; set; }

        #endregion [ Step12 ]

        #region [ Step13 ]

        [DataMember] public string FileMC { get; set; }
        [DataMember] public string FileCOI { get; set; }

        #endregion [ Step13 ]

        #region [ Last ]

        [DataMember] public DateTime? EmailSent { get; set; }

        #endregion [ Last ]

    }
}
