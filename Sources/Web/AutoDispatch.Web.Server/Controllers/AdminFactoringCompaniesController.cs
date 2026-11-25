using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Factoring Companies (V1 FactoringCompanyController, страница FactoringCompany/Index в меню Admin): справочник, из которого карриер
///     выбирает Pay To на шаге 2 setup packet. Только Admin (решено 2026-11-24): справочник общий для всех компаний; в V1 контроллер
///     был без авторизации — пункт меню видел только Admin, но править и удалять мог любой со ссылкой </summary>
[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/FactoringCompanies/[action]")]
public class AdminFactoringCompaniesController : BaseController {

    private const string IndexUrl = "/Admin/FactoringCompanies";

    /// <summary>
    ///     Колонки сортировки — номера колонок V1 LookupAsync (DataTables order[0][column]) </summary>
    private static readonly string[] SortColumns = { "name", "address", "city", "state", "zip", "email" };

    private FactoringCompanyService Service => Host.GetService<FactoringCompanyService>();

    #region [ .ctor ]

    public AdminFactoringCompaniesController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("~" + IndexUrl)]
    public async Task<ActionResult> Index(string search = null, string sort = "name", bool desc = false) {
        return View(new FactoringCompaniesIndexViewModel {
            Search = search,
            Sort = sort,
            Desc = desc,
            // V1 LookupAsync: поиск по всем колонкам и штату, сортировка по номеру колонки; pageSize 0 — все строки (≈190, без страниц)
            Companies = (await Service.LookupAsync(0, 0, String.IsNullOrWhiteSpace(search) ? null : search.Trim(), desc ? "desc" : "asc",
                Math.Max(Array.IndexOf(SortColumns, sort), 0))).Item1
        });
    }

    [HttpGet]
    public ActionResult Create() {
        return FormView(new FactoringCompanyFormViewModel());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Edit(int id) {
        var dto = await Service.GetByIdAsync(id);
        if (dto == null) return NotFound();
        return FormView(new FactoringCompanyFormViewModel {
            EntityId = dto.EntityId,
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            StateId = dto.StateId,
            ZipCode = dto.ZipCode,
            Email = dto.Email,
            CountryId = dto.CountryId
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Save(FactoringCompanyFormViewModel model) {
        if (ModelState.IsValid == false) {
            return FormView(model);
        }
        await Service.SaveAsync(new SetupPacketFactoringCompanyDto {
            EntityId = (short)model.EntityId,
            Name = model.Name.Trim(),
            Address = model.Address.Trim(),
            City = model.City.Trim(),
            StateId = model.StateId.Value,
            ZipCode = model.ZipCode?.Trim(),
            Email = model.Email?.Trim(),
            CountryId = model.CountryId
        });
        using var scope = _l.Scope("admin");
        _l.i($"Factoring company saved: {model.Name} ({model.EntityId}) by {User.Identity?.Name}");
        return RedirectAfterPost(IndexUrl);
    }

    [HttpPost("{id:int}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id) {
        var dto = await Service.GetByIdAsync(id);
        await Service.DeleteAsync(id);
        using var scope = _l.Scope("admin");
        _l.i($"Factoring company deleted: {dto?.Name} ({id}) by {User.Identity?.Name}");
        return RedirectAfterPost(IndexUrl);
    }

    private PartialViewResult FormView(FactoringCompanyFormViewModel model) {
        ViewData["States"] = StateHelper.States.Where(e => e.Id != StateHelper.Unknown).OrderBy(e => e.Name).ToList();
        return PartialView("_FactoringCompanyFormPart", model);
    }
}
