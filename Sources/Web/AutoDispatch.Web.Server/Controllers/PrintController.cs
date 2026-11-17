using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Печатные страницы setup packet (V1 PrintController, URL как в V1): договор и insurance instructions без навигации,
///     кнопка Print → window.print(). Без логина, пропуск — Guid карриера, как у CarrierRegistration </summary>
[AllowAnonymous]
[Route("Print/SetupPacket")]
public class PrintController : BaseController {

    private DatCompanyService Carriers => Host.GetService<DatCompanyService>();
    private DispatchCompanyService Companies => Host.GetService<DispatchCompanyService>();
    private SetupPacketService Packets => Host.GetService<SetupPacketService>();

    public PrintController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     version 2 — Agreement_02 (единственная с подписями в тексте); версия 1 не переносится (AI/wiki/plan.md) </summary>
    [HttpGet("Agreement")]
    public async Task<ActionResult> Agreement(Guid carrierId, int version = 1) {
        var model = await LoadAsync(carrierId);
        if (model?.SetupPacket == null) {
            return View("~/Views/CarrierRegistration/Error.cshtml", new SetupPacketViewModel());
        }
        return View("Agreement", model);
    }

    [HttpGet("InsuranceInstructions")]
    public async Task<ActionResult> InsuranceInstructions(Guid carrierId) {
        var model = await LoadAsync(carrierId);
        if (model == null) {
            return View("~/Views/CarrierRegistration/Error.cshtml", new SetupPacketViewModel());
        }
        return View("InsuranceInstructions", model);
    }

    private async Task<SetupPacketViewModel> LoadAsync(Guid carrierId) {
        var carrier = await Carriers.GetByIdAsync(carrierId);
        var company = carrier == null ? null : await Companies.GetByIdAsync(carrier.ActualCompanyId);
        if (carrier == null || company == null) {
            _l.w($"Print: carrier '{carrierId}' or its company was not found");
            return null;
        }
        var model = new SetupPacketViewModel {
            Mode = PageModeEnum.View,
            Carrier = carrier,
            DatCompanyId = carrier.EntityId,
            Company = company,
            States = StateHelper.GetStateDtos()
        };
        if (carrier.SetupPacketId != null) {
            model.SetupPacket = await Packets.GetByIdAsync(carrier.SetupPacketId.Value);
        }
        return model;
    }
}
