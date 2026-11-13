using Microsoft.AspNetCore.Identity;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Data.Repositories;

namespace AutoDispatch.Web.Data.Context;

public class AdDbContextSeed {

    /// <summary>
    ///     Роли с Id = RoleEnum.ToRoleId(), системная компания, пользователи System и Admin (пароль — Secrets:System:Password) </summary>
    public static async Task SeedAsync(LinqToDbContext context, UserManager<AdUser> userManager, RoleManager<AdRole> roleManager) {
        foreach (var role in Enum.GetValues<RoleEnum>().Where(e => e != RoleEnum.Unknown)) {
            if (await roleManager.RoleExistsAsync(role.ToString()) == false) {
                await roleManager.CreateAsync(new AdRole { Id = role.ToRoleId(), Name = role.ToString() });
            }
        }

        var companyRep = new LinqToDbRepository<Company>(context);
        if (await companyRep.GetByIdAsync(Company.SystemCompanyId) == null) {
            await companyRep.InsertAsync(new Company { Id = Company.SystemCompanyId, Name = AppSettings.ApplicationName }, AdUser.SystemUserId);
        }

        if (AppSettings.SystemPassword == null) {
            _l.w("Seed: Secrets:System:Password is missing, System and Admin users are not created");
            return;
        }
        if (await userManager.FindByIdAsync(AdUser.SystemUserId.ToString()) == null) {
            var user = AdUser.SystemUser;
            await userManager.CreateAsync(user, AppSettings.SystemPassword);
            await userManager.AddToRoleAsync(user, RoleEnum.Admin.ToString());
        }
        if (await userManager.FindByIdAsync(AdUser.AdminUserId.ToString()) == null) {
            var user = AdUser.AdminUser;
            await userManager.CreateAsync(user, AppSettings.SystemPassword);
            await userManager.AddToRoleAsync(user, RoleEnum.Admin.ToString());
        }
    }
}
