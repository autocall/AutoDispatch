using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Чистка истории раз в сутки (V1 QuartzService.DoAsync; таблицы без префикса d_): события и положения удалённых грузов,
///     снимки HistoryObject старше HistoryExpiresDays, на которые не ссылаются живые объекты и грузы не в статусе Paid </summary>
public class HistoryCleanupService : DispatchBaseService {

    public HistoryCleanupService(ServicesHost host) : base(host) { }

    public async Task DoAsync() {
        var historyExpires = AppSettings.HistoryExpiresDays;
        var query = $@"
-- deletes [Event] of deleted loads
DELETE FROM [dbo].[Event] WHERE [HistoryId] IN (SELECT [Id] FROM [dbo].[HistoryLocationLoad] WHERE [LoadId] IN (SELECT [EntityId] FROM [dbo].[Load] WHERE [IsDeleted] = 1))
-- deletes [HistoryLocationLoad] of deleted loads
DELETE FROM [dbo].[HistoryLocationLoad] WHERE [LoadId] IN (SELECT [EntityId] FROM [dbo].[Load] WHERE [IsDeleted] = 1)

DELETE FROM [dbo].[HistoryObject] WHERE DATEDIFF(DAY, [CreatedDate], GETDATE()) > {historyExpires} AND
    -- Objects
    [Id] NOT IN (SELECT HistoryId FROM dbo.Driver) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.DatDriver) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.DatCompany) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.Trailer) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.Truck) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.Warehouse) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.SetupPacket) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.Load) AND
    -- Skips actual loads
    [ReferenceId] NOT IN (SELECT EntityId FROM dbo.Load WHERE Status <> {(int)LoadStatus.Completed} /*paid*/ AND [IsDeleted] = 0) AND
    -- HistoryLocation..
    [Id] NOT IN (SELECT HistoryId FROM dbo.HistoryLocationLoad) AND
    [Id] NOT IN (SELECT HistoryWarehouseId FROM dbo.HistoryLocationLoad WHERE HistoryWarehouseId IS NOT NULL) AND
    [Id] NOT IN (SELECT HistoryDatCompanyId FROM dbo.HistoryLocationLoad WHERE HistoryDatCompanyId IS NOT NULL) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.HistoryLocationTruck) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.HistoryLocationTrailer) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.HistoryLocationDriver) AND
    [Id] NOT IN (SELECT HistoryId FROM dbo.HistoryLocationDatDriver)

DELETE FROM [dbo].[Event] WHERE [HistoryId] IS NOT NULL AND [HistoryId] NOT IN (SELECT [Id] FROM [dbo].[HistoryObject])
";
        var timeout = Db.CommandTimeout;
        try {
            Db.CommandTimeout = (int)TimeSpan.FromMinutes(30).TotalSeconds;
            await Db.ExecuteAsync(query);
            _l.i("HistoryCleanupService.DoAsync Successed");
        } finally {
            Db.CommandTimeout = timeout;
        }
    }
}

/// <summary>
///     Индекс «город + компания» для подсказок при вводе груза, таблица CacheLoadIndex (V1 CacheService.SaveLoadIndex / FixLoadIndexCacheAsync) </summary>
public class LoadIndexCacheService : DispatchBaseService {

    public LoadIndexCacheService(ServicesHost host) : base(host) { }

    private static string Cut(string value) => value.Length <= 16 ? value : value.Substring(0, 16);

    public async Task SaveLoadIndexAsync(Guid companyId, Dtos.Dispatch.LoadIndexSource dto) {
        try {
            if (String.IsNullOrEmpty(dto.OriginCity) == false && String.IsNullOrEmpty(dto.ShipperCompany) == false) {
                await InsertIfMissingAsync(companyId, dto.LoadId, Cut(dto.OriginCity), Cut(dto.ShipperCompany), (byte)CacheLoadIndexType.Origin);
            }
            if (String.IsNullOrEmpty(dto.DestinationCity) == false && String.IsNullOrEmpty(dto.ReceiverCompany) == false) {
                await InsertIfMissingAsync(companyId, dto.LoadId, Cut(dto.DestinationCity), Cut(dto.ReceiverCompany), (byte)CacheLoadIndexType.Destination);
            }
        } catch (Exception e) {
            _l.e("SaveIndex", e);
        }
    }

    private async Task InsertIfMissingAsync(Guid companyId, Guid loadId, string city, string company, byte type) {
        // V1: WHERE NOT EXISTS по City + Company без компании и типа — сохранено
        var exists = await Table<CacheLoadIndex>().AnyAsync(e => e.City == city && e.Company == company);
        if (exists == false) {
            await InsertAsync(new CacheLoadIndex { CompanyId = companyId, LoadId = loadId, City = city, Company = company, Type = type });
        }
    }

    /// <summary>
    ///     Добивает индекс по грузам за days дней (null — по всем), как CacheLoadJob V1 раз в сутки </summary>
    public async Task<string> FixLoadIndexCacheAsync(int? days = null) {
        int added = 0;
        var since = days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : (DateTime?)null;
        var origins = await (
            from l in Table<Load>()
            join c in Table<ShipperReceiverCompany>() on l.ShipperId equals c.EntityId
            where l.IsDeleted == false && l.OriginCity != null && c.Name != null && (since == null || l.LastModifiedDate >= since)
            group l by new { l.CompanyId, City = Sql.Substring(l.OriginCity, 1, 16), Company = Sql.Substring(c.Name, 1, 16) } into g
            select new { g.Key.CompanyId, g.Key.City, g.Key.Company, EntityId = g.Max(e => e.EntityId), Type = (byte)CacheLoadIndexType.Origin }
        ).ToListAsync();
        var destinations = await (
            from l in Table<Load>()
            join c in Table<ShipperReceiverCompany>() on l.ReceiverId equals c.EntityId
            where l.IsDeleted == false && l.DestinationCity != null && c.Name != null && (since == null || l.LastModifiedDate >= since)
            group l by new { l.CompanyId, City = Sql.Substring(l.DestinationCity, 1, 16), Company = Sql.Substring(c.Name, 1, 16) } into g
            select new { g.Key.CompanyId, g.Key.City, g.Key.Company, EntityId = g.Max(e => e.EntityId), Type = (byte)CacheLoadIndexType.Destination }
        ).ToListAsync();

        foreach (var load in origins.Concat(destinations)) {
            var exists = await Table<CacheLoadIndex>().AnyAsync(c => c.CompanyId == load.CompanyId && c.City == load.City && c.Company == load.Company && c.Type == load.Type);
            if (exists == false) {
                await InsertAsync(new CacheLoadIndex { LoadId = load.EntityId, CompanyId = load.CompanyId, City = load.City, Company = load.Company, Type = load.Type });
                added++;
            }
        }
        var result = $"CacheLoadIndex: added {added}";
        _l.i(result);
        return result;
    }
}
