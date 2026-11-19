using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;
using System.Collections.Generic;
using System.Linq;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 SetupPacket/DtoToVm, SetupPacket/VmToDto (тела ConvertCore как есть) </summary>
    public static class SetupPacketConverter {

        public static SetupPacketViewModel ToVm(SetupPacketDto source) {
            if (source == null) return null;
            var destination = new SetupPacketViewModel();
            BaseConverter.FillEventVm(source, destination);

            destination.Version = source.Version;
            destination.EventId = source.EventId;
            destination.LegalName = source.LegalName;
            destination.DBAName = source.DBAName;
            destination.Step = source.Step;

            #region [ Physical Address ]

            destination.PhysicalAddress = source.PhysicalAddress;
            destination.PhysicalCity = source.PhysicalCity;
            destination.PhysicalStateId = source.PhysicalStateId;
            destination.PhysicalZipCode = source.PhysicalZipCode;
            destination.PhysicalCountryId = source.PhysicalCountryId;

            #endregion [ Physical Address ]

            #region [ Mailing Address ]

            destination.MailingCompanyLegalName = source.MailingCompanyLegalName;
            destination.MailingAddress = source.MailingAddress;
            destination.MailingCity = source.MailingCity;
            destination.MailingStateId = source.MailingStateId;
            destination.MailingZipCode = source.MailingZipCode;
            destination.MailingCountryId = source.MailingCountryId;
            destination.MailingEmail = source.MailingEmail;
            destination.DunsNumber = source.DunsNumber;

            #endregion [ Mailing Address ]

            #region [ Factoring Company ]

            destination.UseFactoringCompany = source.UseFactoringCompany;
            destination.FactoringCompanyId = source.FactoringCompanyId;

            #endregion [ Factoring Company ]

            #region [ Contacts ]

            destination.ConfidantContactId = source.ConfidantContactId;
            destination.CorporateContactId = source.CorporateContactId;
            destination.SalesContactId = source.SalesContactId;
            destination.DispatchContactId = source.DispatchContactId;
            destination.AfterHourContactId = source.AfterHourContactId;

            // Лень конвертить контакты
            destination.ConfidantContact = source.ConfidantContact;
            destination.CorporateContact = source.CorporateContact;
            destination.SalesContact = source.SalesContact;
            destination.DispatchContact = source.DispatchContact;
            destination.AfterHourContact = source.AfterHourContact;

            #endregion [ Contacts ]

            #region [ Step3 ]

            destination.CVORNumber = source.CVORNumber;
            destination.NIRNumber = source.NIRNumber;
            destination.NSCNumber1 = source.NSCNumber1;
            destination.NSCNumber2 = source.NSCNumber2;

            destination.CountryServiceType = source.CountryServiceType;

            destination.AuthorizedToHaulElectronics = source.AuthorizedToHaulElectronics;
            destination.HasReferBreakdown = source.HasReferBreakdown;
            destination.HasNonOwnedAuto = source.HasNonOwnedAuto;
            destination.HasActiveShipmentAtTimeOfReg = source.HasActiveShipmentAtTimeOfReg;

            destination.AllowAcquirePolicyExclusions = source.AllowAcquirePolicyExclusions;

            #endregion [ Step3 ]

            #region [ Step4 ]

            destination.AgreementAgree = source.AgreementAgree;
            destination.AgreementDate = source.AgreementDate;

            #endregion [ Step4 ]

            #region [ Step6 ]

            destination.GeneralLiabilityInsuranceAgencyId = source.GeneralLiabilityInsuranceAgencyId;
            destination.AutoInsuranceAgencyId = source.AutoInsuranceAgencyId;
            destination.CargoInsuranceAgencyId = source.CargoInsuranceAgencyId;
            destination.WorkersCompensationInsuranceAgencyId = source.WorkersCompensationInsuranceAgencyId;
            destination.GeneralLiabilityInsuranceAgency = SetupPacketInsuranceAgencyConverter.ToVm(source.GeneralLiabilityInsuranceAgency);
            destination.AutoInsuranceAgency = SetupPacketInsuranceAgencyConverter.ToVm(source.AutoInsuranceAgency);
            destination.CargoInsuranceAgency = SetupPacketInsuranceAgencyConverter.ToVm(source.CargoInsuranceAgency);
            destination.WorkersCompensationInsuranceAgency = SetupPacketInsuranceAgencyConverter.ToVm(source.WorkersCompensationInsuranceAgency);

            destination.WorkersCompensationSignature = source.WorkersCompensationSignature;
            destination.WorkersCompensationSignatureDate = source.WorkersCompensationSignatureDate;

            #endregion [ Step6 ]

            #region [ Step7 ]

            destination.W9Name = source.W9Name;

            destination.W9BusinessName = source.W9BusinessName;

            destination.W9CompanyType = source.W9CompanyType;
            destination.W9TaxClassification = source.W9TaxClassification;
            destination.W9OtherTaxClassification = source.W9OtherTaxClassification;

            destination.W9ExemptPayeeCode = source.W9ExemptPayeeCode;
            destination.W9FATCACode = source.W9FATCACode;

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

            #endregion [ Step7 ]

            #region [ Step8 ]

            destination.Step8Modes = source.Step8Modes;
            destination.Step8Commodities = source.Step8Commodities;

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

            destination.NumberOfUnits = source.NumberOfUnits;

            #endregion [ Step9 ]

            #region [ Step10 ]

            //destination.References = source.References;

            #endregion [ Step10 ]

            #region [ Step11 ]

            destination.W8States = source.W8States;

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

            #region [ Step13 ]

            destination.FileMC = source.FileMC;
            destination.FileCOI = source.FileCOI;

            #endregion [ Step13 ]

            #region [ Last ]

            destination.EmailSent = source.EmailSent;

            #endregion [ Last ]

            return destination;
        }

        public static SetupPacketDto ToDto(SetupPacketViewModel source) {
            if (source == null) return null;
            var destination = new SetupPacketDto();
            BaseConverter.FillEventDto(source, destination);

            destination.Version = source.Version;
            destination.EventId = source.EventId;
            destination.LegalName = source.LegalName;
            destination.DBAName = source.DBAName;
            destination.Step = source.Step;

            #region [ Physical Address ]

            destination.PhysicalAddress = source.PhysicalAddress;
            destination.PhysicalCity = source.PhysicalCity;
            destination.PhysicalStateId = source.PhysicalStateId;
            destination.PhysicalZipCode = source.PhysicalZipCode;
            destination.PhysicalCountryId = source.PhysicalCountryId;

            #endregion [ Physical Address ]

            #region [ Mailing Address ]

            destination.MailingCompanyLegalName = source.MailingCompanyLegalName;
            destination.MailingAddress = source.MailingAddress;
            destination.MailingCity = source.MailingCity;
            destination.MailingStateId = source.MailingStateId;
            destination.MailingZipCode = source.MailingZipCode;
            destination.MailingCountryId = source.MailingCountryId;
            destination.MailingEmail = source.MailingEmail;
            destination.DunsNumber = source.DunsNumber;

            #endregion [ Mailing Address ]

            #region [ Factoring Company ]

            destination.UseFactoringCompany = source.UseFactoringCompany;
            destination.FactoringCompanyId = source.FactoringCompanyId;

            #endregion [ Factoring Company ]

            #region [ Contacts ]

            destination.ConfidantContactId = source.ConfidantContactId;
            destination.CorporateContactId = source.CorporateContactId;
            destination.SalesContactId = source.SalesContactId;
            destination.DispatchContactId = source.DispatchContactId;
            destination.AfterHourContactId = source.AfterHourContactId;

            // Лень конвертить контакты
            destination.ConfidantContact = source.ConfidantContact;
            destination.CorporateContact = source.CorporateContact;
            destination.SalesContact = source.SalesContact;
            destination.DispatchContact = source.DispatchContact;
            destination.AfterHourContact = source.AfterHourContact;

            #endregion [ Contacts ]

            #region [ Step3 ]

            destination.CVORNumber = source.CVORNumber;
            destination.NIRNumber = source.NIRNumber;
            destination.NSCNumber1 = source.NSCNumber1;
            destination.NSCNumber2 = source.NSCNumber2;

            destination.CountryServiceType = source.CountryServiceType;

            destination.AuthorizedToHaulElectronics = source.AuthorizedToHaulElectronics;
            destination.HasReferBreakdown = source.HasReferBreakdown;
            destination.HasNonOwnedAuto = source.HasNonOwnedAuto;
            destination.HasActiveShipmentAtTimeOfReg = source.HasActiveShipmentAtTimeOfReg;

            destination.AllowAcquirePolicyExclusions = source.AllowAcquirePolicyExclusions;

            #endregion [ Step3 ]

            #region [ Step4 ]

            destination.AgreementAgree = source.AgreementAgree;
            destination.AgreementDate = source.AgreementDate;

            #endregion [ Step4 ]

            #region [ Step6 ]

            destination.GeneralLiabilityInsuranceAgencyId = source.GeneralLiabilityInsuranceAgencyId;
            destination.AutoInsuranceAgencyId = source.AutoInsuranceAgencyId;
            destination.CargoInsuranceAgencyId = source.CargoInsuranceAgencyId;
            destination.WorkersCompensationInsuranceAgencyId = source.WorkersCompensationInsuranceAgencyId;

            //destination.GeneralLiabilityInsuranceAgency = source.GeneralLiabilityInsuranceAgency;
            //destination.AutoInsuranceAgency = source.AutoInsuranceAgency;
            //destination.CargoInsuranceAgency = source.CargoInsuranceAgency;
            //destination.WorkersCompensationInsuranceAgency = source.WorkersCompensationInsuranceAgency;
            destination.WorkersCompensationSignature = source.WorkersCompensationSignature;
            destination.WorkersCompensationSignatureDate = source.WorkersCompensationSignatureDate;

            #endregion [ Step6 ]

            #region [ Step7 ]

            destination.W9Name = source.W9Name;

            destination.W9BusinessName = source.W9BusinessName;

            destination.W9CompanyType = source.W9CompanyType;
            destination.W9TaxClassification = source.W9TaxClassification;
            destination.W9OtherTaxClassification = source.W9OtherTaxClassification;

            destination.W9ExemptPayeeCode = source.W9ExemptPayeeCode;
            destination.W9FATCACode = source.W9FATCACode;

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

            #endregion [ Step7 ]

            #region [ Step8 ]

            destination.Step8Modes = source.Step8Modes;
            destination.Step8Commodities = source.Step8Commodities;

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

            destination.NumberOfUnits = source.NumberOfUnits;

            #endregion [ Step9 ]

            #region [ Step10 ]

            //destination.References = source.References;

            #endregion [ Step10 ]

            #region [ Step11 ]

            destination.W8States = source.W8States;

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

            #region [ Step13 ]

            destination.FileMC = source.FileMC;
            destination.FileCOI = source.FileCOI;

            #endregion [ Step13 ]

            #region [ Last ]

            destination.EmailSent = source.EmailSent;

            #endregion [ Last ]

            return destination;
        }
    }
}
