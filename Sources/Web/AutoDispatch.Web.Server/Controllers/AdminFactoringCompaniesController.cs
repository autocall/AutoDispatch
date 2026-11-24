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
///     Factoring Companies (V1 FactoringCompanyController, ÑÑ‚Ñ€Ð°Ð½Ð¸Ñ†Ð° FactoringCompany/Index Ð² Ð¼ÐµÐ½ÑŽ Admin): ÑÐ¿Ñ€Ð°Ð²Ð¾Ñ‡Ð½Ð¸Ðº, Ð¸Ð· ÐºÐ¾Ñ‚Ð¾Ñ€Ð¾Ð³Ð¾ ÐºÐ°Ñ€Ñ€Ð¸ÐµÑ€
///     Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ Pay To Ð½Ð° ÑˆÐ°Ð³Ðµ 2 setup packet. Ð¢Ð¾Ð»ÑŒÐºÐ¾ Admin (Ñ€ÐµÑˆÐµÐ½Ð¾ 2026-11-24): ÑÐ¿Ñ€Ð°Ð²Ð¾Ñ‡Ð½Ð¸Ðº Ð¾Ð±Ñ‰Ð¸Ð¹ Ð´Ð»Ñ Ð²ÑÐµÑ… ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¹; Ð² V1 ÐºÐ¾Ð½Ñ‚Ñ€Ð¾Ð»Ð»ÐµÑ€
///     Ð±Ñ‹Ð» Ð±ÐµÐ· Ð°Ð²Ñ‚Ð¾Ñ€Ð¸Ð·Ð°Ñ†Ð¸Ð¸ â€” Ð¿ÑƒÐ½ÐºÑ‚ Ð¼ÐµÐ½ÑŽ Ð²Ð¸Ð´ÐµÐ» Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Admin, Ð½Ð¾ Ð¿Ñ€Ð°Ð²Ð¸Ñ‚ÑŒ Ð¸ ÑƒÐ´Ð°Ð»ÑÑ‚ÑŒ Ð¼Ð¾Ð³ Ð»ÑŽÐ±Ð¾Ð¹ ÑÐ¾ ÑÑÑ‹Ð»ÐºÐ¾Ð¹ </summary>
[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/FactoringCompanies/[action]")]
public class AdminFactoringCompaniesController : BaseController {

    private const string IndexUrl = "/Admin/FactoringCompanies";

    /// <summary>
    ///     ÐšÐ¾Ð»Ð¾Ð½ÐºÐ¸ ÑÐ¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²ÐºÐ¸ â€” Ð½Ð¾Ð¼ÐµÑ€Ð° ÐºÐ¾Ð»Ð¾Ð½Ð¾Ðº V1 LookupAsync (DataTables order[0][column]) </summary>
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
            // V1 LookupAsync: Ð¿Ð¾Ð¸ÑÐº Ð¿Ð¾ Ð²ÑÐµÐ¼ ÐºÐ¾Ð»Ð¾Ð½ÐºÐ°Ð¼ Ð¸ ÑˆÑ‚Ð°Ñ‚Ñƒ, ÑÐ¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²ÐºÐ° Ð¿Ð¾ Ð½Ð¾Ð¼ÐµÑ€Ñƒ ÐºÐ¾Ð»Ð¾Ð½ÐºÐ¸; pageSize 0 â€” Ð²ÑÐµ ÑÑ‚Ñ€Ð¾ÐºÐ¸ (â‰ˆ190, Ð±ÐµÐ· ÑÑ‚Ñ€Ð°Ð½Ð¸Ñ†)
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
