using System.ComponentModel.DataAnnotations;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Logic.Dtos;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     /Admin/DispatchCompanies — V1 ActualCompany/Index: Admin видит все компании dispatch, Owner — своей компании </summary>
public class DispatchCompaniesIndexViewModel {
    public bool IsAdmin { get; set; }
    public List<ActualCompanyDto> Companies { get; set; }
    /// <summary>
    ///     CompanyId → имя (колонка Company у Admin) </summary>
    public Dictionary<Guid, string> CompanyNames { get; set; }
}

/// <summary>
///     Форма компании dispatch (V1 ActualCompanyModel.ts). Пароли не показываются: пусто — остаётся прежний (OnUpdate сервиса).
///     Логотипы — data URL в скрытых полях, файл логотипа или шаблона RTF приходит отдельным IFormFile </summary>
public class DispatchCompanyFormViewModel {
    public Guid EntityId { get; set; }

    [Required, Display(Name = "MC#")]
    public int? McNumber { get; set; }
    public Guid CompanyId { get; set; }
    [Display(Name = "Board")]
    public DispatchBoardType BoardType { get; set; }

    public string LogoSmall { get; set; }
    public string LogoBig { get; set; }

    [Required, Display(Name = "Legal Name")]
    public string LegalName { get; set; }
    [Required, Display(Name = "Physical Address")]
    public string PhysicalAddress { get; set; }
    [Required, Display(Name = "Mailing Address")]
    public string MailingAddress { get; set; }
    public string PaymentInstructions { get; set; }
    public string ShippersPhone { get; set; }
    public string CellPhone { get; set; }
    public string WorkPhone { get; set; }
    public string WorkFax { get; set; }
    public string WorkEmail { get; set; }

    public string SMTP_Email { get; set; }
    public string SMTP_Password { get; set; }
    public string SMTP_Host { get; set; }
    public int SMTP_Port { get; set; }

    public string SMTP_SchemeEmail { get; set; }
    public string SMTP_SchemePassword { get; set; }
    public string SMTP_SchemeHost { get; set; }
    public int SMTP_SchemePort { get; set; }

    public string SMS_Phone { get; set; }

    /// <summary>
    ///     Имена текущих шаблонов — ссылка download </summary>
    public string ConfirmationFile { get; set; }
    public string BOLFile { get; set; }
    public string InvoiceFile { get; set; }

    public bool InvoiceNumberAsLoadNumber { get; set; }
    [Required, Display(Name = "Next Confirmation#")]
    public int? NextConfirmationNumber { get; set; }
    [Required, Display(Name = "Next Invoice#")]
    public int? NextInvoiceNumber { get; set; }
    [Required, Display(Name = "Next Load#")]
    public int? NextLoadNumber { get; set; }
    public bool SubmitRates { get; set; }

    public string BnsfLogin { get; set; }
    public string BnsfPassword { get; set; }
    public string CarrierViewToken { get; set; }

    public List<DispatchCompanyCustomRowViewModel> Customizes { get; set; } = new();

    #region [ только для отрисовки ]

    public bool IsAdmin { get; set; }
    public List<CompanyDto> Companies { get; set; }
    public bool HasSmtpPassword { get; set; }
    public bool HasSmtpSchemePassword { get; set; }
    public bool HasBnsfPassword { get; set; }

    #endregion [ только для отрисовки ]
}

/// <summary>
///     Кастомизация нумерации (V1 ActualCompanyCustomModel), Type — CompanyCustomizationEnum </summary>
public class DispatchCompanyCustomRowViewModel {
    public Guid EntityId { get; set; }
    public int Type { get; set; }
    public string Name { get; set; }
    public string Params { get; set; }
}
