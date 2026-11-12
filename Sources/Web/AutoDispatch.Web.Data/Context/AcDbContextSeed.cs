using Microsoft.AspNetCore.Identity;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Data.Repositories;

namespace AutoDispatch.Web.Data.Context;

public class AcDbContextSeed {

    /// <summary>
    ///     Роли с Id = RoleEnum, системная компания, пользователи System и Admin (пароль — Secrets:System:Password) </summary>
    public static async Task SeedAsync(LinqToDbContext context, UserManager<AcUser> userManager, RoleManager<AcRole> roleManager) {
        foreach (var role in Enum.GetValues<RoleEnum>().Where(e => e != RoleEnum.Unknown)) {
            if (await roleManager.RoleExistsAsync(role.ToString()) == false) {
                await roleManager.CreateAsync(new AcRole { Id = (int)role, Name = role.ToString() });
            }
        }

        var companyRep = new LinqToDbRepository<Company>(context);
        if (await companyRep.GetByIdAsync(Company.SystemCompanyId) == null) {
            await companyRep.InsertAsync(new Company { Id = Company.SystemCompanyId, Name = AppSettings.ApplicationName }, AcUser.SystemUserId);
        }

        if (AppSettings.SystemPassword == null) {
            _l.w("Seed: Secrets:System:Password is missing, System and Admin users are not created");
            return;
        }
        if (await userManager.FindByIdAsync(AcUser.SystemUserId.ToString()) == null) {
            var user = AcUser.SystemUser;
            await userManager.CreateAsync(user, AppSettings.SystemPassword);
            await userManager.AddToRoleAsync(user, RoleEnum.Admin.ToString());
        }
        if (await userManager.FindByIdAsync(AcUser.AdminUserId.ToString()) == null) {
            var user = AcUser.AdminUser;
            await userManager.CreateAsync(user, AppSettings.SystemPassword);
            await userManager.AddToRoleAsync(user, RoleEnum.Admin.ToString());
        }
    }
}
