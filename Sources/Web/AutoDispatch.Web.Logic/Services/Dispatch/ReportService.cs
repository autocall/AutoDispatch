using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     PDF-документы setup packet (V1 ReportService): поля для шаблонов, W9 (AcroForm fw9.pdf), Setup Packet и Insurance Instructions
///     (RTF-шаблоны Resources/Documents → PDF). State/Country — StateHelper вместо таблиц; шрифт подписи — Resources/Fonts/DancingScript-Bold.ttf (как в Safer, вместо Brush Script MT из V1).
///     RTF→PDF: Telerik Document Processing .NET Standard (TelerikPdfHelper) вместо WPF-версии V1 — AI/wiki/plan.md </summary>
public class ReportService : BaseService {

    private static readonly Dictionary<byte, string> CountryServiceTypeDictionary = new() {
        { 1, "Domestic US ONLY" }, { 2, "Domestic Canada ONLY" }, { 3, "Both Canada and US" }
    };
    private static readonly Dictionary<byte, string> W9CompanyTypeDictionary = new() {
        { 1, "Individual/Sole Proprietor or single-member LLC" }, { 2, "C Corporation" }, { 3, "S Corporation" }, { 4, "Partnership" }, { 5, "Trust/Estate" }, { 6, "Limited Liability Company" }, { 7, "Other" }
    };
    private static readonly Dictionary<byte, string> W9TaxClassificationDictionary = new() {
        { 1, "C" }, { 2, "S" }, { 3, "P" }
    };
    private static readonly Dictionary<byte, string> LoadUrgencyDictionary = new() {
        { 1, "Within 24 Hours" }, { 2, "Within 2-4 Hours" }, { 3, "More than 24 hours from now" },
    };

    /// <summary>
    ///     RTF галочка (ZapfDingbats check) </summary>
    private const string Check = @"က3\'3f";

    public ReportService(ServicesHost host) : base(host) { }

