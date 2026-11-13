using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Mappings;

public class UserSettingMap : IEntityTypeConfiguration<UserSetting> {
    public void Configure(EntityTypeBuilder<UserSetting> builder) {
        builder.ToTable("UserSetting");
        builder.HasKey(e => new { e.UserId, e.Section });
        builder.HasOne<AdUser>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
    }

    public static void LinqToDbConfigure(FluentMappingBuilder builder) {
        builder.Entity<UserSetting>().HasTableName("UserSetting").HasPrimaryKey(e => new { e.UserId, e.Section });
    }
}
