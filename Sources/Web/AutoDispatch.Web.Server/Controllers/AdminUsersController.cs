using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.Identity;
using AutoDispatch.Web.Logic.Helpers;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/Users/[action]")]
public class AdminUsersController : BaseController {

    private UserService Service => Host.GetService<UserService>();
    private CompanyService CompanyService => Host.GetService<CompanyService>();

    #region [ .ctor ]

    public AdminUsersController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("~/Admin/Users")]
    public async Task<ActionResult> Index(UserFilterDto filter) {
        return View(new UsersIndexViewModel {
            Filter = filter,
            Companies = await CompanyService.GetAllAsync(false),
            Users = await Service.GetAllAsync(filter)
        });
    }

    [HttpGet]
    public async Task<ActionResult> Create(int? companyId = null) {
        var model = new UserFormViewModel { CompanyId = companyId ?? 0, TimeZoneId = TimeZoneHelper.UtcId };
        return PartialView("_UserFormPart", await FillAsync(model));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Edit(int id) {
        var dto = await Service.GetAsync(id);
        if (dto == null) return NotFound();
        return PartialView("_UserFormPart", await FillAsync(Mapper.Map<UserFormViewModel>(dto)));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Save(UserFormViewModel model) {
        if (model.Id == 0 && String.IsNullOrEmpty(model.Password)) {
            ModelState.AddModelError(nameof(model.Password), "Password is required for a new user");
        }
        if (model.Role == RoleEnum.Unknown) {
            ModelState.AddModelError(nameof(model.Role), "Role is required");
        }
        if (ModelState.IsValid == false) {
            return PartialView("_UserFormPart", await FillAsync(model));
        }
        using var scope = _l.Scope("admin");
        if (model.Id == 0) {
            var dto = await Service.CreateAsync(Mapper.Map<CreateUserDto>(model), GetUserId());
            _l.i($"User created: {dto.Email} ({dto.Id}) by {User.Identity?.Name}");
        } else {
            var dto = await Service.UpdateAsync(Mapper.Map<UpdateUserDto>(model), GetUserId());
            _l.i($"User updated: {dto.Email} ({dto.Id}) by {User.Identity?.Name}");
        }
        return RedirectAfterPost("/Admin/Users");
    }

    [HttpPost("{id:int}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id) {
        using var scope = _l.Scope("admin");
        await Service.DeleteAsync(id, GetUserId());
        _l.i($"User deleted: {id} by {User.Identity?.Name}");
        return RedirectAfterPost("/Admin/Users");
    }

    [HttpPost("{id:int}"), ValidateAntiForgeryToken]
    public async Task<ActionResult> Restore(int id) {
        using var scope = _l.Scope("admin");
        await Service.RestoreAsync(id, GetUserId());
        _l.i($"User restored: {id} by {User.Identity?.Name}");
        return RedirectAfterPost("/Admin/Users?deleted=true");
    }

    #region [ private ]

    private async Task<UserFormViewModel> FillAsync(UserFormViewModel model) {
        model.Companies = await CompanyService.GetAllAsync(false);
        model.TimeZones = TimeZoneHelper.GetIanaIds();
        model.Password = null;
        return model;
    }

    #endregion [ private ]
}
