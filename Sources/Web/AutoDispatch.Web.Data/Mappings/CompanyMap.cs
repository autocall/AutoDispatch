using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Mappings;

public class CompanyMap : BaseMap<Company> {
    public override void Configure(EntityTypeBuilder<Company> builder) {
        // без базовых FK на CreatedBy/ModifiedBy — см. Company
        builder.ToTable("Company");
    }

    public static void LinqToDbConfigure(FluentMappingBuilder builder) {
        builder.Entity<Company>()
            .HasTableName("Company")
            .Ignore(e => e.CreatedBy)
            .Ignore(e => e.ModifiedBy);
    }
}
