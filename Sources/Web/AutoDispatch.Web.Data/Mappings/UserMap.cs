using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Mappings;

public class UserMap : IEntityTypeConfiguration<AcUser> {
    public void Configure(EntityTypeBuilder<AcUser> builder) {
        builder.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.NoAction);
        builder.ToTable("IdentityUsers");
    }

    public static void LinqToDbConfigure(FluentMappingBuilder builder) {
        builder.Entity<AcUser>().HasTableName("IdentityUsers");
        builder.Entity<AcRole>().HasTableName("IdentityRoles");
        builder.Entity<AcUserRole>().HasTableName("IdentityUserRoles");
    }
}
