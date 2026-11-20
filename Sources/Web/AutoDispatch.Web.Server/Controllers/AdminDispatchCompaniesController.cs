using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Logic.WebServices;
using AutoDispatch.Web.Server.Converters;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Компании dispatch (V1 ActualCompanyController, страница ActualCompany/Index «Dispatch Companies»; таблица d_ActualCompany → DispatchCompany,
///     d_ActualCompanyCustom → DispatchCompanyCustom): Admin — все компании и создание новых, Owner — компании своей Company.
///     SMTP, шаблоны confirmation/BOL/invoice, логотипы, нумерация, BNSF, CarrierView, кастомизации нумерации </summary>
[Authorize(Roles = nameof(RoleEnum.Admin) + "," + nameof(RoleEnum.Owner))]
[Route("Admin/DispatchCompanies/[action]")]
public class AdminDispatchCompaniesController : BaseController {

    private const string IndexUrl = "/Admin/DispatchCompanies";

    private readonly IWebHostEnvironment _env;

    private DispatchCompanyService Service => Host.GetService<DispatchCompanyService>();
    private CompanyService Companies => Host.GetService<CompanyService>();
    private bool IsAdmin => User.IsInRole(nameof(RoleEnum.Admin));

    #region [ .ctor ]

    public AdminDispatchCompaniesController(ServicesHost host, IWebHostEnvironment env) : base(host) {
        _env = env;
    }

    #endregion [ .ctor ]

    [HttpGet("~" + IndexUrl)]
    public async Task<ActionResult> Index() {
        var companies = await Service.GetAllAsync(IsAdmin ? null : GetCompanyId());
        return View(new DispatchCompaniesIndexViewModel {
            IsAdmin = IsAdmin,
            Companies = companies.OrderBy(e => e.CompanyId).ThenBy(e => e.BoardType).ToList(),
            CompanyNames = (await Companies.GetAllAsync(false)).ToDictionary(e => e.Id, e => e.Name)
        });
    }

    /// <summary>
    ///     Новая компания — только Admin (V1: кнопка + Add только у Admin); дефолты V1 ActualCompanyModel.ts </summary>
    [HttpGet]
    public async Task<ActionResult> Create() {
        if (IsAdmin == false) return Forbid();
        var model = new DispatchCompanyFormViewModel {
            CompanyId = GetCompanyId(),
            BoardType = DispatchBoardType.Carrier,
            LogoSmall = DefaultLogo("dispatch_logo_small.png"),
            LogoBig = DefaultLogo("dispatch_logo_big.png"),
            SMTP_Host = "smtp.gmail.com",
            SMTP_Port = 587,
            SMTP_SchemeHost = "smtp.gmail.com",
            SMTP_SchemePort = 587,
            NextConfirmationNumber = 1,
            NextInvoiceNumber = 1,
            NextLoadNumber = 1
        };
        return View("Edit", await FillAsync(model));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id) {
        var dto = await GetAllowedAsync(id);
        if (dto == null) return NotFound();
        return View("Edit", await FillAsync(DispatchCompanyFormConverter.ToVm(dto)));
    }

    /// <summary>
    ///     Обычный POST multipart (логотипы и шаблоны RTF файлами); форма накладывается на DTO из БД </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Save(DispatchCompanyFormViewModel model,
            IFormFile logoSmallFile, IFormFile logoBigFile, IFormFile confirmationFile, IFormFile bolFile, IFormFile invoiceFile) {
        ActualCompanyDto dto;
        if (model.EntityId == Guid.Empty) {
            if (IsAdmin == false) return Forbid();
            dto = new ActualCompanyDto();
        } else {
            dto = await GetAllowedAsync(model.EntityId);
            if (dto == null) return NotFound();
        }
        if (IsAdmin == false) {
            model.CompanyId = GetCompanyId();
        }
        if (model.CompanyId == Guid.Empty) {
            ModelState.AddModelError(nameof(model.CompanyId), "Company is required");
        }

        // логотипы — до проверки формы: при ошибке выбранный логотип остаётся на форме (data URL)
        model.LogoSmall = await ReadLogoAsync(logoSmallFile, 64, 64, nameof(model.LogoSmall)) ?? model.LogoSmall;
        model.LogoBig = await ReadLogoAsync(logoBigFile, 1024, 256, nameof(model.LogoBig)) ?? model.LogoBig;
        if (String.IsNullOrEmpty(model.LogoSmall)) ModelState.AddModelError(nameof(model.LogoSmall), "Small logo is required");
        if (String.IsNullOrEmpty(model.LogoBig)) ModelState.AddModelError(nameof(model.LogoBig), "Big logo is required");

        if (ModelState.IsValid == false) {
            return View("Edit", await FillAsync(model));
        }

        DispatchCompanyFormConverter.Apply(model, dto);
        // шаблоны: V1 — binaryString файла, сервис пишет его Encoding.ASCII
        (dto.ConfirmationFile, dto.ConfirmationData) = await ReadTemplateAsync(confirmationFile, dto.ConfirmationFile);
        (dto.BOLFile, dto.BOLData) = await ReadTemplateAsync(bolFile, dto.BOLFile);
        (dto.InvoiceFile, dto.InvoiceData) = await ReadTemplateAsync(invoiceFile, dto.InvoiceFile);

        using var scope = _l.Scope("admin");
        await Service.SaveAsync(dto, GetUserId());
        _l.i($"Dispatch company saved: {dto.LegalName} MC# {dto.McNumber} ({dto.EntityId}) by {User.Identity?.Name}");
        return RedirectAfterPost(IndexUrl);
    }

    [HttpPost("{id:guid}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(Guid id) {
        var dto = await GetAllowedAsync(id);
        if (dto == null) return NotFound();
        using var scope = _l.Scope("admin");
        await Service.MarkAsDeletedIdAsync(id, GetUserId());
        _l.i($"Dispatch company deleted: {dto.LegalName} MC# {dto.McNumber} ({id}) by {User.Identity?.Name}");
        return RedirectAfterPost(IndexUrl);
    }

    /// <summary>
    ///     FMCSA Fields → Load (V1 ActualCompany/Search + LoadFmscaFields): поля карточки по MC# и сгенерированные логотипы; форма перерисовывается </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> LoadFmcsa(DispatchCompanyFormViewModel model) {
        ModelState.Clear();
        if (model.McNumber == null) {
            ModelState.AddModelError(nameof(model.McNumber), "MC# is required");
            return PartialView("_DispatchCompanyFormPart", await FillAsync(model));
        }
        try {
            var fields = await Host.GetService<DatCompanyService>().GetFieldsAsync(null, model.McNumber);
            model.PhysicalAddress = fields.GetValueOrDefault("Physical Address:");
            model.MailingAddress = fields.GetValueOrDefault("Mailing Address:");
            model.LegalName = fields.GetValueOrDefault("Legal Name:");
            model.WorkPhone = fields.GetValueOrDefault("Phone:");
            model.CellPhone = fields.GetValueOrDefault("PHY_CELL_NUM");
            model.WorkFax = fields.GetValueOrDefault("PHY_FAX_NUM");
            model.WorkEmail = fields.GetValueOrDefault("EMAIL");
            model.BoardType = String.IsNullOrEmpty(fields.GetValueOrDefault("BrokerAuthority")) ? DispatchBoardType.Carrier : DispatchBoardType.Broker;
            var generator = new LogoGenerator();
            model.LogoSmall = generator.GetSmallLogoPng(model.LegalName);
            model.LogoBig = generator.GetBigLogoPng(model.LegalName);
        } catch (Exception e) {
            ModelState.AddModelError(String.Empty, e.GetBaseException().Message);
        }
        return PartialView("_DispatchCompanyFormPart", await FillAsync(model));
    }

    /// <summary>
    ///     Test SMTP — значения с формы без сохранения, пароль пустой → из БД (V1 SendTestEmail) </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public Task<ActionResult> SendTestEmail(DispatchCompanyFormViewModel model, string testEmail) {
        return TestAsync(model, dto => {
            CheckRecipient(testEmail);
            return Host.GetService<DatCompanyService>().SendTestEmail(dto, testEmail);
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<ActionResult> SendTestSchemeEmail(DispatchCompanyFormViewModel model, string testSchemeEmail) {
        return TestAsync(model, dto => {
            CheckRecipient(testSchemeEmail);
            return Host.GetService<DatCompanyService>().SendTestSchemeEmail(dto, testSchemeEmail);
        });
    }

    /// <summary>
    ///     Получатель тестового письма: невалидный адрес SmtpClient отбрасывает и отвечает непонятным «A recipient must be specified» </summary>
    private static void CheckRecipient(string email) {
        if (System.Net.Mail.MailAddress.TryCreate(email, out _) == false) {
            throw new Exception("Enter the recipient email address to send the test to");
        }
    }

    /// <summary>
    ///     Test SMS с Out SMS Phone формы. V1 слал через Voxbone (не переносится) — здесь Bandwidth, как SMS грузов </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public Task<ActionResult> SendTestSms(DispatchCompanyFormViewModel model, string testSms) {
        return TestAsync(model, async dto => {
            var result = await Host.GetService<SmsWebService>().SendSmsAsync(dto.SMS_Phone, testSms, "TEST FROM MYAUTOCALL");
            if (result.Success == false) throw new Exception(result.ErrorMessage);
        });
    }

    /// <summary>
    ///     Текущий шаблон компании (V1 ActualCompany/Download) — только файлы, указанные в карточке </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Download(Guid id, string fileName) {
        var dto = await GetAllowedAsync(id);
        if (dto == null || fileName == null || new[] { dto.ConfirmationFile, dto.BOLFile, dto.InvoiceFile }.Contains(fileName) == false) {
            return NotFound();
        }
        var response = await Host.GetService<DispatchService>().FileLoad(new FileLoadRequestData(GetUserId()) {
            File = new FileDto { EntityId = id, Name = fileName }
        });
        return File(response.File.Data, "application/rtf", response.File.Name);
    }

    #region [ private ]

    /// <summary>
    ///     Компания по Id, если пользователю можно её видеть: Admin — любая, Owner — своей Company </summary>
    private async Task<ActualCompanyDto> GetAllowedAsync(Guid id) {
        var dto = await Service.GetByIdAsync(id);
        if (dto == null || (IsAdmin == false && dto.CompanyId != GetCompanyId())) {
            return null;
        }
        return dto;
    }

    private async Task<DispatchCompanyFormViewModel> FillAsync(DispatchCompanyFormViewModel model) {
        model.IsAdmin = IsAdmin;
        model.Companies = IsAdmin ? await Companies.GetAllAsync(false) : null;
        if (model.EntityId != Guid.Empty) {
            (model.HasSmtpPassword, model.HasSmtpSchemePassword, model.HasBnsfPassword) = await Service.HasPasswordsAsync(model.EntityId);
        }
        return model;
    }

    /// <summary>
    ///     Кнопки Send у Test SMTP / Test SMS: htmx-фрагмент «Success» или текст ошибки </summary>
    private async Task<ActionResult> TestAsync(DispatchCompanyFormViewModel model, Func<ActualCompanyDto, Task> send) {
        if (model.EntityId != Guid.Empty && await GetAllowedAsync(model.EntityId) == null) {
            return NotFound();
        }
        var dto = new ActualCompanyDto { EntityId = model.EntityId };
        DispatchCompanyFormConverter.Apply(model, dto);
        try {
            await send(dto);
            return Content("<span class=\"text-success\">Success</span>", "text/html");
        } catch (Exception e) {
            return Content($"<span class=\"text-error\">{WebUtility.HtmlEncode(e.GetBaseException().Message)}</span>", "text/html");
        }
    }

    /// <summary>
    ///     Файл логотипа → data URL; размер не больше max (V1 проверял на клиенте: 64x64 и 1024x256) </summary>
    private async Task<string> ReadLogoAsync(IFormFile file, int maxWidth, int maxHeight, string field) {
        if (file == null || file.Length == 0) return null;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var data = stream.ToArray();
        var info = SKBitmap.DecodeBounds(data);
        if (info.Width == 0) {
            ModelState.AddModelError(field, "Not an image");
            return null;
        }
        if (info.Width > maxWidth || info.Height > maxHeight) {
            ModelState.AddModelError(field, $"Logo height and width must be lesser than {maxWidth}x{maxHeight}");
            return null;
        }
        return $"data:{file.ContentType};base64,{Convert.ToBase64String(data)}";
    }

    /// <summary>
    ///     Новый шаблон RTF: (имя, содержимое); без файла — прежнее имя и null (сервис файл не трогает) </summary>
    private static async Task<(string, string)> ReadTemplateAsync(IFormFile file, string currentName) {
        if (file == null || file.Length == 0) return (currentName, null);
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        return (Path.GetFileName(file.FileName), Encoding.Latin1.GetString(stream.ToArray()));
    }

    private string DefaultLogo(string fileName) {
        var path = Path.Combine(_env.WebRootPath, "images", "dispatch", fileName);
        return "data:image/png;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes(path));
    }

    #endregion [ private ]
}
