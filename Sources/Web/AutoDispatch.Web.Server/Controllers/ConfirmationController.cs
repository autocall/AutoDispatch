using AutoCall.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Extensions;
using AutoDispatch.Web.Server.Models;
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Подтверждение груза карриером по ссылке из письма (V1 ConfirmationController): /Confirmation/?LoadId= — rate confirmation
///     и форма с именем, POST — подпись и письмо (DispatchService.ConfirmedSend от System), Download/{loadId} — подписанный PDF.
///     Без логина — пропуск только Guid груза; URL как в V1, ссылки уже в разосланных письмах </summary>
[AllowAnonymous]
[Route("Confirmation")]
public class ConfirmationController : BaseController {

    private ConfirmationService Service => Host.GetService<ConfirmationService>();
    private DatCompanyService DatCompanies => Host.GetService<DatCompanyService>();
    private DispatchService Dispatch => Host.GetService<DispatchService>();

    public ConfirmationController(ServicesHost host) : base(host) { }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
        using var scope = _l.Scope("confirmation");
        await base.OnActionExecutionAsync(context, next);
    }

    [HttpGet("~/Confirmation")]
    public async Task<ActionResult> Index(Guid? loadId) {
        if (loadId == null) {
            // скорей всего это какой-то робот (V1)
            return JsonFail(System.Net.HttpStatusCode.BadRequest, "LoadId is required");
        }
        var model = await LoadAsync(loadId.Value);
        if (model == null) {
            return View("Error", new ConfirmationViewModel());
        }
        _l.i($"Confirmation opened: load {model.Load.IndexString} ({loadId}), {(model.Load.ConfirmedDate != null ? "signed" : "not signed")}, {Request.GetIp()}");
        return View(model.Load.ConfirmedDate != null ? "Confirmed" : "Index", model);
    }

    /// <summary>
    ///     Подпись: имя 4–24 символа; успех — та же страница (уже Confirmed), ошибка — фрагмент формы с текстом </summary>
    [HttpPost("~/Confirmation"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Index(ConfirmationViewModel form) {
        var model = await LoadAsync(form.LoadId);
        if (model == null) {
            return View("Error", new ConfirmationViewModel());
        }
        model.Name = form.Name;
        if (ModelState.IsValid == false || (model.Name ?? "").Trim().Length < 4) {
            model.Error = "The Name must be at least 4 characters long.";
            return FormResult(model);
        }
        if (model.Load.ConfirmedDate != null) {
            model.Error = "Email already sent";
            return FormResult(model);
        }
        try {
            await Dispatch.ConfirmedSend(new ConfirmedSendRequestData(AdUser.SystemUserId) {
                LoadId = model.LoadId,
                Signature = model.Name,
                Emails = model.Load.EmailConfirmationSent
            });
            _l.i($"Confirmation signed: load {model.Load.IndexString} ({model.LoadId}) by '{model.Name}', {Request.GetIp()}");
        } catch (InfoException e) {
            model.Error = e.Message;
            return FormResult(model);
        } catch (Exception e) {
            _l.e($"Confirmation sign failed: load {model.LoadId}", e);
            model.Error = e.Message;
            return FormResult(model);
        }
        return RedirectAfterPost($"/Confirmation/?LoadId={model.LoadId}");
    }

    private ActionResult FormResult(ConfirmationViewModel model) {
        return Request.IsHtmx() ? PartialView("_SignFormPart", model) : View("Index", model);
    }

    /// <summary>
    ///     Подписанный PDF из каталога груза (V1 FilePath/{loadId}/{ConfirmedFile}) </summary>
    [HttpGet("Download/{loadId:guid}")]
    public async Task<ActionResult> Download(Guid loadId) {
        var load = await Service.GetLoadByIdAsync(loadId);
        if (load == null) {
            _l.e($"Load '{loadId}' was not found");
            return View("Error", new ConfirmationViewModel());
        }
        // аудит D22: как страница подтверждения (LoadAsync) — удалённый груз не отдаётся
        if (load.IsDeleted) {
            _l.e($"Load '{loadId}' is deleted");
            return View("Error", new ConfirmationViewModel());
        }
        if (load.ConfirmedFile == null) {
            _l.e($"Load '{loadId}' is not confirmed");
            return View("Error", new ConfirmationViewModel());
        }
        var filePath = DispatchFileHelper.FileGetFullPath(load.EntityId, load.ConfirmedFile);
        if (System.IO.File.Exists(filePath) == false) {
            _l.e($"Load '{loadId}': confirmed file not found: {filePath}");
            return View("Error", new ConfirmationViewModel());
        }
        return File(System.IO.File.OpenRead(filePath), "application/pdf", load.ConfirmedFile);
    }

    /// <summary>
    ///     Груз, карриер (с полями Safer — телефон), компания; null — что-то не найдено, в лог как в V1 </summary>
    private async Task<ConfirmationViewModel> LoadAsync(Guid loadId) {
        var load = await Service.GetLoadByIdAsync(loadId);
        if (load == null) {
            _l.e($"Load '{loadId}' was not found");
            return null;
        }
        if (load.IsDeleted) {
            _l.e($"Load '{loadId}' is deleted");
            return null;
        }
        if (load.DatCompanyId == null) {
            _l.w($"Load {load.IndexString}:{load.EntityId}. DatCompanyId is NULL");
            return null;
        }
        if (load.ActualCompanyId == null) {
            using (_l.Property(AppSettings.LogDetailsProperty, JsonExtension.Serialize(load))) {
                _l.w($"Load {load.IndexString}:{load.EntityId}. ActualCompanyId is NULL");
            }
            return null;
        }
        var carrier = await Service.GetDatCompanyByIdAsync(load.DatCompanyId.Value);
        var company = await Service.GetActualCompanyByIdAsync(load.ActualCompanyId.Value);
        if (carrier == null || company == null) {
            _l.e($"Load {load.IndexString}:{load.EntityId}. Carrier or company was not found");
            return null;
        }
        // телефон карриера из Safer — необязателен: карриер не найден или Safer недоступен — страница без него (V1 падала бы)
        try {
            carrier.Fields = await DatCompanies.GetFieldsAsync(carrier.DotNumber, carrier.McNumber) ?? new Dictionary<string, string>();
        } catch (Exception e) {
            _l.w($"Carrier fields for {carrier.Name} (MC {carrier.McNumber}, DOT {carrier.DotNumber}): {e.Message}");
            carrier.Fields = new Dictionary<string, string>();
        }

        var rate = DispatchHelper.GetRate(load);
        var model = new ConfirmationViewModel {
            LoadId = load.EntityId,
            Load = load,
            Carrier = carrier,
            Company = company,
            Rate = rate,
            Currency = ((CurrencyType)load.Currency).GetDescription(),
            Equipment = AutoCall.Common.Extensions.EnumExtensions.GetDescriptionOrTostring((AutoCallEquipmentType)load.Equipment),
            Weight = $"{load.Weight} {((WeightType)load.WeightType).GetDescription()}",
            Length = load.Length.NullableLengthToString(),
            PickupAppointment = DispatchStringExtension.DateTimeToAppointmentFullDateTime(load.PickupFrom.Date, load.PickupFrom.TimeOfDay, load.PickupTo.TimeOfDay),
            DeliveryAppointment = DispatchStringExtension.DateTimeToAppointmentFullDateTime(load.DeliveryFrom.Date, load.DeliveryFrom.TimeOfDay, load.DeliveryTo.TimeOfDay),
            CarrierPhone = carrier.Fields.TryGetValue("Phone:", out var phone) ? phone : null
        };
        if (load.LineHaul != null && rate != load.LineHaul) {
            model.Gains = ExpensesConverter.ToGains(load.Gains).Where(g => g.Cost != null).ToList();
        }
        return model;
    }
}