    public Dictionary<string, string> GetFields(SetupPacketDto setupPacketDto, DatCompanyDto carrier, ActualCompanyDto actualCompany) {
        var fields = new Dictionary<string, string>();

        #region SetupPacketDto

        fields["SetupPacket.LegalName"] = setupPacketDto.LegalName;
        fields["SetupPacket.DBAName"] = setupPacketDto.DBAName;

        #region [ Physical Address ]

        fields["SetupPacket.PhysicalAddress"] = setupPacketDto.PhysicalAddress;
        fields["SetupPacket.PhysicalCity"] = setupPacketDto.PhysicalCity;
        fields["SetupPacket.PhysicalStateId"] = StateHelper.GetState(setupPacketDto.PhysicalStateId)?.Code ?? "??";
        fields["SetupPacket.PhysicalState"] = StateHelper.GetState(setupPacketDto.PhysicalStateId)?.Name ?? "??";
        fields["SetupPacket.PhysicalZipCode"] = setupPacketDto.PhysicalZipCode;
        fields["SetupPacket.PhysicalCountryId"] = StateHelper.GetCountry(setupPacketDto.PhysicalCountryId)?.Name ?? "??";

        #endregion [ Physical Address ]

        #region [ Mailing Address ]

        fields["SetupPacket.MailingCompanyLegalName"] = setupPacketDto.MailingCompanyLegalName;
        fields["SetupPacket.MailingAddress"] = setupPacketDto.MailingAddress;
        fields["SetupPacket.MailingCity"] = setupPacketDto.MailingCity;
        fields["SetupPacket.MailingStateId"] = StateHelper.GetState(setupPacketDto.MailingStateId)?.Code ?? "??";
        fields["SetupPacket.MailingZipCode"] = setupPacketDto.MailingZipCode;
        fields["SetupPacket.MailingCountryId"] = StateHelper.GetCountry(setupPacketDto.MailingCountryId)?.Name ?? "??";
        fields["SetupPacket.MailingEmail"] = setupPacketDto.MailingEmail;
        fields["SetupPacket.DunsNumber"] = setupPacketDto.DunsNumber;

        #endregion [ Mailing Address ]

        #region [ Factoring Company ]

        fields["SetupPacket.UseFactoringCompany"] = setupPacketDto.UseFactoringCompany ? Check : "";

        #endregion [ Factoring Company ]

        #region [ Contacts ]

        var setupPacketContactsAction = new Action<string, SetupPacketContactDto>((name, contactDto) => {
            fields[$"SetupPacket.{name}.Contact"] = contactDto.Contact;
            fields[$"SetupPacket.{name}.Phone"] = contactDto.Phone?.ToPhone();
            fields[$"SetupPacket.{name}.Email"] = contactDto.Email;
            fields[$"SetupPacket.{name}.Title"] = contactDto.Title;
            fields[$"SetupPacket.{name}.Cell"] = contactDto.Cell?.ToPhone();
            fields[$"SetupPacket.{name}.Fax"] = contactDto.Fax?.ToPhone();
        });

        if (setupPacketDto.ConfidantContact != null)
            setupPacketContactsAction(nameof(setupPacketDto.ConfidantContact), setupPacketDto.ConfidantContact);
        if (setupPacketDto.CorporateContact != null)
            setupPacketContactsAction(nameof(setupPacketDto.CorporateContact), setupPacketDto.CorporateContact);
        if (setupPacketDto.SalesContact != null)
            setupPacketContactsAction(nameof(setupPacketDto.SalesContact), setupPacketDto.SalesContact);
        if (setupPacketDto.DispatchContact != null)
            setupPacketContactsAction(nameof(setupPacketDto.DispatchContact), setupPacketDto.DispatchContact);
        if (setupPacketDto.AfterHourContact != null)
            setupPacketContactsAction(nameof(setupPacketDto.AfterHourContact), setupPacketDto.AfterHourContact);

        #endregion [ Contacts ]

        #region [ Step3 ]

        fields["SetupPacket.CVORNumber"] = setupPacketDto.CVORNumber;
        fields["SetupPacket.NIRNumber"] = setupPacketDto.NIRNumber;
        fields["SetupPacket.NSCNumber1"] = setupPacketDto.NSCNumber1;
        fields["SetupPacket.NSCNumber2"] = setupPacketDto.NSCNumber2;

        fields["SetupPacket.CountryServiceType"] = setupPacketDto.CountryServiceType != null ? CountryServiceTypeDictionary[(byte)setupPacketDto.CountryServiceType] : "";
        fields["SetupPacket.AuthorizedToHaulElectronics"] = setupPacketDto.AuthorizedToHaulElectronics == true ? Check : "";
        fields["SetupPacket.HasReferBreakdown"] = setupPacketDto.HasReferBreakdown == true ? Check : "";
        fields["SetupPacket.HasNonOwnedAuto"] = setupPacketDto.HasNonOwnedAuto == true ? Check : "";
        fields["SetupPacket.HasActiveShipmentAtTimeOfReg"] = setupPacketDto.HasActiveShipmentAtTimeOfReg == true ? Check : "";
        fields["SetupPacket.AllowAcquirePolicyExclusions"] = setupPacketDto.AllowAcquirePolicyExclusions == true ? Check : "";

        #endregion [ Step3 ]

        #region [ Step4 ]

        fields["SetupPacket.AgreementAgree"] = setupPacketDto.AgreementAgree == true ? Check : "";
        fields["SetupPacket.AgreementDate"] = setupPacketDto.AgreementDate != null ? setupPacketDto.AgreementDate.Value.ToShortDateString() : "ERROR!!!";
        fields["SetupPacket.Agreement_02Date"] = setupPacketDto.Agreement_02Date != null ? setupPacketDto.Agreement_02Date.Value.ToShortDateString() : "ERROR!!!";
        fields["SetupPacket.Agreement_02SignatureDate"] = setupPacketDto.Agreement_02SignatureDate != null ? setupPacketDto.Agreement_02SignatureDate.Value.ToShortDateString() : "ERROR!!!";
        fields["SetupPacket.Agreement_02Signature"] = setupPacketDto.Agreement_02Signature;
        fields["SetupPacket.Agreement2_02Signature"] = setupPacketDto.Agreement2_02Signature;

        #endregion [ Step4 ]

        #region [ Step6 ]

        var setupPacketInsuranceAgencyAction = new Action<string, SetupPacketInsuranceAgencyDto>((name, insuranceAgencyDto) => {
            fields[$"SetupPacket.{name}.CompanyName"] = insuranceAgencyDto.CompanyName;
            fields[$"SetupPacket.{name}.Phone"] = insuranceAgencyDto.Phone?.ToPhone();
            fields[$"SetupPacket.{name}.Fax"] = insuranceAgencyDto.Fax?.ToPhone();
            fields[$"SetupPacket.{name}.Email"] = insuranceAgencyDto.Email;
            fields[$"SetupPacket.{name}.Number"] = insuranceAgencyDto.Number;
        });

        if (setupPacketDto.GeneralLiabilityInsuranceAgency != null)
            setupPacketInsuranceAgencyAction(nameof(setupPacketDto.GeneralLiabilityInsuranceAgency), setupPacketDto.GeneralLiabilityInsuranceAgency);
        if (setupPacketDto.AutoInsuranceAgency != null)
            setupPacketInsuranceAgencyAction(nameof(setupPacketDto.AutoInsuranceAgency), setupPacketDto.AutoInsuranceAgency);
        if (setupPacketDto.CargoInsuranceAgency != null)
            setupPacketInsuranceAgencyAction(nameof(setupPacketDto.CargoInsuranceAgency), setupPacketDto.CargoInsuranceAgency);
        if (setupPacketDto.WorkersCompensationInsuranceAgency != null) {
            setupPacketInsuranceAgencyAction(nameof(setupPacketDto.WorkersCompensationInsuranceAgency), setupPacketDto.WorkersCompensationInsuranceAgency);
        } else {
            fields["SetupPacket.WorkersCompensationSignatureDate"] = setupPacketDto.WorkersCompensationSignatureDate?.ToString("d");
            fields["SetupPacket.WorkersCompensationSignature"] = setupPacketDto.WorkersCompensationSignature;
        }

        #endregion [ Step6 ]

        #region [ Step7 ]

        fields["SetupPacket.W9Name"] = setupPacketDto.W9Name;
        fields["SetupPacket.W9BusinessName"] = setupPacketDto.W9BusinessName;
        fields["SetupPacket.W9CompanyType"] = setupPacketDto.W9CompanyType != null ? (setupPacketDto.W9CompanyType - 1 /* the fix #1 */).ToString() : "";
        fields["SetupPacket.W9TaxClassification"] = setupPacketDto.W9TaxClassification != null ? W9TaxClassificationDictionary[(byte)setupPacketDto.W9TaxClassification] : "";
        fields["SetupPacket.W9OtherTaxClassification"] = setupPacketDto.W9OtherTaxClassification;
        fields["SetupPacket.W9ExemptPayeeCode"] = setupPacketDto.W9ExemptPayeeCode != null ? setupPacketDto.W9ExemptPayeeCode.ToString() : "";
        fields["SetupPacket.W9FATCACode"] = setupPacketDto.W9FATCACode != null ? setupPacketDto.W9FATCACode.ToString() : "";
        fields["SetupPacket.W9Address"] = setupPacketDto.W9Address;
        fields["SetupPacket.W9City"] = setupPacketDto.W9City;
        fields["SetupPacket.W9StateId"] = StateHelper.GetState(setupPacketDto.W9StateId)?.Code;
        fields["SetupPacket.W9ZipCode"] = setupPacketDto.W9ZipCode;
        fields["SetupPacket.W9NamePerson"] = setupPacketDto.W9NamePerson;
        fields["SetupPacket.W9AccountNumber"] = setupPacketDto.W9AccountNumber;
        fields["SetupPacket.W9SocialSecurityNumber"] = setupPacketDto.W9SocialSecurityNumber;
        fields["SetupPacket.W9EmployerIdentificationNumber"] = setupPacketDto.W9EmployerIdentificationNumber;
        fields["SetupPacket.W9Agree"] = setupPacketDto.W9Agree == true ? Check : "";
        fields["SetupPacket.W9AgreeDate"] = setupPacketDto.W9AgreeDate?.ToString("d");
        fields["SetupPacket.W9Signature"] = setupPacketDto.W9Signature;

        #endregion [ Step7 ]

        #region [ Step8 ]

        fields["SetupPacket.TractorsCount"] = setupPacketDto.TractorsCount.ToString();
        fields["SetupPacket.SCACCode"] = setupPacketDto.SCACCode;

        fields["SetupPacket.DriversCount"] = setupPacketDto.DriversCount?.ToString();
        fields["SetupPacket.TeamsCount"] = setupPacketDto.TeamsCount?.ToString();
        fields["SetupPacket.OperatorsCount"] = setupPacketDto.OperatorsCount?.ToString();

        fields["SetupPacket.OwnerName"] = setupPacketDto.OwnerName;
        fields["SetupPacket.OwnerPhone"] = setupPacketDto.OwnerPhone;
        fields["SetupPacket.OwnerEmail"] = setupPacketDto.OwnerEmail;

        fields["SetupPacket.HasMexico"] = setupPacketDto.HasMexico == true ? Check : "";
        fields["SetupPacket.HasCanadian"] = setupPacketDto.HasCanadian == true ? Check : "";
        fields["SetupPacket.HasEmailCarrier"] = setupPacketDto.HasEmailCarrier == true ? Check : "";

        fields["SetupPacket.CertificationSmartWay"] = setupPacketDto.CertificationSmartWay == true ? Check : "";
        fields["SetupPacket.CertificationC_TPAT"] = setupPacketDto.CertificationC_TPAT == true ? Check : "";
        fields["SetupPacket.CertificationFAST"] = setupPacketDto.CertificationFAST == true ? Check : "";
        fields["SetupPacket.CertificationTWIC"] = setupPacketDto.CertificationTWIC == true ? Check : "";
        fields["SetupPacket.CertificationCARB"] = setupPacketDto.CertificationCARB == true ? Check : "";
        fields["SetupPacket.CertificationHazMatCertification"] = setupPacketDto.CertificationHazMatCertification == true ? Check : "";
        fields["SetupPacket.CertificationSafetyPermit"] = setupPacketDto.CertificationSafetyPermit == true ? Check : "";

        fields["SetupPacket.SafetyAgree"] = setupPacketDto.SafetyAgree == true ? Check : "";
        fields["SetupPacket.LoadUrgency"] = LoadUrgencyDictionary.ContainsKey(setupPacketDto.LoadUrgency) ? LoadUrgencyDictionary[setupPacketDto.LoadUrgency] : "";

        #endregion [ Step8 ]

        #region [ Step13 ]

        fields["SetupPacket.FileMC"] = setupPacketDto.FileMC;
        fields["SetupPacket.FileCOI"] = setupPacketDto.FileCOI;

        #endregion [ Step13 ]

        #endregion SetupPacketDto

        #region DatCompanyDto

        fields["DatCompany.Name"] = carrier.Name;
        fields["DatCompany.McNumber"] = carrier.McNumber.ToString();
        fields["DatCompany.DotNumber"] = carrier.DotNumber?.ToString();
        fields["DatCompany.Contacts"] = carrier.Contacts;
        fields["DatCompany.InsuranceExpiration"] = carrier.InsuranceExpiration?.ToString("d");
        fields["DatCompany.Comments"] = carrier.Comments;

        #endregion DatCompanyDto

        #region ActualCompanyDto

        fields["ActualCompany.LogoSmall"] = actualCompany.LogoSmall;
        fields["ActualCompany.LogoBig"] = actualCompany.LogoBig;
        fields["ActualCompany.McNumber"] = actualCompany.McNumber.ToString();
        fields["ActualCompany.LegalName"] = actualCompany.LegalName;
        fields["ActualCompany.CellPhone"] = actualCompany.CellPhone?.ToPhone();
        fields["ActualCompany.ShippersPhone"] = actualCompany.ShippersPhone?.ToPhone();
        fields["ActualCompany.WorkPhone"] = actualCompany.WorkPhone?.ToPhone();
        fields["ActualCompany.WorkFax"] = actualCompany.WorkFax?.ToPhone();
        fields["ActualCompany.PhysicalAddress1"] = actualCompany.PhysicalAddress.Trim().Split('\n').First().Trim();
        fields["ActualCompany.PhysicalAddress2"] = actualCompany.PhysicalAddress.Trim().Split('\n').Last().Trim();
        fields["ActualCompany.MailingAddress1"] = actualCompany.MailingAddress.Trim().Split('\n').First().Trim();
        fields["ActualCompany.MailingAddress2"] = actualCompany.MailingAddress.Trim().Split('\n').Last().Trim();
        fields["ActualCompany.PhysicalAddress"] = actualCompany.PhysicalAddress.Trim();
        fields["ActualCompany.MailingAddress"] = actualCompany.MailingAddress.Trim();

        #endregion ActualCompanyDto

        fields["DateTime.Now"] = DateTime.Now.ToShortDateString();

        return fields;
    }

