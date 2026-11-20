using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Converters;

/// <summary>
///     ActualCompanyDto ↔ форма /Admin/DispatchCompanies. Обратно форма накладывается на DTO из БД: поля, которых на форме нет
///     (NextLoadExtra, FactoringCompanyJson, Bnsf*), не затираются; пустой пароль → null → прежний (DispatchCompanyService.OnUpdate) </summary>
public static class DispatchCompanyFormConverter {

    public static DispatchCompanyFormViewModel ToVm(ActualCompanyDto dto) {
        return new DispatchCompanyFormViewModel {
            EntityId = dto.EntityId,
            McNumber = dto.McNumber,
            CompanyId = dto.CompanyId,
            BoardType = dto.BoardType,
            LogoSmall = dto.LogoSmall,
            LogoBig = dto.LogoBig,
            LegalName = dto.LegalName,
            PhysicalAddress = dto.PhysicalAddress,
            MailingAddress = dto.MailingAddress,
            PaymentInstructions = dto.PaymentInstructions,
            ShippersPhone = dto.ShippersPhone,
            CellPhone = dto.CellPhone,
            WorkPhone = dto.WorkPhone,
            WorkFax = dto.WorkFax,
            WorkEmail = dto.WorkEmail,
            SMTP_Email = dto.SMTP_Email,
            SMTP_Host = dto.SMTP_Host,
            SMTP_Port = dto.SMTP_Port,
            SMTP_SchemeEmail = dto.SMTP_SchemeEmail,
            SMTP_SchemeHost = dto.SMTP_SchemeHost,
            SMTP_SchemePort = dto.SMTP_SchemePort,
            SMS_Phone = dto.SMS_Phone,
            ConfirmationFile = dto.ConfirmationFile,
            BOLFile = dto.BOLFile,
            InvoiceFile = dto.InvoiceFile,
            InvoiceNumberAsLoadNumber = dto.InvoiceNumberAsLoadNumber,
            NextConfirmationNumber = dto.NextConfirmationNumber,
            NextInvoiceNumber = dto.NextInvoiceNumber,
            NextLoadNumber = dto.NextLoadNumber,
            SubmitRates = dto.SubmitRates,
            BnsfLogin = dto.BnsfLogin,
            CarrierViewToken = dto.CarrierViewToken,
            Customizes = (dto.Customizes ?? new List<ActualCompanyCustomDto>()).Select(e => new DispatchCompanyCustomRowViewModel {
                EntityId = e.EntityId,
                Type = e.Type,
                Name = e.Name,
                Params = e.Params
            }).ToList()
        };
    }

    public static void Apply(DispatchCompanyFormViewModel vm, ActualCompanyDto dto) {
        dto.McNumber = vm.McNumber ?? 0;
        dto.CompanyId = vm.CompanyId;
        dto.BoardType = vm.BoardType;
        dto.LogoSmall = vm.LogoSmall;
        dto.LogoBig = vm.LogoBig;
        dto.LegalName = vm.LegalName;
        dto.PhysicalAddress = vm.PhysicalAddress;
        dto.MailingAddress = vm.MailingAddress;
        dto.PaymentInstructions = vm.PaymentInstructions;
        dto.ShippersPhone = vm.ShippersPhone;
        dto.CellPhone = vm.CellPhone;
        dto.WorkPhone = vm.WorkPhone;
        dto.WorkFax = vm.WorkFax;
        dto.WorkEmail = vm.WorkEmail;
        dto.SMTP_Email = vm.SMTP_Email?.Trim();
        dto.SMTP_Password = NullIfEmpty(vm.SMTP_Password);
        dto.SMTP_Host = vm.SMTP_Host?.Trim();
        dto.SMTP_Port = vm.SMTP_Port;
        dto.SMTP_SchemeEmail = vm.SMTP_SchemeEmail?.Trim();
        dto.SMTP_SchemePassword = NullIfEmpty(vm.SMTP_SchemePassword);
        dto.SMTP_SchemeHost = vm.SMTP_SchemeHost?.Trim();
        dto.SMTP_SchemePort = vm.SMTP_SchemePort;
        dto.SMS_Phone = vm.SMS_Phone;
        dto.InvoiceNumberAsLoadNumber = vm.InvoiceNumberAsLoadNumber;
        dto.NextConfirmationNumber = vm.NextConfirmationNumber ?? 1;
        dto.NextInvoiceNumber = vm.NextInvoiceNumber ?? 1;
        dto.NextLoadNumber = vm.NextLoadNumber ?? 1;
        dto.SubmitRates = vm.SubmitRates;
        dto.BnsfLogin = vm.BnsfLogin;
        dto.BnsfPassword = NullIfEmpty(vm.BnsfPassword);
        dto.CarrierViewToken = vm.CarrierViewToken;
        dto.Customizes = vm.Customizes.Select(e => new ActualCompanyCustomDto {
            EntityId = e.EntityId,
            ActualCompanyId = dto.EntityId,
            Type = e.Type,
            Name = e.Name,
            Params = e.Params
        }).ToList();
    }

    /// <summary>
    ///     Значение поля пароля, когда пароль задан: браузер показывает точки, сам пароль в страницу не попадает </summary>
    public const string PasswordMask = "********";

    /// <summary>
    ///     Пусто или маска → null → прежний пароль из БД </summary>
    private static string NullIfEmpty(string value) => String.IsNullOrEmpty(value) || value == PasswordMask ? null : value;
}
