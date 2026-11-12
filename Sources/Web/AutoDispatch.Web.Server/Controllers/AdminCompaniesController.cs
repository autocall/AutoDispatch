using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/Companies/[action]")]
public class AdminCompaniesController : BaseController {

    private CompanyService Service => Host.GetService<CompanyService>();

    #region [ .ctor ]

    public AdminCompaniesController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("~/Admin/Companies")]
    public async Task<ActionResult> Index(bool deleted = false, string name = null) {
        return View(new CompaniesIndexViewModel {
            Deleted = deleted,
            Name = name,
            Companies = await Service.GetAllAsync(deleted, name)
        });
    }

    [HttpGet]
    public ActionResult Create() {
        return PartialView("_CompanyFormPart", new CompanyFormViewModel());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Edit(int id) {
        var dto = await Service.GetAsync(id);
        if (dto == null) return NotFound();
        return PartialView("_CompanyFormPart", Mapper.Map<CompanyFormViewModel>(dto));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Save(CompanyFormViewModel model) {
        if (ModelState.IsValid == false) {
            return PartialView("_CompanyFormPart", model);
        }
        if (model.Id == 0) {
            await Service.CreateAsync(Mapper.Map<CreateCompanyDto>(model), GetUserId());
        } else {
            await Service.UpdateAsync(Mapper.Map<UpdateCompanyDto>(model), GetUserId());
        }
        return RedirectAfterPost("/Admin/Companies");
    }

    [HttpPost("{id:int}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id) {
        await Service.DeleteAsync(id, GetUserId());
        return RedirectAfterPost("/Admin/Companies");
    }

    [HttpPost("{id:int}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Restore(int id) {
        await Service.RestoreAsync(id, GetUserId());
        return RedirectAfterPost("/Admin/Companies?deleted=true");
    }
}
