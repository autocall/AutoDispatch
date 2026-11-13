using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

/// <summary>
///     V1 SetupPacket_DtoToModel: контакты и агентства — только Id (сами записи сохраняет SetupPacketService) </summary>
public class SetupPacket_DtoToModel : TypeConverter<SetupPacketDto, SetupPacket> {
    protected override SetupPacket ConvertCore(SetupPacketDto source) {
        var destination = new SetupPacket();

        // sets default version
        destination.Version = source.Version;
        destination.CompanyId = source.CompanyId;
        destination.EntityId = source.EntityId;
        destination.CreatedById = source.CreatedById;
        destination.CreatedDate = source.CreatedDate;
        destination.LastModifiedById = source.LastModifiedById;
        destination.LastModifiedDate = source.LastModifiedDate;
        destination.IsDeleted = source.IsDeleted;
        destination.HistoryId = source.HistoryId;

        destination.LegalName = source.LegalName;
        destination.DBAName = source.DBAName;
        destination.Step = source.Step;
        destination.PhysicalAddress = source.PhysicalAddress;
        destination.PhysicalCity = source.PhysicalCity;
        destination.PhysicalStateId = source.PhysicalStateId;
        destination.PhysicalZipCode = source.PhysicalZipCode;
        destination.PhysicalCountryId = source.PhysicalCountryId;

        destination.MailingCompanyLegalName = source.MailingCompanyLegalName;
        destination.MailingAddress = source.MailingAddress;
        destination.MailingCity = source.MailingCity;
        destination.MailingStateId = source.MailingStateId;
        destination.MailingZipCode = source.MailingZipCode;
        destination.MailingCountryId = source.MailingCountryId;
        destination.MailingEmail = source.MailingEmail;
        destination.DunsNumber = source.DunsNumber;

        destination.UseFactoringCompany = source.UseFactoringCompany;
        destination.FactoringCompanyId = source.FactoringCompanyId;

        destination.SalesSameAs = source.SalesSameAs;
        destination.DispatchSameAs = source.DispatchSameAs;
        destination.AfterHourSameAs = source.AfterHourSameAs;

        destination.ConfidantContactId = source.ConfidantContactId;
        destination.CorporateContactId = source.CorporateContactId;
        destination.SalesContactId = source.SalesContactId;
        destination.DispatchContactId = source.DispatchContactId;
        destination.AfterHourContactId = source.AfterHourContactId;

        if (source.CorporateContact != null && source.CorporateContact.HasContact == false) {
            destination.CorporateContactId = null;
        }
        if (source.SalesContact != null && source.SalesContact.HasContact == false) {
            destination.SalesContactId = null;
        }
        if (source.DispatchContact != null && source.DispatchContact.HasContact == false) {
            destination.DispatchContactId = null;
        }
        if (source.AfterHourContact != null && source.AfterHourContact.HasContact == false) {
            destination.AfterHourContactId = null;
        }

        #region [ Step 3 ]

        destination.CVORNumber = source.CVORNumber;
        destination.NIRNumber = source.NIRNumber;
        if (source.NSCNumber1 != null && source.NSCNumber2 != null) {
            destination.NSCNumber = $"{source.NSCNumber1} {source.NSCNumber2}";
        }

        destination.CountryServiceType = source.CountryServiceType;

        destination.AuthorizedToHaulElectronics = source.AuthorizedToHaulElectronics;
        destination.HasReferBreakdown = source.HasReferBreakdown;
        destination.HasNonOwnedAuto = source.HasNonOwnedAuto;
        destination.HasActiveShipmentAtTimeOfReg = source.HasActiveShipmentAtTimeOfReg;

        destination.AllowAcquirePolicyExclusions = source.AllowAcquirePolicyExclusions;

        #endregion [ Step 3 ]

        #region [ Step 4 ]

        destination.AgreementAgree = source.AgreementAgree;
        destination.AgreementDate = source.AgreementDate;

        destination.Agreement_02Signature = source.Agreement_02Signature;
        destination.Agreement_02SignatureDate = source.Agreement_02SignatureDate;
        destination.Agreement_02Date = source.Agreement_02Date;
        destination.Agreement2_02Signature = source.Agreement2_02Signature;

        #endregion [ Step 4 ]

        #region [ Step 5 ]

        destination.GeneralLiabilityInsuranceAgencyId = source.GeneralLiabilityInsuranceAgencyId;
        destination.AutoInsuranceAgencyId = source.AutoInsuranceAgencyId;
        destination.CargoInsuranceAgencyId = source.CargoInsuranceAgencyId;
        destination.WorkersCompensationInsuranceAgencyId = source.WorkersCompensationInsuranceAgencyId;
        destination.WorkersCompensationSignature = source.WorkersCompensationSignature;
        destination.WorkersCompensationSignatureDate = source.WorkersCompensationSignatureDate;

        if (source.GeneralLiabilityInsuranceAgency != null && source.GeneralLiabilityInsuranceAgency.HasInsurance == false) {
            destination.GeneralLiabilityInsuranceAgencyId = null;
        }
        if (source.AutoInsuranceAgency != null && source.AutoInsuranceAgency.HasInsurance == false) {
            destination.AutoInsuranceAgencyId = null;
        }
        if (source.CargoInsuranceAgency != null && source.CargoInsuranceAgency.HasInsurance == false) {
            destination.CargoInsuranceAgencyId = null;
        }
        if (source.WorkersCompensationInsuranceAgency != null && source.WorkersCompensationInsuranceAgency.HasInsurance == false) {
            destination.WorkersCompensationInsuranceAgencyId = null;
        }

        #endregion [ Step 5 ]

        #region [ Step 6 ]

        destination.AutoSameAs = source.AutoSameAs;
        destination.CargoSameAs = source.CargoSameAs;
        destination.WorkersCompensationSameAs = source.WorkersCompensationSameAs;

        #endregion [ Step 6 ]

        #region [ Step 7 ]

        destination.W9Name = source.W9Name;
        destination.W9BusinessName = source.W9BusinessName;
        destination.W9CompanyType = source.W9CompanyType;
        destination.W9TaxClassification = source.W9TaxClassification;
        destination.W9OtherTaxClassification = source.W9OtherTaxClassification;
        destination.W9ExemptPayeeCode = source.W9ExemptPayeeCode;
        destination.W9FATCACode = (byte?)source.W9FATCACode;
        destination.W9Address = source.W9Address;
        destination.W9City = source.W9City;
        destination.W9StateId = source.W9StateId;
        destination.W9ZipCode = source.W9ZipCode;
        destination.W9NamePerson = source.W9NamePerson;
        destination.W9AccountNumber = source.W9AccountNumber;
        destination.W9SocialSecurityNumber = source.W9SocialSecurityNumber;
        destination.W9EmployerIdentificationNumber = source.W9EmployerIdentificationNumber;
        destination.W9Agree = source.W9Agree;
        destination.W9AgreeDate = source.W9AgreeDate;
        destination.W9Signature = source.W9Signature;

        #endregion [ Step 7 ]

        #region [ Step8 ]

        if (source.Step8Modes != null)
            destination.Step8Modes = SetupPacketBinaryHelper.BackConvert<bool>(source.Step8Modes);
        if (source.Step8Commodities != null)
            destination.Step8Commodities = SetupPacketBinaryHelper.BackConvert<bool>(source.Step8Commodities);

        destination.TractorsCount = source.TractorsCount;
        destination.SCACCode = source.SCACCode;

        destination.DriversCount = source.DriversCount;
        destination.TeamsCount = source.TeamsCount;
        destination.OperatorsCount = source.OperatorsCount;

        destination.OwnerName = source.OwnerName;
        destination.OwnerPhone = source.OwnerPhone;
        destination.OwnerEmail = source.OwnerEmail;

        destination.HasMexico = source.HasMexico;
        destination.HasCanadian = source.HasCanadian;
        destination.HasEmailCarrier = source.HasEmailCarrier;

        destination.CertificationSmartWay = source.CertificationSmartWay;
        destination.CertificationC_TPAT = source.CertificationC_TPAT;
        destination.CertificationFAST = source.CertificationFAST;
        destination.CertificationTWIC = source.CertificationTWIC;
        destination.CertificationCARB = source.CertificationCARB;
        destination.CertificationHazMatCertification = source.CertificationHazMatCertification;
        destination.CertificationSafetyPermit = source.CertificationSafetyPermit;

        destination.SafetyAgree = source.SafetyAgree;

        destination.LoadUrgency = source.LoadUrgency;

        #endregion [ Step8 ]

        #region [ Step9 ]

        if (source.NumberOfUnits != null)
            destination.NumberOfUnits = SetupPacketBinaryHelper.BackConvert<short>(source.NumberOfUnits);

        #endregion [ Step9 ]

        // Step10: References сохраняет SetupPacketService отдельной таблицей

        #region [ Step11 ]

        if (source.W8States != null)
            destination.W8States = SetupPacketBinaryHelper.BackConvert<short>(source.W8States);

        #endregion [ Step11 ]

        #region [ Step12 ]

        destination.Diversity1M_WBE = source.Diversity1M_WBE;
        destination.Diversity1 = source.Diversity1;

        destination.Diversity2 = source.Diversity2;
        destination.Diversity2Other = source.Diversity2Other;

        destination.Diversity3Certified = source.Diversity3Certified;
        destination.Diversity3 = source.Diversity3;
        destination.Diversity3Other = source.Diversity3Other;

        #endregion [ Step12 ]

        destination.FileMC = source.FileMC;
        destination.FileCOI = source.FileCOI;

        destination.EmailSent = source.EmailSent;

        return destination;
    }
}

