using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Mappings;

public class UserMap : IEntityTypeConfiguration<AdUser> {
    public void Configure(EntityTypeBuilder<AdUser> builder) {
        builder.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.NoAction);
        builder.ToTable("IdentityUsers");
    }

    public static void LinqToDbConfigure(FluentMappingBuilder builder) {
        builder.Entity<AdUser>().HasTableName("IdentityUsers");
        builder.Entity<AdRole>().HasTableName("IdentityRoles");
        builder.Entity<AdUserRole>().HasTableName("IdentityUserRoles");
    }
}
