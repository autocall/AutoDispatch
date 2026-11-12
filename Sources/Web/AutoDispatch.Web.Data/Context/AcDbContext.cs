using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Mappings;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     EF Core: Identity и миграции. Запросы и записи бизнес-данных — через LinqToDbContext </summary>
public class AcDbContext : IdentityDbContext<AcUser, AcRole, int, AcUserClaim, AcUserRole, AcUserLogin, AcRoleClaim, AcUserToken> {

    public AcDbContext(DbContextOptions<AcDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Identity
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.Entity<AcRole>().ToTable("IdentityRoles");
        modelBuilder.Entity<AcUserRole>().ToTable("IdentityUserRoles");
        modelBuilder.Entity<AcRoleClaim>().ToTable("IdentityRoleClaim");
        modelBuilder.Entity<AcUserLogin>().ToTable("IdentityUserLogins");
        modelBuilder.Entity<AcUserClaim>().ToTable("IdentityUserClaims");
        modelBuilder.Entity<AcUserToken>().ToTable("IdentityUserToken");

        modelBuilder.ApplyConfiguration(new CompanyMap());

        // настройки карты пользователя (секция Map)
        modelBuilder.ApplyConfiguration(new UserSettingMap());
    }
}
