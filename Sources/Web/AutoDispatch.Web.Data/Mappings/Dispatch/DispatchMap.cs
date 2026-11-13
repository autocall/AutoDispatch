using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Data.Mappings.Dispatch;

/// <summary>
///     Маппинг 28 сущностей dispatch (V1 Mappings/Dispatch/*Map.cs, без Message): таблицы без префикса d_ — своя БД,
///     имя таблицы = имя класса. Связи и индексы — атрибутами в сущностях ([ForeignKey], [InverseProperty], [Index]),
///     здесь — то, что атрибутами не задаётся </summary>
public static class DispatchMap {

    public static readonly Type[] EntityTypes = [
        typeof(DispatchCompany), typeof(DispatchCompanyCustom), typeof(CacheLoadIndex), typeof(CompanyCustomStatus),
        typeof(CustomerCompany), typeof(ShipperReceiverCompany), typeof(DatCompany), typeof(DatDriver), typeof(Driver),
        typeof(Defect), typeof(DefectRepair), typeof(Event), typeof(HistoryObject),
        typeof(HistoryLocationDatDriver), typeof(HistoryLocationDriver), typeof(HistoryLocationLoad),
        typeof(HistoryLocationTrailer), typeof(HistoryLocationTruck), typeof(Load),
        typeof(SetupPacket), typeof(SetupPacketContact), typeof(SetupPacketFactoringCompany),
        typeof(SetupPacketInsuranceAgency), typeof(SetupPacketReference), typeof(Trailer), typeof(Truck),
        typeof(Warehouse), typeof(Yard)
    ];

