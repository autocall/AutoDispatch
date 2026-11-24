using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Custom Statuses (V1 CustomStatusController, страница «Dispatch Custom Load Statuses»): свои статусы груза компании пользователя —
///     колонка Custom Status бордов App. Admin и Owner, как V1 (меню у обоих); статусы — только своей компании (V1 GetCompanyByUserAsync),
///     сервис — перенесённый CustomStatusService. App получает список при входе (SessionDto.CustomStatuses) </summary>
[Authorize(Roles = nameof(RoleEnum.Admin) + "," + nameof(RoleEnum.Owner))]
[Route("Admin/CustomStatuses/[action]")]
public class AdminCustomStatusesController : BaseController {

    private const string IndexUrl = "/Admin/CustomStatuses";

    private CustomStatusService Service => Host.GetService<CustomStatusService>();

    #region [ .ctor ]

    public AdminCustomStatusesController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("~" + IndexUrl)]
    public async Task<ActionResult> Index() {
        return View(await Service.GetStatusesAsync(GetUserId()));
    }

    [HttpGet]
    public ActionResult Create() {
        return PartialView("_CustomStatusFormPart", new CustomStatusFormViewModel());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id) {
        var dto = await Service.GetStatusAsync(id, GetUserId());
        return PartialView("_CustomStatusFormPart", new CustomStatusFormViewModel { EntityId = dto.EntityId, Name = dto.Name });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Save(CustomStatusFormViewModel model) {
        if (ModelState.IsValid == false) {
            return PartialView("_CustomStatusFormPart", model);
        }
        var dto = await Service.SaveStatusAsync(new CompanyCustomStatusDto { EntityId = model.EntityId, Name = model.Name.Trim() }, GetUserId());
        using var scope = _l.Scope("admin");
        _l.i($"Custom status saved: {dto.Name} ({dto.EntityId}), company {dto.CompanyId} by {User.Identity?.Name}");
        return RedirectAfterPost(IndexUrl);
    }

    /// <summary>
    ///     Удаление, как V1: у грузов остаётся CustomStatusId без FK (App не находит имя — колонка пустая) </summary>
    [HttpPost("{id:guid}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(Guid id) {
        var dto = await Service.GetStatusAsync(id, GetUserId());
        await Service.DeleteStatusAsync(id, GetUserId());
        using var scope = _l.Scope("admin");
        _l.i($"Custom status deleted: {dto.Name} ({id}), company {dto.CompanyId} by {User.Identity?.Name}");
        return RedirectAfterPost(IndexUrl);
    }
}
