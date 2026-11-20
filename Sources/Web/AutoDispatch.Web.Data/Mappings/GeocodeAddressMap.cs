using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Mappings;

public class GeocodeAddressMap : IEntityTypeConfiguration<GeocodeAddress> {
    public void Configure(EntityTypeBuilder<GeocodeAddress> builder) {
        builder.ToTable("GeocodeAddress");
        builder.HasKey(e => new { e.Lat, e.Lng });
        builder.Property(e => e.Address).IsRequired();
    }

    public static void LinqToDbConfigure(FluentMappingBuilder builder) {
        builder.Entity<GeocodeAddress>().HasTableName("GeocodeAddress").HasPrimaryKey(e => new { e.Lat, e.Lng });
    }
}
