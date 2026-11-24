using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class SetupPacketViewModel : BaseObjectViewModel {

        #region [ _fields ]

        public byte Version { 
            get => version;
            set { 
                version = value;
                OnPropertyChanged();
            }
        }

        private string _LegalName;
        private string _DBAName;
        private byte _Step;

        #region [ Physical Address ]

        private string _PhysicalAddress;
        private string _PhysicalCity;
        private byte? _PhysicalStateId;
        private string _PhysicalZipCode;
        private byte? _PhysicalCountryId;

        #endregion [ Physical Address ]

        #region [ Mailing Address ]

        private string _MailingCompanyLegalName;
        private string _MailingAddress;
        private string _MailingCity;
        private byte? _MailingStateId;
        private string _MailingZipCode;
        private byte? _MailingCountryId;
        private string _MailingEmail;
        private string _DunsNumber;

        #endregion [ Mailing Address ]

        #region [ Factoring Company ]

        private bool _UseFactoringCompany;
        private short? _FactoringCompanyId;

        #endregion [ Factoring Company ]

        #region [ Contacts ]

        private Guid? _ConfidantContactId;
        private Guid? _CorporateContactId;
        private Guid? _SalesContactId;
        private Guid? _DispatchContactId;
        private Guid? _AfterHourContactId;

        //private SetupPacketContactDto ConfidantContact;
        //private SetupPacketContactDto CorporateContact;
        //private SetupPacketContactDto SalesContact;
        //private SetupPacketContactDto DispatchContact;
        //private SetupPacketContactDto AfterHourContact;

        #endregion [ Contacts ]

        #region [ Step3 ]

        private string _CVORNumber;
        private string _NIRNumber;
        private string _NSCNumber1;
        private string _NSCNumber2;

        private byte? _CountryServiceType;

        private bool? _AuthorizedToHaulElectronics;
        private bool? _HasReferBreakdown;
        private bool? _HasNonOwnedAuto;
        private bool? _HasActiveShipmentAtTimeOfReg;

        private bool _AllowAcquirePolicyExclusions;

        #endregion [ Step3 ]

        #region [ Step4 ]

        private bool _AgreementAgree;
        private DateTime? _AgreementDate;

        #endregion [ Step4 ]

        #region [ Step6 ]

        private Guid? _GeneralLiabilityInsuranceAgencyId;
        private Guid? _AutoInsuranceAgencyId;
        private Guid? _CargoInsuranceAgencyId;
        private Guid? _WorkersCompensationInsuranceAgencyId;

        //private SetupPacketInsuranceAgencyDto GeneralLiabilityInsuranceAgency;
        //private SetupPacketInsuranceAgencyDto AutoInsuranceAgency;
        //private SetupPacketInsuranceAgencyDto CargoInsuranceAgency;
        //private SetupPacketInsuranceAgencyDto WorkersCompensationInsuranceAgency;
        private string _WorkersCompensationSignature;
        private DateTime? _WorkersCompensationSignatureDate;

        #endregion [ Step6 ]

        #region [ Step7 ]

        private string _W9Name;

        private string _W9BusinessName;

        private byte? _W9CompanyType;
        private byte? _W9TaxClassification;
        private string _W9OtherTaxClassification;
        private bool _W9ForeignPartnersHas;

        private byte? _W9ExemptPayeeCode;
        private char? _W9FATCACode;

        private string _W9Address;
        private string _W9City;
        private byte? _W9StateId;
        private string _W9ZipCode;
        private string _W9NamePerson;
        private string _W9AccountNumber;
        private string _W9SocialSecurityNumber;
        private string _W9EmployerIdentificationNumber;

        private bool _W9Agree;
        private DateTime? _W9AgreeDate;
        private string _W9Signature;

        #endregion [ Step7 ]

        #region [ Step8 ]

        private List<bool> _Step8Modes;
        private List<bool> _Step8Commodities;

        private short _TractorsCount;
        private string _SCACCode;

        private short? _DriversCount;
        private short? _TeamsCount;
        private short? _OperatorsCount;

        private string _OwnerName;
        private string _OwnerPhone;
        private string _OwnerEmail;

        private bool _HasMexico;
        private bool _HasCanadian;
        private bool _HasEmailCarrier;

        private bool? _CertificationSmartWay;
        private bool? _CertificationC_TPAT;
        private bool? _CertificationFAST;
        private bool? _CertificationTWIC;
        private bool? _CertificationCARB;
        private bool? _CertificationHazMatCertification;
        private bool? _CertificationSafetyPermit;

        private bool _SafetyAgree;

        private byte _LoadUrgency;

        #endregion [ Step8 ]

        #region [ Step9 ]

        private List<short> _NumberOfUnits;

        #endregion [ Step9 ]

        #region [ Step10 ]

        //private List<SetupPacketReferenceDto> References;

        #endregion [ Step10 ]

        #region [ Step11 ]

        private List<short> _W8States;

        #endregion [ Step11 ]

        #region [ Step12 ]

        private bool? _Diversity1M_WBE;
        private byte? _Diversity1;

        private byte? _Diversity2;
        private string _Diversity2Other;

        private bool? _Diversity3Certified;
        private byte? _Diversity3;
        private string _Diversity3Other;

        #endregion [ Step12 ]

        #region [ Step13 ]

        private string _FileMC;
        private string _FileCOI;

        #endregion [ Step13 ]

        #region [ Last ]

        private DateTime? _EmailSent;
        private byte version;

        #endregion [ Last ]

        #endregion [ _fields ] 

        #region [ Properties ]

        public string LegalName {
            get { return _LegalName; }
            set {
                if (_LegalName != value) {
                    _LegalName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DBAName {
            get { return _DBAName; }
            set {
                if (_DBAName != value) {
                    _DBAName = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte Step {
            get { return _Step; }
            set {
                if (_Step != value) {
                    _Step = value;
                    OnPropertyChanged();
                }
            }
        }

        #region [ Physical Address ]

        public string PhysicalAddress {
            get { return _PhysicalAddress; }
            set {
                if (_PhysicalAddress != value) {
                    _PhysicalAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PhysicalCity {
            get { return _PhysicalCity; }
            set {
                if (_PhysicalCity != value) {
                    _PhysicalCity = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? PhysicalStateId {
            get { return _PhysicalStateId; }
            set {
                if (_PhysicalStateId != value) {
                    _PhysicalStateId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PhysicalZipCode {
            get { return _PhysicalZipCode; }
            set {
                if (_PhysicalZipCode != value) {
                    _PhysicalZipCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? PhysicalCountryId {
            get { return _PhysicalCountryId; }
            set {
                if (_PhysicalCountryId != value) {
                    _PhysicalCountryId = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Physical Address ]

        #region [ Mailing Address ]

        public string MailingCompanyLegalName {
            get { return _MailingCompanyLegalName; }
            set {
                if (_MailingCompanyLegalName != value) {
                    _MailingCompanyLegalName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MailingAddress {
            get { return _MailingAddress; }
            set {
                if (_MailingAddress != value) {
                    _MailingAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MailingCity {
            get { return _MailingCity; }
            set {
                if (_MailingCity != value) {
                    _MailingCity = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? MailingStateId {
            get { return _MailingStateId; }
            set {
                if (_MailingStateId != value) {
                    _MailingStateId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MailingZipCode {
            get { return _MailingZipCode; }
            set {
                if (_MailingZipCode != value) {
                    _MailingZipCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? MailingCountryId {
            get { return _MailingCountryId; }
            set {
                if (_MailingCountryId != value) {
                    _MailingCountryId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MailingEmail {
            get { return _MailingEmail; }
            set {
                if (_MailingEmail != value) {
                    _MailingEmail = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DunsNumber {
            get { return _DunsNumber; }
            set {
                if (_DunsNumber != value) {
                    _DunsNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Mailing Address ]

        #region [ Factoring Company ]

        public bool UseFactoringCompany {
            get { return _UseFactoringCompany; }
            set {
                if (_UseFactoringCompany != value) {
                    _UseFactoringCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public short? FactoringCompanyId {
            get { return _FactoringCompanyId; }
            set {
                if (_FactoringCompanyId != value) {
                    _FactoringCompanyId = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Factoring Company ]

        #region [ Contacts ]

        public Guid? ConfidantContactId {
            get { return _ConfidantContactId; }
            set {
                if (_ConfidantContactId != value) {
                    _ConfidantContactId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? CorporateContactId {
            get { return _CorporateContactId; }
            set {
                if (_CorporateContactId != value) {
                    _CorporateContactId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? SalesContactId {
            get { return _SalesContactId; }
            set {
                if (_SalesContactId != value) {
                    _SalesContactId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? DispatchContactId {
            get { return _DispatchContactId; }
            set {
                if (_DispatchContactId != value) {
                    _DispatchContactId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? AfterHourContactId {
            get { return _AfterHourContactId; }
            set {
                if (_AfterHourContactId != value) {
                    _AfterHourContactId = value;
                    OnPropertyChanged();
                }
            }
        }

        public SetupPacketContactDto ConfidantContact { get; set; }
        public SetupPacketContactDto CorporateContact { get; set; }
        public SetupPacketContactDto SalesContact { get; set; }
        public SetupPacketContactDto DispatchContact { get; set; }
        public SetupPacketContactDto AfterHourContact { get; set; }

        #endregion [ Contacts ]

        #region [ Step3 ]

        public string CVORNumber {
            get { return _CVORNumber; }
            set {
                if (_CVORNumber != value) {
                    _CVORNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string NIRNumber {
            get { return _NIRNumber; }
            set {
                if (_NIRNumber != value) {
                    _NIRNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string NSCNumber1 {
            get { return _NSCNumber1; }
            set {
                if (_NSCNumber1 != value) {
                    _NSCNumber1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string NSCNumber2 {
            get { return _NSCNumber2; }
            set {
                if (_NSCNumber2 != value) {
                    _NSCNumber2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? CountryServiceType {
            get { return _CountryServiceType; }
            set {
                if (_CountryServiceType != value) {
                    _CountryServiceType = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? AuthorizedToHaulElectronics {
            get { return _AuthorizedToHaulElectronics; }
            set {
                if (_AuthorizedToHaulElectronics != value) {
                    _AuthorizedToHaulElectronics = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? HasReferBreakdown {
            get { return _HasReferBreakdown; }
            set {
                if (_HasReferBreakdown != value) {
                    _HasReferBreakdown = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? HasNonOwnedAuto {
            get { return _HasNonOwnedAuto; }
            set {
                if (_HasNonOwnedAuto != value) {
                    _HasNonOwnedAuto = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? HasActiveShipmentAtTimeOfReg {
            get { return _HasActiveShipmentAtTimeOfReg; }
            set {
                if (_HasActiveShipmentAtTimeOfReg != value) {
                    _HasActiveShipmentAtTimeOfReg = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AllowAcquirePolicyExclusions {
            get { return _AllowAcquirePolicyExclusions; }
            set {
                if (_AllowAcquirePolicyExclusions != value) {
                    _AllowAcquirePolicyExclusions = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step3 ]

        #region [ Step4 ]

        public bool AgreementAgree {
            get { return _AgreementAgree; }
            set {
                if (_AgreementAgree != value) {
                    _AgreementAgree = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? AgreementDate {
            get { return _AgreementDate; }
            set {
                if (_AgreementDate != value) {
                    _AgreementDate = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step4 ]

        #region [ Step6 ]

        public Guid? GeneralLiabilityInsuranceAgencyId {
            get { return _GeneralLiabilityInsuranceAgencyId; }
            set {
                if (_GeneralLiabilityInsuranceAgencyId != value) {
                    _GeneralLiabilityInsuranceAgencyId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? AutoInsuranceAgencyId {
            get { return _AutoInsuranceAgencyId; }
            set {
                if (_AutoInsuranceAgencyId != value) {
                    _AutoInsuranceAgencyId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? CargoInsuranceAgencyId {
            get { return _CargoInsuranceAgencyId; }
            set {
                if (_CargoInsuranceAgencyId != value) {
                    _CargoInsuranceAgencyId = value;
                    OnPropertyChanged();
                }
            }
        }
        public Guid? WorkersCompensationInsuranceAgencyId {
            get { return _WorkersCompensationInsuranceAgencyId; }
            set {
                if (_WorkersCompensationInsuranceAgencyId != value) {
                    _WorkersCompensationInsuranceAgencyId = value;
                    OnPropertyChanged();
                }
            }
        }

        public SetupPacketInsuranceAgencyViewModel GeneralLiabilityInsuranceAgency { get; set; }
        public SetupPacketInsuranceAgencyViewModel AutoInsuranceAgency { get; set; }
        public SetupPacketInsuranceAgencyViewModel CargoInsuranceAgency { get; set; }
        public SetupPacketInsuranceAgencyViewModel WorkersCompensationInsuranceAgency { get; set; }

        public string WorkersCompensationSignature {
            get { return _WorkersCompensationSignature; }
            set {
                if (_WorkersCompensationSignature != value) {
                    _WorkersCompensationSignature = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? WorkersCompensationSignatureDate {
            get { return _WorkersCompensationSignatureDate; }
            set {
                if (_WorkersCompensationSignatureDate != value) {
                    _WorkersCompensationSignatureDate = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step6 ]

        #region [ Step7 ]

        public string W9Name {
            get { return _W9Name; }
            set {
                if (_W9Name != value) {
                    _W9Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string W9BusinessName {
            get { return _W9BusinessName; }
            set {
                if (_W9BusinessName != value) {
                    _W9BusinessName = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? W9CompanyType {
            get { return _W9CompanyType; }
            set {
                if (_W9CompanyType != value) {
                    _W9CompanyType = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte? W9TaxClassification {
            get { return _W9TaxClassification; }
            set {
                if (_W9TaxClassification != value) {
                    _W9TaxClassification = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9OtherTaxClassification {
            get { return _W9OtherTaxClassification; }
            set {
                if (_W9OtherTaxClassification != value) {
                    _W9OtherTaxClassification = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool W9ForeignPartnersHas {
            get { return _W9ForeignPartnersHas; }
            set {
                if (_W9ForeignPartnersHas != value) {
                    _W9ForeignPartnersHas = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? W9ExemptPayeeCode {
            get { return _W9ExemptPayeeCode; }
            set {
                if (_W9ExemptPayeeCode != value) {
                    _W9ExemptPayeeCode = value;
                    OnPropertyChanged();
                }
            }
        }
        public char? W9FATCACode {
            get { return _W9FATCACode; }
            set {
                if (_W9FATCACode != value) {
                    _W9FATCACode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string W9Address {
            get { return _W9Address; }
            set {
                if (_W9Address != value) {
                    _W9Address = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9City {
            get { return _W9City; }
            set {
                if (_W9City != value) {
                    _W9City = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte? W9StateId {
            get { return _W9StateId; }
            set {
                if (_W9StateId != value) {
                    _W9StateId = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9ZipCode {
            get { return _W9ZipCode; }
            set {
                if (_W9ZipCode != value) {
                    _W9ZipCode = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9NamePerson {
            get { return _W9NamePerson; }
            set {
                if (_W9NamePerson != value) {
                    _W9NamePerson = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9AccountNumber {
            get { return _W9AccountNumber; }
            set {
                if (_W9AccountNumber != value) {
                    _W9AccountNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9SocialSecurityNumber {
            get { return _W9SocialSecurityNumber; }
            set {
                if (_W9SocialSecurityNumber != value) {
                    _W9SocialSecurityNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9EmployerIdentificationNumber {
            get { return _W9EmployerIdentificationNumber; }
            set {
                if (_W9EmployerIdentificationNumber != value) {
                    _W9EmployerIdentificationNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool W9Agree {
            get { return _W9Agree; }
            set {
                if (_W9Agree != value) {
                    _W9Agree = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? W9AgreeDate {
            get { return _W9AgreeDate; }
            set {
                if (_W9AgreeDate != value) {
                    _W9AgreeDate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9Signature {
            get { return _W9Signature; }
            set {
                if (_W9Signature != value) {
                    _W9Signature = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step7 ]

        #region [ Step8 ]

        public List<bool> Step8Modes {
            get { return _Step8Modes; }
            set {
                if (_Step8Modes != value) {
                    _Step8Modes = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<bool> Step8Commodities {
            get { return _Step8Commodities; }
            set {
                if (_Step8Commodities != value) {
                    _Step8Commodities = value;
                    OnPropertyChanged();
                }
            }
        }

        public short TractorsCount {
            get { return _TractorsCount; }
            set {
                if (_TractorsCount != value) {
                    _TractorsCount = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SCACCode {
            get { return _SCACCode; }
            set {
                if (_SCACCode != value) {
                    _SCACCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public short? DriversCount {
            get { return _DriversCount; }
            set {
                if (_DriversCount != value) {
                    _DriversCount = value;
                    OnPropertyChanged();
                }
            }
        }
        public short? TeamsCount {
            get { return _TeamsCount; }
            set {
                if (_TeamsCount != value) {
                    _TeamsCount = value;
                    OnPropertyChanged();
                }
            }
        }
        public short? OperatorsCount {
            get { return _OperatorsCount; }
            set {
                if (_OperatorsCount != value) {
                    _OperatorsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string OwnerName {
            get { return _OwnerName; }
            set {
                if (_OwnerName != value) {
                    _OwnerName = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OwnerPhone {
            get { return _OwnerPhone; }
            set {
                if (_OwnerPhone != value) {
                    _OwnerPhone = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OwnerEmail {
            get { return _OwnerEmail; }
            set {
                if (_OwnerEmail != value) {
                    _OwnerEmail = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasMexico {
            get { return _HasMexico; }
            set {
                if (_HasMexico != value) {
                    _HasMexico = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasCanadian {
            get { return _HasCanadian; }
            set {
                if (_HasCanadian != value) {
                    _HasCanadian = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool HasEmailCarrier {
            get { return _HasEmailCarrier; }
            set {
                if (_HasEmailCarrier != value) {
                    _HasEmailCarrier = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? CertificationSmartWay {
            get { return _CertificationSmartWay; }
            set {
                if (_CertificationSmartWay != value) {
                    _CertificationSmartWay = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? CertificationC_TPAT {
            get { return _CertificationC_TPAT; }
            set {
                if (_CertificationC_TPAT != value) {
                    _CertificationC_TPAT = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? CertificationFAST {
            get { return _CertificationFAST; }
            set {
                if (_CertificationFAST != value) {
                    _CertificationFAST = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? CertificationTWIC {
            get { return _CertificationTWIC; }
            set {
                if (_CertificationTWIC != value) {
                    _CertificationTWIC = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? CertificationCARB {
            get { return _CertificationCARB; }
            set {
                if (_CertificationCARB != value) {
                    _CertificationCARB = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? CertificationHazMatCertification {
            get { return _CertificationHazMatCertification; }
            set {
                if (_CertificationHazMatCertification != value) {
                    _CertificationHazMatCertification = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool? CertificationSafetyPermit {
            get { return _CertificationSafetyPermit; }
            set {
                if (_CertificationSafetyPermit != value) {
                    _CertificationSafetyPermit = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool SafetyAgree {
            get { return _SafetyAgree; }
            set {
                if (_SafetyAgree != value) {
                    _SafetyAgree = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte LoadUrgency {
            get { return _LoadUrgency; }
            set {
                if (_LoadUrgency != value) {
                    _LoadUrgency = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step8 ]

        #region [ Step9 ]

        public List<short> NumberOfUnits {
            get { return _NumberOfUnits; }
            set {
                if (_NumberOfUnits != value) {
                    _NumberOfUnits = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step9 ]

        #region [ Step10 ]

        //public List<SetupPacketReferenceDto> References {

        #endregion [ Step10 ]

        #region [ Step11 ]

        public List<short> W8States {
            get { return _W8States; }
            set {
                if (_W8States != value) {
                    _W8States = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step11 ]

        #region [ Step12 ]

        public bool? Diversity1M_WBE {
            get { return _Diversity1M_WBE; }
            set {
                if (_Diversity1M_WBE != value) {
                    _Diversity1M_WBE = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte? Diversity1 {
            get { return _Diversity1; }
            set {
                if (_Diversity1 != value) {
                    _Diversity1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? Diversity2 {
            get { return _Diversity2; }
            set {
                if (_Diversity2 != value) {
                    _Diversity2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Diversity2Other {
            get { return _Diversity2Other; }
            set {
                if (_Diversity2Other != value) {
                    _Diversity2Other = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? Diversity3Certified {
            get { return _Diversity3Certified; }
            set {
                if (_Diversity3Certified != value) {
                    _Diversity3Certified = value;
                    OnPropertyChanged();
                }
            }
        }
        public byte? Diversity3 {
            get { return _Diversity3; }
            set {
                if (_Diversity3 != value) {
                    _Diversity3 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Diversity3Other {
            get { return _Diversity3Other; }
            set {
                if (_Diversity3Other != value) {
                    _Diversity3Other = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step12 ]

        #region [ Step13 ]

        public string FileMC {
            get { return _FileMC; }
            set {
                if (_FileMC != value) {
                    _FileMC = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileCOI {
            get { return _FileCOI; }
            set {
                if (_FileCOI != value) {
                    _FileCOI = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Step13 ]

        #region [ Last ]

        public DateTime? EmailSent {
            get { return _EmailSent; }
            set {
                if (_EmailSent != value) {
                    _EmailSent = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Last ]

        #endregion [ Properties ]

        public override bool Filter(string search) {
            throw new NotImplementedException();
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            throw new NotImplementedException();
        }
    }
}
