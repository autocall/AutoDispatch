using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.StaticFiles;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Extensions;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Setup packet карриера — мастер из 15 шагов (V1 SetupPacketController, маршрут CarrierRegistration, URL как в письмах V1).
///     Index — карриер по ссылке из письма без логина (пропуск — Guid карриера), View/Edit — агент под логином сайта (ссылки из App).
///     Шаг — обычная форма POST StepNN?CarrierId= → сохранение → редирект на Index (User) или на тот же шаг (Edit);
///     PrevNN — сохранить и шаг назад. Ошибки валидации — та же страница с сообщениями у полей </summary>
// без [AllowAnonymous] на классе: он перекрыл бы [Authorize] у View/Edit; остальные действия анонимны по умолчанию (fallback-политики нет)
[Route("CarrierRegistration")]
public class CarrierRegistrationController : BaseController {

    private DatCompanyService Carriers => Host.GetService<DatCompanyService>();
    private DispatchCompanyService Companies => Host.GetService<DispatchCompanyService>();
    private SetupPacketService Packets => Host.GetService<SetupPacketService>();
    private DispatchService Dispatch => Host.GetService<DispatchService>();

    public CarrierRegistrationController(ServicesHost host) : base(host) { }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
        using var scope = _l.Scope("setup-packet");
        await base.OnActionExecutionAsync(context, next);
    }

    #region [ Pages ]

    /// <summary>
    ///     Карриер по ссылке из письма: текущий шаг packet'а, без packet'а — шаг 0, завершённый — Finish </summary>
    [HttpGet("~/CarrierRegistration")]
    public async Task<ActionResult> Index(Guid carrierId) {
        var model = await LoadAsync(carrierId, PageModeEnum.User);
        if (model == null) {
            return View("Error", new SetupPacketViewModel());
        }
        if (model.Carrier.SetupPacketCompleted) {
            return View("Finish", model);
        }
        if (model.SetupPacket == null) {
            model.SetupPacket = new SetupPacketDto { Version = (byte)AppSettings.SetupPacketVersion };
            model.SetupPacketStepModel = new SetupPacketStep00Model();
            return View("Step00", model);
        }
        return await StepViewAsync(model, model.SetupPacket.Step);
    }

    [HttpGet("View"), Authorize]
    public Task<ActionResult> ViewPage(Guid carrierId, byte? step = null) => AgentPageAsync(carrierId, step, PageModeEnum.View);

    [HttpGet("Edit"), Authorize]
    public Task<ActionResult> Edit(Guid carrierId, byte? step = null) => AgentPageAsync(carrierId, step, PageModeEnum.Edit);

    private async Task<ActionResult> AgentPageAsync(Guid carrierId, byte? step, PageModeEnum mode) {
        var model = await LoadAsync(carrierId, mode);
        if (model?.SetupPacket == null || step > 14) {
            return View("Error", new SetupPacketViewModel());
        }
        return await StepViewAsync(model, step ?? model.SetupPacket.Step);
    }

    #endregion [ Pages ]

    #region [ Next ]

    [HttpPost("Step00"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Step00(Guid carrierId, [FromForm] SetupPacketStep00Model model) {
        if (model.ConfidantContact != null) {
            RequireContact(model.ConfidantContact, nameof(model.ConfidantContact));
        }
        return await NextAsync(carrierId, model, createPacket: true);
    }

    [HttpPost("Step01"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step01(Guid carrierId, [FromForm] SetupPacketStep01Model model) => NextAsync(carrierId, model);

    [HttpPost("Step02"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Step02(Guid carrierId, [FromForm] SetupPacketStep02Model model) {
        // правила knockout.validation V1: основной адрес и Pay To обязательны, контакты — если включены
        Require(model.LegalName, nameof(model.LegalName));
        Require(model.PhysicalAddress, nameof(model.PhysicalAddress));
        Require(model.PhysicalCity, nameof(model.PhysicalCity));
        Require(model.PhysicalStateId, nameof(model.PhysicalStateId));
        Require(model.PhysicalZipCode, nameof(model.PhysicalZipCode));
        Require(model.PhysicalCountryId, nameof(model.PhysicalCountryId));
        Require(model.MailingCompanyLegalName, nameof(model.MailingCompanyLegalName));
        Require(model.MailingAddress, nameof(model.MailingAddress));
        Require(model.MailingCity, nameof(model.MailingCity));
        Require(model.MailingStateId, nameof(model.MailingStateId));
        Require(model.MailingZipCode, nameof(model.MailingZipCode));
        Require(model.MailingCountryId, nameof(model.MailingCountryId));
        RequireContact(model.CorporateContact, nameof(model.CorporateContact));
        RequireContact(model.SalesContact, nameof(model.SalesContact));
        RequireContact(model.DispatchContact, nameof(model.DispatchContact));
        RequireContact(model.AfterHourContact, nameof(model.AfterHourContact));
        return await NextAsync(carrierId, model);
    }

    [HttpPost("Step03"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step03(Guid carrierId, [FromForm] SetupPacketStep03Model model) => NextAsync(carrierId, model);

    [HttpPost("Step04"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Step04(Guid carrierId, [FromForm] SetupPacketStep04Model model) {
        return await NextAsync(carrierId, model, before: dto => {
            if (dto.Version >= 2) {
                // версия 2: две подписи (договор и политика re-brokering), дата подписи — первой подписи
                Require(model.Agreement_02Signature, nameof(model.Agreement_02Signature), "You must sign the agreement.");
                Require(model.Agreement2_02Signature, nameof(model.Agreement2_02Signature), "You must sign the agreement.");
                model.Agreement_02Date ??= dto.Agreement_02Date ?? PacificNow();
                model.Agreement_02SignatureDate ??= dto.Agreement_02SignatureDate ?? PacificNow();
                model.AgreementAgree = dto.AgreementAgree;
                model.AgreementDate = dto.AgreementDate;
            } else if (model.AgreementAgree == false) {
                ModelState.AddModelError(nameof(model.AgreementAgree), "You must accept the agreement");
            }
        });
    }

    [HttpPost("Step05"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step05(Guid carrierId, [FromForm] SetupPacketStep05Model model) => NextAsync(carrierId, model);

    [HttpPost("Step06"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Step06(Guid carrierId, [FromForm] SetupPacketStep06Model model) {
        RequireInsurance(model.GeneralLiabilityInsuranceAgency, nameof(model.GeneralLiabilityInsuranceAgency));
        RequireInsurance(model.AutoInsuranceAgency, nameof(model.AutoInsuranceAgency));
        RequireInsurance(model.CargoInsuranceAgency, nameof(model.CargoInsuranceAgency));
        RequireInsurance(model.WorkersCompensationInsuranceAgency, nameof(model.WorkersCompensationInsuranceAgency));
        // без страховки workers compensation — подпись отказа
        if (model.WorkersCompensationInsuranceAgency?.HasInsurance == false) {
            Require(model.WorkersCompensationSignature, nameof(model.WorkersCompensationSignature));
        }
        return await NextAsync(carrierId, model);
    }

    [HttpPost("Step07"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Step07(Guid carrierId, [FromForm] SetupPacketStep07Model model) {
        // правила V1 SetupPacketStep07Model.ts: имя, бизнес-имя, адрес; классификация — только для LLC (тип 6); SSN или EIN в формате
        Require(model.W9Name, nameof(model.W9Name));
        Require(model.W9BusinessName, nameof(model.W9BusinessName));
        if (model.W9CompanyType == 6) {
            Require(model.W9TaxClassification, nameof(model.W9TaxClassification));
        }
        Require(model.W9Address, nameof(model.W9Address));
        Require(model.W9City, nameof(model.W9City));
        Require(model.W9StateId, nameof(model.W9StateId));
        Require(model.W9ZipCode, nameof(model.W9ZipCode));
        var ssn = model.W9SocialSecurityNumber?.Trim();
        var ein = model.W9EmployerIdentificationNumber?.Trim();
        if (String.IsNullOrEmpty(ssn) && String.IsNullOrEmpty(ein)) {
            ModelState.AddModelError(nameof(model.W9SocialSecurityNumber), "Social security number or employer identification number is required");
        }
        // отступление от V1 (как Safer, решено 2026-11-23, AI/wiki/plan.md): W-9 Part I — один TIN, SSN или EIN, не оба
        if (String.IsNullOrEmpty(ssn) == false && String.IsNullOrEmpty(ein) == false) {
            ModelState.AddModelError(nameof(model.W9EmployerIdentificationNumber), "Enter only SSN or EIN — not both");
        }
        if (String.IsNullOrEmpty(ssn) == false && System.Text.RegularExpressions.Regex.IsMatch(ssn, @"^[0-9]{3}-[0-9]{2}-[0-9]{4}$") == false) {
            ModelState.AddModelError(nameof(model.W9SocialSecurityNumber), "Social security number format is incorrect. Must be ###-##-####");
        }
        if (String.IsNullOrEmpty(ein) == false && System.Text.RegularExpressions.Regex.IsMatch(ein, @"^[0-9]{2}-[0-9]{7}$") == false) {
            ModelState.AddModelError(nameof(model.W9EmployerIdentificationNumber), "Employer identification number format is incorrect. Must be ##-#######");
        }
        if (model.W9Agree == false) {
            ModelState.AddModelError(nameof(model.W9Agree), "You must agree to the Terms below in order to continue.");
        }
        Require(model.W9Signature, nameof(model.W9Signature));
        return await NextAsync(carrierId, model);
    }

    [HttpPost("Step08"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step08(Guid carrierId, [FromForm] SetupPacketStep08Model model) => NextAsync(carrierId, model);

    [HttpPost("Step09"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step09(Guid carrierId, [FromForm] SetupPacketStep09Model model) => NextAsync(carrierId, model);

    [HttpPost("Step10"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step10(Guid carrierId, [FromForm] SetupPacketStep10Model model) => NextAsync(carrierId, model, before: dto => {
        if (model.NewReference != null && String.IsNullOrEmpty(model.NewReference.Company) == false) {
            dto.References ??= new List<SetupPacketReferenceDto>();
            dto.References.Add(model.NewReference);
        }
    });

    [HttpPost("Step11"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step11(Guid carrierId, [FromForm] SetupPacketStep11Model model) => NextAsync(carrierId, model);

    [HttpPost("Step12"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step12(Guid carrierId, [FromForm] SetupPacketStep12Model model) => NextAsync(carrierId, model);

    /// <summary>
    ///     Документы: multipart с файлами MC и COI (V1 — base64 в JSON через knockout-file-bindings) </summary>
    [HttpPost("Step13"), ValidateAntiForgeryToken]
    public Task<ActionResult> Step13(Guid carrierId, [FromForm] SetupPacketStep13Model model, IFormFile fileMC, IFormFile fileCOI) {
        return NextAsync(carrierId, model, before: async dto => {
            if (fileMC is { Length: > 0 }) {
                await using var stream = fileMC.OpenReadStream();
                dto.FileMC = await Packets.SaveStepFileAsync(carrierId, "MC", fileMC.FileName, stream, dto.FileMC);
            }
            if (fileCOI is { Length: > 0 }) {
                await using var stream = fileCOI.OpenReadStream();
                dto.FileCOI = await Packets.SaveStepFileAsync(carrierId, "COI", fileCOI.FileName, stream, dto.FileCOI);
            }
        });
    }

    /// <summary>
    ///     Завершение: SetupPacketCompleted → SaveDatCompany шлёт письмо с PDF (DispatchService.SendSetupPacketAceeptEmailAsync);
    ///     в режиме User страница шлёт форму сама при загрузке (V1 auto-next) </summary>
    [HttpPost("Step14"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Step14(Guid carrierId, [FromForm] SetupPacketStep14Model model) {
        var page = await LoadAsync(carrierId, model.Mode);
        if (page?.SetupPacket == null) {
            return View("Error", new SetupPacketViewModel());
        }
        try {
            page.Carrier.SetupPacketCompleted = true;
            await Dispatch.SaveDatCompany(new SaveDatCompanyRequestData(page.Carrier.CreatedById) { DatCompany = page.Carrier });
            _l.i($"Setup packet completed: carrier {page.Carrier.Name} ({carrierId}), {Request.GetIp()}");
        } catch (Exception e) {
            _l.e($"Send email: carrier {carrierId}, identity {User?.Identity?.Name}", e);
            page.Error = e.Message;
            page.SetupPacketStepModel = model;
            page.Step = 14;
            return View("Step14", page);
        }
        return RedirectToStep(carrierId, model.Mode, 14);
    }

    #endregion [ Next ]

    #region [ Prev ]

    [HttpPost("Prev01"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev01(Guid carrierId, [FromForm] SetupPacketStep01Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev02"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev02(Guid carrierId, [FromForm] SetupPacketStep02Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev03"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev03(Guid carrierId, [FromForm] SetupPacketStep03Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev04"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev04(Guid carrierId, [FromForm] SetupPacketStep04Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev05"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev05(Guid carrierId, [FromForm] SetupPacketStep05Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev06"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev06(Guid carrierId, [FromForm] SetupPacketStep06Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev07"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev07(Guid carrierId, [FromForm] SetupPacketStep07Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev08"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev08(Guid carrierId, [FromForm] SetupPacketStep08Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev09"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev09(Guid carrierId, [FromForm] SetupPacketStep09Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev10"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev10(Guid carrierId, [FromForm] SetupPacketStep10Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev11"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev11(Guid carrierId, [FromForm] SetupPacketStep11Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev12"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev12(Guid carrierId, [FromForm] SetupPacketStep12Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev13"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev13(Guid carrierId, [FromForm] SetupPacketStep13Model model) => PrevAsync(carrierId, model);
    [HttpPost("Prev14"), ValidateAntiForgeryToken]
    public Task<ActionResult> Prev14(Guid carrierId, [FromForm] SetupPacketStep14Model model) => PrevAsync(carrierId, model);

    #endregion [ Prev ]

    #region [ Files ]

    [HttpGet("DownloadFileMC")]
    public Task<ActionResult> DownloadFileMC(Guid carrierId) => DownloadAsync(carrierId, p => p.FileMC);

    [HttpGet("DownloadFileCOI")]
    public Task<ActionResult> DownloadFileCOI(Guid carrierId) => DownloadAsync(carrierId, p => p.FileCOI);

    private async Task<ActionResult> DownloadAsync(Guid carrierId, Func<SetupPacketDto, string> fileName) {
        var model = await LoadAsync(carrierId, PageModeEnum.View);
        var name = model?.SetupPacket == null ? null : fileName(model.SetupPacket);
        if (name == null) {
            return View("Error", new SetupPacketViewModel());
        }
        var path = DispatchFileHelper.FileGetFullPath(carrierId, name);
        if (System.IO.File.Exists(path) == false) {
            _l.w($"Setup packet file not found: {path}");
            return View("Error", new SetupPacketViewModel());
        }
        if (new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType) == false) {
            contentType = "application/octet-stream";
        }
        return File(System.IO.File.OpenRead(path), contentType, name);
    }

    #endregion [ Files ]

    #region [ private ]

    /// <summary>
    ///     Карриер, его компания и setup packet (может не быть); null — карриер или компания не найдены </summary>
    private async Task<SetupPacketViewModel> LoadAsync(Guid carrierId, PageModeEnum mode) {
        var carrier = await Carriers.GetByIdAsync(carrierId);
        if (carrier == null) {
            _l.w($"Carrier '{carrierId}' was not found");
            return null;
        }
        var company = await Companies.GetByIdAsync(carrier.ActualCompanyId);
        if (company == null) {
            _l.w($"Company '{carrier.ActualCompanyId}' of carrier {carrierId} was not found");
            return null;
        }
        var model = new SetupPacketViewModel { Mode = mode, Carrier = carrier, DatCompanyId = carrier.EntityId, Company = company };
        if (carrier.SetupPacketId != null) {
            model.SetupPacket = await Packets.GetByIdAsync(carrier.SetupPacketId.Value);
        }
        return model;
    }

    /// <summary>
    ///     Страница шага: модель шага из DTO плюс справочники и даты по умолчанию (V1 _applyStepParamsAsync) </summary>
    private async Task<ActionResult> StepViewAsync(SetupPacketViewModel model, byte step, SetupPacketStepModel posted = null) {
        model.Step = step;
        var packet = model.SetupPacket;
        model.SetupPacketStepModel = posted ?? step switch {
            0 => Mapper.Map<SetupPacketStep00Model>(packet),
            1 => new SetupPacketStep01Model(),
            2 => Mapper.Map<SetupPacketStep02Model>(packet),
            3 => Mapper.Map<SetupPacketStep03Model>(packet),
            4 => Mapper.Map<SetupPacketStep04Model>(packet),
            5 => new SetupPacketStep05Model(),
            6 => Mapper.Map<SetupPacketStep06Model>(packet),
            7 => Mapper.Map<SetupPacketStep07Model>(packet),
            8 => Mapper.Map<SetupPacketStep08Model>(packet),
            9 => Mapper.Map<SetupPacketStep09Model>(packet),
            10 => Mapper.Map<SetupPacketStep10Model>(packet),
            11 => Mapper.Map<SetupPacketStep11Model>(packet),
            12 => Mapper.Map<SetupPacketStep12Model>(packet),
            13 => new SetupPacketStep13Model(),
            _ => new SetupPacketStep14Model()
        };
        var stepModel = (SetupPacketStepModel)model.SetupPacketStepModel;
        stepModel.Step = step;
        stepModel.Mode = model.Mode;
        model.States = StateHelper.GetStateDtos().Where(s => s.EntityId != 100).ToList();
        model.Countries = StateHelper.GetCountryDtos().Where(c => c.EntityId != 100).ToList();
        switch (step) {
            case 2:
                model.FactoringCompanies = (await Packets.GetFactoringCompaniesAsync()).OrderBy(c => c.Name).ToList();
                if (posted == null) {
                    // контакты по умолчанию — из Confidant (V1 SetupPacketStep02Model.ts)
                    var step02 = (SetupPacketStep02Model)model.SetupPacketStepModel;
                    if (step02.CorporateContact == null && packet.ConfidantContact != null) {
                        step02.CorporateContact = Mapper.Map<SetupPacketContactDto>(packet.ConfidantContact);
                        step02.CorporateContact.EntityId = Guid.NewGuid();
                    }
                    step02.CorporateContact ??= new SetupPacketContactDto { EntityId = Guid.NewGuid() };
                    step02.SalesContact ??= new SetupPacketContactDto { EntityId = Guid.NewGuid(), HasContact = packet.CorporateContact == null };
                    step02.DispatchContact ??= new SetupPacketContactDto { EntityId = Guid.NewGuid(), HasContact = packet.CorporateContact == null };
                    step02.AfterHourContact ??= new SetupPacketContactDto { EntityId = Guid.NewGuid(), HasContact = packet.CorporateContact == null };
                }
                break;
            case 4:
                if (packet.Version >= 2) {
                    packet.Agreement_02Date ??= PacificNow();
                    // V1 SetupPacketStep04_02Model.ts: moment(null) — сегодня; поле только для чтения, сохраняется при Next
                    packet.Agreement_02SignatureDate ??= PacificNow();
                } else {
                    packet.AgreementDate ??= PacificNow();
                }
                break;
            case 6:
                packet.WorkersCompensationSignatureDate ??= PacificNow();
                packet.PhysicalState = StateHelper.GetState(packet.PhysicalStateId)?.Name;
                if (posted == null) {
                    var step06 = (SetupPacketStep06Model)model.SetupPacketStepModel;
                    var first = packet.AutoInsuranceAgency == null;
                    step06.GeneralLiabilityInsuranceAgency ??= new SetupPacketInsuranceAgencyDto { EntityId = Guid.NewGuid(), HasInsurance = first };
                    step06.AutoInsuranceAgency ??= new SetupPacketInsuranceAgencyDto { EntityId = Guid.NewGuid() };
                    step06.CargoInsuranceAgency ??= new SetupPacketInsuranceAgencyDto { EntityId = Guid.NewGuid() };
                    step06.WorkersCompensationInsuranceAgency ??= new SetupPacketInsuranceAgencyDto { EntityId = Guid.NewGuid(), HasInsurance = first };
                }
                break;
            case 7:
                packet.W9AgreeDate ??= PacificNow();
                break;
            case 0:
                if (posted == null) {
                    var step00 = (SetupPacketStep00Model)model.SetupPacketStepModel;
                    step00.ConfidantContact ??= new SetupPacketContactDto { EntityId = Guid.NewGuid() };
                }
                break;
        }
        return View(ViewName(step, packet.Version), model);
    }

    /// <summary>
    ///     Версия packet'а выбирает view (V1 DispatchService.GetViewNameByVersion): Step04_02, Step06_02. Views версии 1 (Step04, Step06,
    ///     Agreement — 3 619 старых packet'ов) не перенесены: шаги 4 и 6 всегда открываются views версии 2 (AI/wiki/plan.md, «Решить») </summary>
    private static string ViewName(byte step, byte version) {
        var name = $"Step{step:00}";
        // W-9: версия 3 — Rev. March 2024, раньше — Rev. November 2017 (форма, которую карриер заполнял)
        if (step == 7) {
            return version >= 3 ? name + "_03" : name;
        }
        return step is 4 or 6 ? name + "_02" : name;
    }

    /// <summary>
    ///     Сохранить шаг и перейти дальше: модель шага → DTO, в режиме User — следующий включённый шаг.
    ///     createPacket — шаг 0 без packet'а: CreateSetupPacketAsync (карточка FMCSA, сертификат MC) и SetupPacketId у карриера </summary>
    private async Task<ActionResult> NextAsync<T>(Guid carrierId, T model, bool createPacket = false, Action<SetupPacketDto> before = null)
        where T : SetupPacketStepModel {
        return await NextAsync(carrierId, model, createPacket, before == null ? null : dto => { before(dto); return Task.CompletedTask; });
    }

    private async Task<ActionResult> NextAsync<T>(Guid carrierId, T model, bool createPacket, Func<SetupPacketDto, Task> before)
        where T : SetupPacketStepModel {
        var page = await LoadAsync(carrierId, model.Mode);
        if (page == null || (page.SetupPacket == null && createPacket == false)) {
            return View("Error", new SetupPacketViewModel());
        }
        if (page.Carrier.SetupPacketCompleted && model.Mode == PageModeEnum.User) {
            return RedirectToStep(carrierId, model.Mode, model.Step);
        }
        var created = false;
        if (page.SetupPacket == null) {
            try {
                page.SetupPacket = await Packets.CreateSetupPacketAsync(page.Carrier);
            } catch (Exception e) {
                // карточка FMCSA через Safer недоступна или карриер не найден — шаг 0 с текстом ошибки, как InfoException при сохранении
                _l.e($"Create setup packet: carrier {page.Carrier.Name} ({carrierId})", e);
                page.Error = e.Message;
                page.SetupPacket = new SetupPacketDto { Version = (byte)AppSettings.SetupPacketVersion };
                return await StepViewAsync(page, model.Step, model);
            }
            created = true;
        }
        var dto = page.SetupPacket;
        if (before != null) {
            await before(dto);
        }
        if (ModelState.IsValid == false) {
            _l.i($"Step {model.Step} invalid: carrier {carrierId}, {String.Join(", ", ModelState.Where(e => e.Value.Errors.Count > 0).Select(e => e.Key))}");
            return await StepViewAsync(page, model.Step, model);
        }
        Mapper.Map(model, dto);
        if (model.Mode == PageModeEnum.User) {
            dto.Step = page.Next(model.Step);
        }
        try {
            await Packets.SaveAsync(dto, page.Carrier.CreatedById);
            if (created) {
                page.Carrier.SetupPacketId = dto.EntityId;
                await Dispatch.SaveDatCompany(new SaveDatCompanyRequestData(page.Carrier.CreatedById) { DatCompany = page.Carrier });
            }
        } catch (InfoException e) {
            page.Error = e.Message;
            return await StepViewAsync(page, model.Step, model);
        }
        _l.i($"Step {model.Step} saved ({model.Mode}): carrier {page.Carrier.Name} ({carrierId}) → step {dto.Step}");
        return RedirectToStep(carrierId, model.Mode, model.Step);
    }

    /// <summary>
    ///     Сохранить и вернуться на предыдущий включённый шаг (V1 PrevAsync: данные шага тоже сохраняются, без валидации) </summary>
    private async Task<ActionResult> PrevAsync<T>(Guid carrierId, T model) where T : SetupPacketStepModel {
        var page = await LoadAsync(carrierId, model.Mode);
        if (page?.SetupPacket == null) {
            return View("Error", new SetupPacketViewModel());
        }
        var dto = page.SetupPacket;
        Mapper.Map(model, dto);
        if (dto.Step > 0) {
            dto.Step = page.Prev(model.Step);
        }
        await Packets.SaveAsync(dto, page.Carrier.CreatedById);
        return RedirectToStep(carrierId, model.Mode, dto.Step);
    }

    /// <summary>
    ///     User — Index показывает текущий шаг packet'а; Edit — тот же шаг (кнопка Save, V1) </summary>
    private ActionResult RedirectToStep(Guid carrierId, PageModeEnum mode, byte step) {
        return mode == PageModeEnum.Edit
            ? LocalRedirect($"/CarrierRegistration/Edit?CarrierId={carrierId}&step={step}")
            : LocalRedirect($"/CarrierRegistration/?CarrierId={carrierId}");
    }

    /// <summary>
    ///     Даты подписей — по тихоокеанскому времени, как в V1 </summary>
    private static DateTime PacificNow() => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time");

    private void Require(object value, string field, string message = "This field is required.") {
        if (value == null || value is string s && String.IsNullOrWhiteSpace(s)) {
            ModelState.AddModelError(field, message);
        }
    }

    private void RequireContact(SetupPacketContactDto contact, string prefix) {
        if (contact == null || contact.HasContact == false) return;
        Require(contact.Contact, $"{prefix}.{nameof(contact.Contact)}");
        Require(contact.Phone, $"{prefix}.{nameof(contact.Phone)}");
        Require(contact.Email, $"{prefix}.{nameof(contact.Email)}");
    }

    private void RequireInsurance(SetupPacketInsuranceAgencyDto agency, string prefix) {
        if (agency == null || agency.HasInsurance == false) return;
        Require(agency.CompanyName, $"{prefix}.{nameof(agency.CompanyName)}");
        Require(agency.Phone, $"{prefix}.{nameof(agency.Phone)}");
    }

    #endregion [ private ]
}
