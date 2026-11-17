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