    public static void Configure(ModelBuilder modelBuilder) {
        foreach (var type in EntityTypes) {
            modelBuilder.Entity(type).ToTable(type.Name);
        }
        // несколько ссылок на один тип (Load → DatCompany ×3, Warehouse ×3, Trailer ×3 …): EF Core не подбирает пары по
        // [ForeignKey] сам — явно, как в V1 LoadMap; коллекция на другой стороне — одна, остальные без обратной навигации
        modelBuilder.Entity<Load>(load => {
            load.HasOne(e => e.Warehouse).WithMany(e => e.Loads).HasForeignKey(e => e.WarehouseId);
            load.HasOne(e => e.DestinationWarehouse).WithMany().HasForeignKey(e => e.DestinationWarehouseId);
            load.HasOne(e => e.HistoryWarehouse).WithMany().HasForeignKey(e => e.HistoryWarehouseId);
            load.HasOne(e => e.Trailer).WithMany(e => e.Loads).HasForeignKey(e => e.TrailerId);
            load.HasOne(e => e.HistoryTrailer).WithMany().HasForeignKey(e => e.HistoryTrailerId);
            load.HasOne(e => e.HistoryAssignedTrailer).WithMany().HasForeignKey(e => e.HistoryAssignedTrailerId);
            load.HasOne(e => e.DatCompany).WithMany(e => e.Loads).HasForeignKey(e => e.DatCompanyId);
            load.HasOne(e => e.HistoryDatCompany).WithMany().HasForeignKey(e => e.HistoryDatCompanyId);
            load.HasOne(e => e.DatBroker).WithMany().HasForeignKey(e => e.DatBrokerId);
            load.HasOne(e => e.DatDriver).WithMany(e => e.Loads).HasForeignKey(e => e.DatDriverId);
            load.HasOne(e => e.HistoryDatDriver).WithMany().HasForeignKey(e => e.HistoryDatDriverId);
            load.HasOne(e => e.HistoryDriver).WithMany(e => e.Histories).HasForeignKey(e => e.HistoryDriverId);
            load.HasOne(e => e.HistoryTruck).WithMany().HasForeignKey(e => e.HistoryTruckId);
            load.HasOne(e => e.ReceiverCompany1).WithMany().HasForeignKey(e => e.ReceiverId);
            load.HasOne(e => e.ShipperCompany1).WithMany().HasForeignKey(e => e.ShipperId);
            load.HasOne(e => e.Parent).WithMany(e => e.Children1).HasForeignKey(e => e.ParentId);
            load.HasOne(e => e.Shipment).WithMany(e => e.Children2).HasForeignKey(e => e.ShipmentId);
        });
        modelBuilder.Entity<Driver>().HasOne(e => e.Load).WithMany().HasForeignKey(e => e.CurrentLoadId);
        modelBuilder.Entity<HistoryLocationLoad>(h => {
            h.HasOne(e => e.HistoryDatCompany).WithMany().HasForeignKey(e => e.HistoryDatCompanyId);
            h.HasOne(e => e.HistoryWarehouse).WithMany().HasForeignKey(e => e.HistoryWarehouseId);
        });
        modelBuilder.Entity<SetupPacket>(s => {
            s.HasOne(e => e.ConfidantContact).WithMany().HasForeignKey(e => e.ConfidantContactId);
            s.HasOne(e => e.CorporateContact).WithMany().HasForeignKey(e => e.CorporateContactId);
            s.HasOne(e => e.SalesContact).WithMany().HasForeignKey(e => e.SalesContactId);
            s.HasOne(e => e.DispatchContact).WithMany().HasForeignKey(e => e.DispatchContactId);
            s.HasOne(e => e.AfterHourContact).WithMany().HasForeignKey(e => e.AfterHourContactId);
            s.HasOne(e => e.GeneralLiabilityInsuranceAgency).WithMany().HasForeignKey(e => e.GeneralLiabilityInsuranceAgencyId);
            s.HasOne(e => e.AutoInsuranceAgency).WithMany().HasForeignKey(e => e.AutoInsuranceAgencyId);
            s.HasOne(e => e.CargoInsuranceAgency).WithMany().HasForeignKey(e => e.CargoInsuranceAgencyId);
            s.HasOne(e => e.WorkersCompensationInsuranceAgency).WithMany().HasForeignKey(e => e.WorkersCompensationInsuranceAgencyId);
        });

        // V1: DatabaseGenerated(Computed) + default getdate() в БД
        modelBuilder.Entity<CacheLoadIndex>().Property(e => e.CreatedDate).HasDefaultValueSql("getdate()");

        // строки — varchar: в данных только латиница, БД autodispatch в UTF-8 collation (решено 2026-11-13, AI/wiki/plan.md)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => EntityTypes.Contains(e.ClrType))) {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(string))) {
                property.SetIsUnicode(false);
            }
        }

        // V1 (AutoCallDbContext): OneToManyCascadeDeleteConvention снята — все FK без каскада,
        // иначе SQL Server отказывает из-за нескольких путей каскада (Load → Warehouse/Trailer/… → Company)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => EntityTypes.Contains(e.ClrType))) {
            foreach (var fk in entityType.GetForeignKeys()) {
                fk.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }

    public static void LinqToDbConfigure(FluentMappingBuilder builder) {
        builder.Entity<DispatchCompany>().HasTableName("DispatchCompany").HasPrimaryKey(e => e.EntityId);
        builder.Entity<DispatchCompanyCustom>().HasTableName("DispatchCompanyCustom").HasPrimaryKey(e => e.EntityId);
        builder.Entity<CacheLoadIndex>().HasTableName("CacheLoadIndex").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id)
            .Property(e => e.CreatedDate).HasSkipOnInsert();
        builder.Entity<CompanyCustomStatus>().HasTableName("CompanyCustomStatus").HasPrimaryKey(e => e.EntityId);
        builder.Entity<CustomerCompany>().HasTableName("CustomerCompany").HasPrimaryKey(e => e.EntityId);
        builder.Entity<ShipperReceiverCompany>().HasTableName("ShipperReceiverCompany").HasPrimaryKey(e => e.EntityId);
        builder.Entity<DatCompany>().HasTableName("DatCompany").HasPrimaryKey(e => e.EntityId);
        builder.Entity<DatDriver>().HasTableName("DatDriver").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Driver>().HasTableName("Driver").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Defect>().HasTableName("Defect").HasPrimaryKey(e => e.EntityId);
        builder.Entity<DefectRepair>().HasTableName("DefectRepair").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Event>().HasTableName("Event").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<HistoryObject>().HasTableName("HistoryObject").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<HistoryLocationDatDriver>().HasTableName("HistoryLocationDatDriver").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<HistoryLocationDriver>().HasTableName("HistoryLocationDriver").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<HistoryLocationLoad>().HasTableName("HistoryLocationLoad").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<HistoryLocationTrailer>().HasTableName("HistoryLocationTrailer").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<HistoryLocationTruck>().HasTableName("HistoryLocationTruck").HasIdentity(e => e.Id).HasPrimaryKey(e => e.Id);
        builder.Entity<Load>().HasTableName("Load").HasPrimaryKey(e => e.EntityId);
        builder.Entity<SetupPacket>().HasTableName("SetupPacket").HasPrimaryKey(e => e.EntityId);
        builder.Entity<SetupPacketContact>().HasTableName("SetupPacketContact").HasPrimaryKey(e => e.EntityId);
        builder.Entity<SetupPacketFactoringCompany>().HasTableName("SetupPacketFactoringCompany").HasPrimaryKey(e => e.EntityId);
        builder.Entity<SetupPacketInsuranceAgency>().HasTableName("SetupPacketInsuranceAgency").HasPrimaryKey(e => e.EntityId);
        builder.Entity<SetupPacketReference>().HasTableName("SetupPacketReference").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Trailer>().HasTableName("Trailer").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Truck>().HasTableName("Truck").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Warehouse>().HasTableName("Warehouse").HasPrimaryKey(e => e.EntityId);
        builder.Entity<Yard>().HasTableName("Yard").HasPrimaryKey(e => e.EntityId);
    }
}
