using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Mappings;
using AutoDispatch.Web.Data.Mappings.Dispatch;

namespace AutoDispatch.Web.Data.Context;

/// <summary>
///     EF Core: Identity и миграции. Запросы и записи бизнес-данных — через LinqToDbContext </summary>
public class AdDbContext : IdentityDbContext<AdUser, AdRole, Guid, AdUserClaim, AdUserRole, AdUserLogin, AdRoleClaim, AdUserToken> {

    public AdDbContext(DbContextOptions<AdDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        // Identity
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.Entity<AdRole>().ToTable("IdentityRoles");
        modelBuilder.Entity<AdUserRole>().ToTable("IdentityUserRoles");
        modelBuilder.Entity<AdRoleClaim>().ToTable("IdentityRoleClaim");
        modelBuilder.Entity<AdUserLogin>().ToTable("IdentityUserLogins");
        modelBuilder.Entity<AdUserClaim>().ToTable("IdentityUserClaims");
        modelBuilder.Entity<AdUserToken>().ToTable("IdentityUserToken");

        modelBuilder.ApplyConfiguration(new CompanyMap());

        // настройки карты пользователя (секция Map)
        modelBuilder.ApplyConfiguration(new UserSettingMap());

        // 28 сущностей dispatch (этап 2)
        DispatchMap.Configure(modelBuilder);
    }
}