/// <summary>
///     V1 SetupPacket_ModelToDto: контакты, агентства и References — из навигаций, если загружены (SetupPacketService.LoadRelationsAsync) </summary>
public class SetupPacket_ModelToDto : TypeConverter<SetupPacket, SetupPacketDto> {
    protected override SetupPacketDto ConvertCore(SetupPacket source) {
        var mapper = Context.Mapper;

        var destination = new SetupPacketDto();
        destination.Version = source.Version;
        destination.CompanyId = source.CompanyId;
        destination.EntityId = source.EntityId;
        destination.CreatedById = source.CreatedById;
        destination.CreatedDate = source.CreatedDate;
        destination.LastModifiedById = source.LastModifiedById;
        destination.LastModifiedDate = source.LastModifiedDate;
        destination.IsDeleted = source.IsDeleted;
        destination.HistoryId = source.HistoryId;

        destination.LegalName = source.LegalName;
        destination.DBAName = source.DBAName;
        destination.Step = source.Step;
        destination.PhysicalAddress = source.PhysicalAddress;
        destination.PhysicalCity = source.PhysicalCity;
        destination.PhysicalStateId = source.PhysicalStateId;
        destination.PhysicalZipCode = source.PhysicalZipCode;
        destination.PhysicalCountryId = source.PhysicalCountryId;

        destination.MailingCompanyLegalName = source.MailingCompanyLegalName;
        destination.MailingAddress = source.MailingAddress;
        destination.MailingCity = source.MailingCity;
        destination.MailingStateId = source.MailingStateId;
        destination.MailingZipCode = source.MailingZipCode;
        destination.MailingCountryId = source.MailingCountryId;
        destination.MailingEmail = source.MailingEmail;
        destination.DunsNumber = source.DunsNumber;

        destination.UseFactoringCompany = source.UseFactoringCompany;
        destination.FactoringCompanyId = source.FactoringCompanyId;

        destination.SalesSameAs = source.SalesSameAs;
        destination.DispatchSameAs = source.DispatchSameAs;
        destination.AfterHourSameAs = source.AfterHourSameAs;

        destination.ConfidantContactId = source.ConfidantContactId;
        destination.CorporateContactId = source.CorporateContactId;
        destination.SalesContactId = source.SalesContactId;
        destination.DispatchContactId = source.DispatchContactId;
        destination.AfterHourContactId = source.AfterHourContactId;

        destination.ConfidantContact = mapper.Map<SetupPacketContactDto>(source.ConfidantContact);
        destination.CorporateContact = mapper.Map<SetupPacketContactDto>(source.CorporateContact);
        destination.SalesContact = mapper.Map<SetupPacketContactDto>(source.SalesContact);
        destination.DispatchContact = mapper.Map<SetupPacketContactDto>(source.DispatchContact);
        destination.AfterHourContact = mapper.Map<SetupPacketContactDto>(source.AfterHourContact);

        #region [ Step 3 ]

        destination.CVORNumber = source.CVORNumber;
        destination.NIRNumber = source.NIRNumber;
        if (source.NSCNumber != null) {
            destination.NSCNumber1 = source.NSCNumber.Split(' ').First();
            destination.NSCNumber2 = source.NSCNumber.Split(' ').Last();
        }

        destination.CountryServiceType = source.CountryServiceType;

        destination.AuthorizedToHaulElectronics = source.AuthorizedToHaulElectronics;
        destination.HasReferBreakdown = source.HasReferBreakdown;
        destination.HasNonOwnedAuto = source.HasNonOwnedAuto;
        destination.HasActiveShipmentAtTimeOfReg = source.HasActiveShipmentAtTimeOfReg;

        destination.AllowAcquirePolicyExclusions = source.AllowAcquirePolicyExclusions;

        #endregion [ Step 3 ]

        #region [ Step 4 ]

        destination.AgreementAgree = source.AgreementAgree;
        destination.AgreementDate = source.AgreementDate;

        destination.Agreement_02Signature = source.Agreement_02Signature;
        destination.Agreement_02SignatureDate = source.Agreement_02SignatureDate;
        destination.Agreement_02Date = source.Agreement_02Date;
        destination.Agreement2_02Signature = source.Agreement2_02Signature;

        #endregion [ Step 4 ]

        #region [ Step 5 ]

        destination.GeneralLiabilityInsuranceAgencyId = source.GeneralLiabilityInsuranceAgencyId;
        destination.AutoInsuranceAgencyId = source.AutoInsuranceAgencyId;
        destination.CargoInsuranceAgencyId = source.CargoInsuranceAgencyId;
        destination.WorkersCompensationInsuranceAgencyId = source.WorkersCompensationInsuranceAgencyId;
        destination.WorkersCompensationSignature = source.WorkersCompensationSignature;
        destination.WorkersCompensationSignatureDate = source.WorkersCompensationSignatureDate;

        destination.GeneralLiabilityInsuranceAgency = mapper.Map<SetupPacketInsuranceAgencyDto>(source.GeneralLiabilityInsuranceAgency);
        destination.AutoInsuranceAgency = mapper.Map<SetupPacketInsuranceAgencyDto>(source.AutoInsuranceAgency);
        destination.CargoInsuranceAgency = mapper.Map<SetupPacketInsuranceAgencyDto>(source.CargoInsuranceAgency);
        destination.WorkersCompensationInsuranceAgency = mapper.Map<SetupPacketInsuranceAgencyDto>(source.WorkersCompensationInsuranceAgency);

        #endregion [ Step 5 ]

        #region [ Step 6 ]

        destination.AutoSameAs = source.AutoSameAs;
        destination.CargoSameAs = source.CargoSameAs;
        destination.WorkersCompensationSameAs = source.WorkersCompensationSameAs;

        #endregion [ Step 6 ]

        #region [ Step 7 ]

        destination.W9Name = source.W9Name;
        destination.W9BusinessName = source.W9BusinessName;
        destination.W9CompanyType = source.W9CompanyType;
        destination.W9TaxClassification = source.W9TaxClassification;
        destination.W9OtherTaxClassification = source.W9OtherTaxClassification;
        destination.W9ExemptPayeeCode = source.W9ExemptPayeeCode;
        destination.W9FATCACode = (char?)source.W9FATCACode;
        destination.W9Address = source.W9Address;
        destination.W9City = source.W9City;
        destination.W9StateId = source.W9StateId;
        destination.W9ZipCode = source.W9ZipCode;
        destination.W9NamePerson = source.W9NamePerson;
        destination.W9AccountNumber = source.W9AccountNumber;
        destination.W9SocialSecurityNumber = source.W9SocialSecurityNumber;
        destination.W9EmployerIdentificationNumber = source.W9EmployerIdentificationNumber;
        destination.W9Agree = source.W9Agree;
        destination.W9AgreeDate = source.W9AgreeDate;
        destination.W9Signature = source.W9Signature;

        #endregion [ Step 7 ]

        #region [ Step8 ]

        destination.Step8Modes = SetupPacketBinaryHelper.Convert<bool>(source.Step8Modes ?? new byte[0], 11).ToList();
        destination.Step8Commodities = SetupPacketBinaryHelper.Convert<bool>(source.Step8Commodities ?? new byte[0], 20).ToList();

        destination.TractorsCount = source.TractorsCount;
        destination.SCACCode = source.SCACCode;

        destination.DriversCount = source.DriversCount;
        destination.TeamsCount = source.TeamsCount;
        destination.OperatorsCount = source.OperatorsCount;

        destination.OwnerName = source.OwnerName;
        destination.OwnerPhone = source.OwnerPhone;
        destination.OwnerEmail = source.OwnerEmail;

        destination.HasMexico = source.HasMexico;
        destination.HasCanadian = source.HasCanadian;
        destination.HasEmailCarrier = source.HasEmailCarrier;

        destination.CertificationSmartWay = source.CertificationSmartWay;
        destination.CertificationC_TPAT = source.CertificationC_TPAT;
        destination.CertificationFAST = source.CertificationFAST;
        destination.CertificationTWIC = source.CertificationTWIC;
        destination.CertificationCARB = source.CertificationCARB;
        destination.CertificationHazMatCertification = source.CertificationHazMatCertification;
        destination.CertificationSafetyPermit = source.CertificationSafetyPermit;

        destination.SafetyAgree = source.SafetyAgree;

        destination.LoadUrgency = source.LoadUrgency;

        #endregion [ Step8 ]

        destination.NumberOfUnits = SetupPacketBinaryHelper.Convert<short>(source.NumberOfUnits ?? new byte[0], 39).ToList();

        if (source.References != null)
            destination.References = mapper.Map<List<SetupPacketReferenceDto>>(source.References.OrderBy(r => r.CreatedDate));

        destination.W8States = SetupPacketBinaryHelper.Convert<short>(source.W8States ?? new byte[0]).ToList();

        #region [ Step12 ]

        destination.Diversity1M_WBE = source.Diversity1M_WBE;
        destination.Diversity1 = source.Diversity1;

        destination.Diversity2 = source.Diversity2;
        destination.Diversity2Other = source.Diversity2Other;

        destination.Diversity3Certified = source.Diversity3Certified;
        destination.Diversity3 = source.Diversity3;
        destination.Diversity3Other = source.Diversity3Other;

        #endregion [ Step12 ]

        destination.FileMC = source.FileMC;
        destination.FileCOI = source.FileCOI;

        destination.EmailSent = source.EmailSent;

        return destination;
    }
}