    public byte[] GenerateSetupPacketPdf(string path, SetupPacketDto setupPacket, Dictionary<string, string> fields) {
        return RtfToPdf(FillRtfTemplate(path, setupPacket, fields));
    }

    public byte[] GenerateInsuranceInstructionsPdf(string path, SetupPacketDto setupPacket, Dictionary<string, string> fields) {
        return RtfToPdf(FillRtfTemplate(path, setupPacket, fields));
    }

    /// <summary>
    ///     Подстановка полей, логотипа и вырезание пустых контактов по MARKER* в RTF-шаблон (общая часть V1 GenerateSetupPacketPdf и
    ///     GenerateInsuranceInstructionsPdf) </summary>
    public static string FillRtfTemplate(string path, SetupPacketDto setupPacket, Dictionary<string, string> fields) {
        var sb = new StringBuilder(File.ReadAllText(path));
        // таблицы шаблонов — стиль Table Grid (\ts23: \trbrdr* \brdrs\brdrw10), ячейки границ не задают (\brdrtbl): WPF RadDocument V1
        // рисовал без рамок, Telerik Document Processing рисует рамки стиля — у стилей таблиц границы убираются, как в PDF V1
        // (линии абзацев \brdrt/\brdrb не трогаются) — AI/wiki/app-testing.md 6.26
        sb = new StringBuilder(Regex.Replace(sb.ToString(), @"(\\trbrdr[tlbrhv]\s*)\\brdrs\\brdrw\d+", "$1\\brdrnone"));

        foreach (var field in fields) {
            sb.Replace(field.Key, WebUtility.HtmlDecode(field.Value));
        }

        #region logo

        sb.Replace("[logo]", BitConverter.ToString(Convert.FromBase64String(fields["ActualCompany.LogoBig"]
            .Replace("data:image/png;base64,", "").Replace("data:image/jpeg;base64,", ""))).Replace("-", ""));

        #endregion logo

        #region removes contact information

        // a reverse order

        var marker1 = sb.IndexOf("MARKER1");
        var marker2 = sb.IndexOf("MARKER2");
        var marker3 = sb.IndexOf("MARKER3");
        var marker4 = sb.IndexOf("MARKER4");

        var marker21 = sb.IndexOf("MARKER21");
        var marker22 = sb.IndexOf("MARKER22");
        var marker23 = sb.IndexOf("MARKER23");

        if (setupPacket.WorkersCompensationInsuranceAgency != null) {
            if (marker22 != -1 && marker23 != -1) {
                sb.Remove(marker22, marker23 + 8 - marker22);
            }
        } else {
            if (marker21 != -1 && marker22 != -1) {
                sb.Remove(marker21, marker22 + 8 - marker21);
            }
        }
        marker21 = sb.IndexOf("MARKER21");
        if (marker21 != -1) {
            sb.Remove(marker21, 8);
        }
        marker22 = sb.IndexOf("MARKER22");
        if (marker22 != -1) {
            sb.Remove(marker22, 8);
        }
        marker23 = sb.IndexOf("MARKER23");
        if (marker23 != -1) {
            sb.Remove(marker23, 8);
        }
        // секции контактов: MARKER1 Sales MARKER2 Dispatch MARKER3 «No Accounting» (AfterHour) MARKER4. В V1 здесь маркеры сначала
        // удалялись, а fixContact потом вырезал по старым позициям ещё по 7 символов — в PDF «ontact Information», «h Contact
        // Information», «unting Contact…» и лишняя «h» (так и в PDF V1). Маркеры убирает только fixContact (с конца) и MARKER1 ниже
        // (AI/wiki/app-testing.md 6.25)
        marker1 = sb.IndexOf("MARKER1");
        marker2 = sb.IndexOf("MARKER2");
        marker3 = sb.IndexOf("MARKER3");
        marker4 = sb.IndexOf("MARKER4");
        var fixContact = new Action<int, int, string>((m1, m2, contactName) => {
            if (m1 != -1 && m2 != -1) {
                if (fields.ContainsKey($"SetupPacket.{contactName}.Contact")) {
                    sb.Remove(m2, 7);
                } else {
                    sb.Remove(m1, m2 - m1 + 7);
                }
            }
        });
        fixContact(marker3 + 7, marker4, "AfterHourContact");
        fixContact(marker2 + 7, marker3, "DispatchContact");
        fixContact(marker1 + 7, marker2, "SalesContact");
        if (marker1 != -1) {
            sb.Remove(marker1, 7);
        }

        #endregion removes contact information

        return sb.ToString();
    }

