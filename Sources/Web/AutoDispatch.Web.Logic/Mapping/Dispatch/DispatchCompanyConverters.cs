using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

/// <summary>
///     V1 ActualCompany_DtoToModel: ActualCompanyDto контракта ↔ DispatchCompany </summary>
public class DispatchCompany_DtoToModel : Base_DtoToModel<ActualCompanyDto, DispatchCompany> {
    protected override DispatchCompany ConvertCore(ActualCompanyDto source) {
        var destination = base.ConvertCore(source);
        destination.CompanyId = source.CompanyId;
        destination.BoardType = (byte)source.BoardType;
        destination.PhysicalAddress = source.PhysicalAddress;
        destination.MailingAddress = source.MailingAddress;
        destination.LegalName = source.LegalName;
        destination.ShippersPhone = source.ShippersPhone;
        destination.CellPhone = source.CellPhone;
        destination.WorkPhone = source.WorkPhone;
        destination.WorkFax = source.WorkFax;
        destination.WorkEmail = source.WorkEmail;
        destination.PaymentInstructions = source.PaymentInstructions;

        destination.SMTP_Email = source.SMTP_Email;
        destination.SMTP_Password = source.SMTP_Password; // NOTE: allow to change
        destination.SMTP_Host = source.SMTP_Host;
        destination.SMTP_Port = source.SMTP_Port;

        destination.SMTP_SchemeEmail = source.SMTP_SchemeEmail;
        destination.SMTP_SchemePassword = source.SMTP_SchemePassword; // NOTE: allow to change
        destination.SMTP_SchemeHost = source.SMTP_SchemeHost;
        destination.SMTP_SchemePort = source.SMTP_SchemePort;

        destination.SMS_Phone = source.SMS_Phone;

        destination.ConfirmationFile = source.ConfirmationFile;
        destination.BOLFile = source.BOLFile;
        destination.InvoiceFile = source.InvoiceFile;

        destination.LogoBig = source.LogoBig;
        destination.LogoSmall = source.LogoSmall;
        destination.McNumber = source.McNumber;
        destination.NextConfirmationNumber = source.NextConfirmationNumber;
        destination.NextInvoiceNumber = source.NextInvoiceNumber;
        destination.NextLoadNumber = source.NextLoadNumber;
        destination.NextLoadExtra = source.NextLoadExtra;
        destination.InvoiceNumberAsLoadNumber = source.InvoiceNumberAsLoadNumber;
        destination.SubmitRates = source.SubmitRates;

        destination.BnsfLogin = source.BnsfLogin;
        destination.BnsfPassword = source.BnsfPassword;
        destination.BnsfCookies = source.BnsfCookies;
        destination.BnsfLastMessage = source.BnsfLastMessage;
        destination.BnsfLastDone = source.BnsfLastDone;

        destination.CarrierViewToken = String.IsNullOrWhiteSpace(source.CarrierViewToken) ? null : source.CarrierViewToken;

        destination.FactoringCompanyJson = source.FactoringCompanyJson;
        return destination;
    }
}

/// <summary>
///     V1 ActualCompany_ModelToDto: пароли не отдаются; *Data — RTF шаблоны компании (свой файл или Documents/ по умолчанию);
///     Customizes — из навигации, если загружена </summary>
public class DispatchCompany_ModelToDto : Base_ModelToDto<DispatchCompany, ActualCompanyDto> {
    protected override ActualCompanyDto ConvertCore(DispatchCompany source) {
        var destination = base.ConvertCore(source);
        destination.CompanyId = source.CompanyId;
        destination.BoardType = (DispatchBoardType)source.BoardType;
        destination.PhysicalAddress = source.PhysicalAddress;
        destination.MailingAddress = source.MailingAddress;
        destination.LegalName = source.LegalName;
        destination.ShippersPhone = source.ShippersPhone;
        destination.CellPhone = source.CellPhone;
        destination.WorkPhone = source.WorkPhone;
        destination.WorkFax = source.WorkFax;
        destination.WorkEmail = source.WorkEmail;
        destination.PaymentInstructions = source.PaymentInstructions;

        destination.SMTP_Email = source.SMTP_Email;
        // SMTP_Password — NOTE: skip
        destination.SMTP_Host = source.SMTP_Host;
        destination.SMTP_Port = source.SMTP_Port;

        destination.SMTP_SchemeEmail = source.SMTP_SchemeEmail;
        // SMTP_SchemePassword — NOTE: skip
        destination.SMTP_SchemeHost = source.SMTP_SchemeHost;
        destination.SMTP_SchemePort = source.SMTP_SchemePort;

        destination.SMS_Phone = source.SMS_Phone;

        destination.ConfirmationFile = source.ConfirmationFile;
        destination.BOLFile = source.BOLFile;
        destination.InvoiceFile = source.InvoiceFile;

        // special file or default
        var board = (DispatchBoardType)source.BoardType;
        if (board == DispatchBoardType.Broker || board == DispatchBoardType.BrokerAccount) {
            destination.ConfirmationData = ReadTemplate(source, destination.ConfirmationFile, "confirmation.rtf");
            destination.BOLData = ReadTemplate(source, destination.BOLFile, "bill_of_lading.rtf");
        }
        if (board == DispatchBoardType.Carrier || board == DispatchBoardType.CarrierAccount) {
            destination.InvoiceData = ReadTemplate(source, destination.InvoiceFile, "invoice.rtf");
        }

        destination.LogoBig = source.LogoBig;
        destination.LogoSmall = source.LogoSmall;
        destination.McNumber = source.McNumber;
        destination.NextConfirmationNumber = source.NextConfirmationNumber;
        destination.NextInvoiceNumber = source.NextInvoiceNumber;
        destination.NextLoadNumber = source.NextLoadNumber;
        destination.NextLoadExtra = source.NextLoadExtra;
        destination.InvoiceNumberAsLoadNumber = source.InvoiceNumberAsLoadNumber;
        destination.SubmitRates = source.SubmitRates;

        destination.BnsfLogin = source.BnsfLogin;
        // BnsfPassword — NOTE: skip
        destination.BnsfLastMessage = source.BnsfLastMessage;
        destination.BnsfLastDone = source.BnsfLastDone;

        destination.CarrierViewToken = source.CarrierViewToken;

        destination.FactoringCompanyJson = source.FactoringCompanyJson;

        destination.Customizes = Context.Mapper.Map<List<ActualCompanyCustomDto>>(source.Customizes);

        return destination;
    }

    private static string ReadTemplate(DispatchCompany source, string fileName, string defaultName) {
        try {
            var path = fileName != null
                ? DispatchFileHelper.FileGetFullPath(source.EntityId, fileName)
                : Path.Combine(AppSettings.DocumentsPath, defaultName);
            return File.ReadAllText(path);
        } catch (Exception e) {
            _l.e($"{defaultName}. {source.CompanyId}. {source.LegalName}", e);
            return null;
        }
    }
}