    /// <summary>
    ///     V1: Telerik RtfFormatProvider.Import → PdfFormatProvider.Export (deflate 9) на WPF RadDocument; здесь та же пара провайдеров
    ///     Telerik Document Processing .NET Standard (уровня сжатия в настройках экспорта нет) — TelerikPdfHelper </summary>
    private static byte[] RtfToPdf(string rtf) {
        return TelerikPdfHelper.RtfToPdf(rtf);
    }

    /// <summary>
    ///     W-9 по версии packet'а: версия 3 — Rev. March 2024 (Documents/fw9_03.pdf), до 3 — Rev. November 2017 V1 (Documents/fw9.pdf):
    ///     карриер получает PDF той формы, которую заполнял (решено 2026-11-23, AI/wiki/plan.md) </summary>
    public byte[] GenerateW9Pdf(string path, SetupPacketDto setupPacket, Dictionary<string, string> fields) {
        return setupPacket.Version >= 3 ? GenerateW9Pdf2024(path, setupPacket, fields) : GenerateW9Pdf2017(path, fields);
    }

    /// <summary>
    ///     W-9 Rev. November 2017 — код V1; null в SetField и несбалансированный BeginText роняют iTextSharp.LGPLv2.Core (AI/wiki/app-testing.md 6.21) </summary>
    private static byte[] GenerateW9Pdf2017(string path, Dictionary<string, string> fields) {
        using var ms = new MemoryStream();
        var pdfReader = new PdfReader(path);
        var pdfStamper = new PdfStamper(pdfReader, ms);

        /*
        f1_1[0]: 1.
        f1_2[0]: 2.
        Address[0].f1_7[0]: 5.
        Address[0].f1_8[0]: 6.
        f1_9[0]: Requester
        f1_10[0]: 7.
        Exemptions[0].f1_5[0]: 4.
        Exemptions[0].f1_6[0]: 4.
        SSN[0].f1_11/12/13]: social security number
        EmployerID[0].f1_14/15[0]: Employer Id

        FederalClassification[0].c1_1[0-6]: 3. value 1-7
        FederalClassification[0].f1_4[0]: 3. Other
        FederalClassification[0].f1_3[0]: 3. Tax classification
        */

        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].f1_1[0]", fields["SetupPacket.W9Name"] ?? "");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].f1_2[0]", fields["SetupPacket.W9BusinessName"] ?? "");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].Address[0].f1_7[0]", fields["SetupPacket.W9Address"] ?? "");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].Address[0].f1_8[0]", $"{fields["SetupPacket.W9City"]}, {fields["SetupPacket.W9StateId"]} {fields["SetupPacket.W9ZipCode"]}");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].f1_9[0]", fields["SetupPacket.W9NamePerson"] ?? "");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].f1_10[0]", fields["SetupPacket.W9AccountNumber"] ?? "");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].Exemptions[0].f1_5[0]", fields["SetupPacket.W9ExemptPayeeCode"] ?? "");
        pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].Exemptions[0].f1_6[0]", fields["SetupPacket.W9FATCACode"] ?? "");
        var arr1 = fields["SetupPacket.W9SocialSecurityNumber"]?.Split('-');
        if (fields["SetupPacket.W9SocialSecurityNumber"] != null && arr1.Length == 3) {
            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].SSN[0].f1_11[0]", arr1[0]);
            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].SSN[0].f1_12[0]", arr1[1]);
            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].SSN[0].f1_13[0]", arr1[2]);
        }
        var arr2 = fields["SetupPacket.W9EmployerIdentificationNumber"]?.Split('-');
        if (fields["SetupPacket.W9EmployerIdentificationNumber"] != null && arr2.Length == 2) {
            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].EmployerID[0].f1_14[0]", arr2[0]);
            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].EmployerID[0].f1_15[0]", arr2[1]);
        }

        // the fix #1
        #region  writes check
        {
            try {
                // позиция до BeginText: без типа компании GetFieldPositions даёт null, и незакрытый BeginText ронял PDF
                // iTextSharp 4.x: float[] { page, llx, lly, urx, ury }
                var position = pdfStamper.AcroFields.GetFieldPositions($"topmostSubform[0].Page1[0].FederalClassification[0].c1_1[{fields["SetupPacket.W9CompanyType"]}]");
                if (position != null) {
                    PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
                    pdfContentByte.SetColorFill(BaseColor.Black);
                    BaseFont bfTimes = BaseFont.CreateFont(BaseFont.ZAPFDINGBATS, BaseFont.CP1252, false);
                    pdfContentByte.SetFontAndSize(bfTimes, 12);
                    pdfContentByte.BeginText();
                    pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, "3", position[1], position[2], 0);
                    pdfContentByte.EndText();
                }
            } catch (Exception) {
            }

            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].FederalClassification[0].f1_4[0]", fields["SetupPacket.W9OtherTaxClassification"] ?? "");
            pdfStamper.AcroFields.SetField("topmostSubform[0].Page1[0].FederalClassification[0].f1_3[0]", fields["SetupPacket.W9TaxClassification"] ?? "");
        }
        #endregion  writes check

        #region signature
        {
            PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
            // signature
            string fontPath = Path.Combine(AppSettings.FontsPath, "DancingScript-Bold.ttf");
            BaseFont font = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            pdfContentByte.SetColorFill(BaseColor.Black);
            pdfContentByte.SetFontAndSize(font, 20);
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, fields["SetupPacket.W9Signature"] ?? "-", 133, 255, 0);
            pdfContentByte.EndText();
            // date
            font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            pdfContentByte.SetColorFill(BaseColor.Black);
            pdfContentByte.SetFontAndSize(font, 10);
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, fields["SetupPacket.W9AgreeDate"] ?? "-", 410, 255, 0);
            pdfContentByte.EndText();
        }
        #endregion signature

        pdfStamper.SetFullCompression();
        pdfStamper.FormFlattening = true;
        pdfStamper.Close();
        pdfReader.Close();

        return ms.ToArray();
    }

    private static byte[] GenerateW9Pdf2024(string path, SetupPacketDto setupPacket, Dictionary<string, string> fields) {
        using var ms = new MemoryStream();
        var pdfReader = new PdfReader(path);
        var pdfStamper = new PdfStamper(pdfReader, ms);

        /*
        Форма W-9 Rev. March 2024 (Documents/fw9_03.pdf, как в Safer FormW9.pdf; версия packet'а 3), в форме 2017 другие имена полей:
        f1_01[0]: 1. Name, f1_02[0]: 2. Business name
        Boxes3a-b_ReadOrder[0].c1_1[0-6]: 3a. тип (состояния "1".."7"), .f1_03[0]: LLC tax classification, .f1_04[0]: Other,
        Boxes3a-b_ReadOrder[0].c1_2[0]: 3b. foreign partners (состояние "1")
        f1_05[0], f1_06[0]: 4. Exempt payee code, FATCA
        Address_ReadOrder[0].f1_07[0], .f1_08[0]: 5., 6.; f1_09[0]: Requester; f1_10[0]: 7. Account numbers
        f1_11/12/13[0]: SSN, f1_14/15[0]: EIN — на верхнем уровне Page1 (в форме 2017 были в SSN[0]/EmployerID[0])
        */
        const string page = "topmostSubform[0].Page1[0].";
        var form = pdfStamper.AcroFields;

        // iTextSharp.LGPLv2.Core на null бросает ArgumentNullException (iTextSharp 5 V1 оставлял поле пустым) — пустые W9NamePerson, W9AccountNumber и т.п. роняли Accept (AI/wiki/app-testing.md 6.21)
        form.SetField(page + "f1_01[0]", fields["SetupPacket.W9Name"] ?? "");
        form.SetField(page + "f1_02[0]", fields["SetupPacket.W9BusinessName"] ?? "");
        form.SetField(page + "Address_ReadOrder[0].f1_07[0]", fields["SetupPacket.W9Address"] ?? "");
        form.SetField(page + "Address_ReadOrder[0].f1_08[0]", $"{fields["SetupPacket.W9City"]}, {fields["SetupPacket.W9StateId"]} {fields["SetupPacket.W9ZipCode"]}");
        form.SetField(page + "f1_09[0]", fields["SetupPacket.W9NamePerson"] ?? "");
        form.SetField(page + "f1_10[0]", fields["SetupPacket.W9AccountNumber"] ?? "");
        form.SetField(page + "f1_05[0]", fields["SetupPacket.W9ExemptPayeeCode"] ?? "");
        form.SetField(page + "f1_06[0]", fields["SetupPacket.W9FATCACode"] ?? "");
        var arr1 = fields["SetupPacket.W9SocialSecurityNumber"]?.Split('-');
        if (fields["SetupPacket.W9SocialSecurityNumber"] != null && arr1.Length == 3) {
            form.SetField(page + "f1_11[0]", arr1[0]);
            form.SetField(page + "f1_12[0]", arr1[1]);
            form.SetField(page + "f1_13[0]", arr1[2]);
        }
        var arr2 = fields["SetupPacket.W9EmployerIdentificationNumber"]?.Split('-');
        if (fields["SetupPacket.W9EmployerIdentificationNumber"] != null && arr2.Length == 2) {
            form.SetField(page + "f1_14[0]", arr2[0]);
            form.SetField(page + "f1_15[0]", arr2[1]);
        }

        // 3a, 3b — настоящие чекбоксы формы (в форме 2017 галочка рисовалась поверх символом ZapfDingbats); W9CompanyType 1..7 → c1_1[0..6]
        if (setupPacket.W9CompanyType is >= 1 and <= 7) {
            SetCheckbox(form, $"{page}Boxes3a-b_ReadOrder[0].c1_1[{setupPacket.W9CompanyType - 1}]");
        }
        if (setupPacket.W9ForeignPartnersHas) {
            SetCheckbox(form, $"{page}Boxes3a-b_ReadOrder[0].c1_2[0]");
        }
        form.SetField(page + "Boxes3a-b_ReadOrder[0].f1_03[0]", fields["SetupPacket.W9TaxClassification"] ?? "");
        form.SetField(page + "Boxes3a-b_ReadOrder[0].f1_04[0]", fields["SetupPacket.W9OtherTaxClassification"] ?? "");

        #region signature
        {
            PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
            // signature
            string fontPath = Path.Combine(AppSettings.FontsPath, "DancingScript-Bold.ttf");
            BaseFont font = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            pdfContentByte.SetColorFill(BaseColor.Black);
            pdfContentByte.SetFontAndSize(font, 20);
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, fields["SetupPacket.W9Signature"] ?? "-", 125, 195, 0);
            pdfContentByte.EndText();
            // date
            font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            pdfContentByte.SetColorFill(BaseColor.Black);
            pdfContentByte.SetFontAndSize(font, 10);
            pdfContentByte.BeginText();
            pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, fields["SetupPacket.W9AgreeDate"] ?? "-", 410, 195, 0);
            pdfContentByte.EndText();
        }
        #endregion signature

        pdfStamper.SetFullCompression();
        pdfStamper.FormFlattening = true;
        pdfStamper.Close();
        pdfReader.Close();

        return ms.ToArray();
    }

    /// <summary>
    ///     Включает чекбокс формы: значение «включено» у каждого чекбокса W-9 своё ("1".."7"), берётся из состояний (как Safer PdfService) </summary>
    private static void SetCheckbox(AcroFields form, string field) {
        var on = form.GetAppearanceStates(field)?.FirstOrDefault(s => s != "Off");
        if (on != null) {
            form.SetField(field, on);
        }
    }
}
