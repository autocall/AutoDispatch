using System.Globalization;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    #region [ Load ]

    public async Task<LoadDriversResponseData> LoadDrivers(LoadDriversRequestData request) {
        var entities = await BaseLoadAsync<Driver>(request);
        var dtos = this.Mapper.Map<List<DriverDto>>(await entities.ToListAsync());
        await this.LoadNameOfLocationAsync(dtos);
        return new LoadDriversResponseData { Drivers = dtos };
    }

    public async Task<LoadTrucksResponseData> LoadTrucks(LoadTrucksRequestData request) {
        var entities = await BaseLoadAsync<Truck>(request);
        var dtos = this.Mapper.Map<List<TruckDto>>(await entities.ToListAsync());
        // NOTES: подгрузка в конвертере приведёт к множеству запросов
        await this.loadDefectsAsync(dtos, request.AllDefects);
        await this.LoadNameOfLocationAsync(dtos);
        return new LoadTrucksResponseData { Trucks = dtos };
    }

    public async Task<LoadTrailersResponseData> LoadTrailers(LoadTrailersRequestData request) {
        var trailerEntities = await BaseLoadAsync<Trailer>(request);
        // loads from history
        if (request.HistoryBegin != null && request.HistoryEnd != null) {
            var loadEntities = await BaseLoadAsync<Load>(request);
            var expression = GetHistoryLoadsLambda(request.Types, request.HistoryBegin, request.HistoryEnd);
            var trailerIds = (await loadEntities.Where(expression)
                .Select(e => new { e.HistoryAssignedTrailerId, e.HistoryTrailerId, e.TrailerId }).ToListAsync())
                .SelectMany(e => new List<Guid?>() { e.HistoryAssignedTrailerId, e.HistoryTrailerId, e.TrailerId }).Distinct()
                .Where(e => e != null).Select(e => (Guid)e).ToList();
            trailerEntities = trailerEntities.Where(e => trailerIds.Contains(e.EntityId));
        } else {
            trailerEntities = trailerEntities.Where(e => e.History == false);
        }
        var dtos = this.Mapper.Map<List<TrailerDto>>(await trailerEntities.ToListAsync());
        await this.loadDefectsAsync(dtos, request.AllDefects);
        await this.LoadNameOfLocationAsync(dtos);
        return new LoadTrailersResponseData { Trailers = dtos };
    }

    public async Task<LoadWarehousesResponseData> LoadWarehouses(LoadWarehousesRequestData request) {
        var entities = await BaseLoadAsync<Warehouse>(request);
        return new LoadWarehousesResponseData { Warehouses = this.Mapper.Map<List<WarehouseDto>>(await entities.ToListAsync()) };
    }

    public async Task<LoadYardsResponseData> LoadYards(LoadYardsRequestData request) {
        var entities = await BaseLoadAsync<Yard>(request);
        return new LoadYardsResponseData { Yards = this.Mapper.Map<List<YardDto>>(await entities.ToListAsync()) };
    }

    public async Task<LoadLoadsResponseData> LoadLoads(LoadLoadsRequestData request) {
        var entities = await BaseLoadAsync<Load>(request);
        var expression = GetHistoryLoadsLambda(request.Types, request.HistoryBegin, request.HistoryEnd);
        var models = await entities.Where(expression).ToListAsync();
        var dtos = this.Mapper.Map<List<LoadDto>>(models);
        dtos = await dtos.IncludeCompaniesAsync(Db);
        await this.LoadNameOfLocationAsync(dtos);
        return new LoadLoadsResponseData { Loads = dtos };
    }

    // History
    public async Task<LoadLoadsZipResponseData> LoadLoadsZip(LoadLoadsZipRequestData request) {
        var entities = await BaseLoadAsync<Load>(request);
        var expression = GetHistoryLoadsLambda(request.Types, request.HistoryBegin, request.HistoryEnd);
        var dtos = await this.Mapper.Map<List<LoadDto>>(await entities.Where(expression).ToListAsync()).IncludeCompaniesAsync(Db);
        await this.LoadNameOfLocationAsync(dtos);
        return new LoadLoadsZipResponseData {
            LoadsZip = dtos.DivideBy(100).Select(e => ZipExtension.SerializeObject(e)).ToList()
        };
    }

    /// <summary>
    ///     История грузов с фильтрами и постранично — сырой SQL как в V1 (таблицы без d_, пользователи — IdentityUsers,
    ///     радиус — гаверсинус по парам координат вместо STDistance) </summary>
    public async Task<LoadHistory2ResponseData> LoadHistory2(LoadHistory2RequestData request) {
        Guid companyId = await GetCompanyIdByUserIdAsync(request.UserId);
        var ci = CultureInfo.InvariantCulture;

        var conditions = new List<string>();
        conditions.Add($"[CompanyId]='{companyId}'");
        conditions.Add($"([Board]=2 OR [Board]=0)");/* Carrier / CarrierAccount */
        conditions.Add($"[ParentId] IS NULL");
        if (request.Request.MinPickupDate != null) {
            // Adds Chicago Time Zone
            var dt = request.Request.MinPickupDate.Value.AddHours(-5);
            conditions.Add($"[PickupFrom]>='{dt:yyyy-MM-dd HH:mm}'");
        }
        if (request.Request.MaxPickupDate != null) {
            var dt = request.Request.MaxPickupDate.Value.AddDays(1).AddHours(-5);
            conditions.Add($"[PickupTo]<='{dt:yyyy-MM-dd HH:mm}'");
        }
        if (request.Request.MinDeliveryDate != null) {
            var dt = request.Request.MinDeliveryDate.Value.AddHours(-5);
            conditions.Add($"[DeliveryFrom]>='{dt:yyyy-MM-dd HH:mm}'");
        }
        if (request.Request.MaxDeliveryDate != null) {
            var dt = request.Request.MaxDeliveryDate.Value.AddDays(1).AddHours(-5);
            conditions.Add($"[DeliveryTo]<='{dt:yyyy-MM-dd HH:mm}'");
        }
        if (request.Request.Origin != null) {
            if (request.Request.OriginRadius != null) {
                conditions.Add(RadiusCondition("Origin", request.Request.Origin, request.Request.OriginRadius.Value));
            } else {
                conditions.Add($"[OriginCity] LIKE '{Escape(request.Request.Origin.FormattedAddress)}%'");
            }
        }
        if (request.Request.Destination != null) {
            if (request.Request.DestinationRadius != null) {
                conditions.Add(RadiusCondition("Destination", request.Request.Destination, request.Request.DestinationRadius.Value));
            } else {
                conditions.Add($"[DestinationCity] LIKE '{Escape(request.Request.Destination.FormattedAddress)}%'");
            }
        }
        if (request.Request.MinPallets != null) conditions.Add($"[Pallets]>={request.Request.MinPallets}");
        if (request.Request.MaxPallets != null) conditions.Add($"[Pallets]<={request.Request.MaxPallets}");
        if (request.Request.MinWeight != null) conditions.Add($"[Weight]>={request.Request.MinWeight}");
        if (request.Request.MaxWeight != null) conditions.Add($"[Weight]<={request.Request.MaxWeight}");
        if (request.Request.MinLineHaul != null) conditions.Add($"[LineHaul]>={request.Request.MinLineHaul}");
        if (request.Request.MaxLineHaul != null) conditions.Add($"[LineHaul]<={request.Request.MaxLineHaul}");
        if (request.Request.Equipment == AutoCallEquipmentType.Van) {
            conditions.Add($"([Equipment]={(int)AutoCallEquipmentType.Van} OR [Equipment]={(int)AutoCallEquipmentType.VanorReefer})");
        }
        if (request.Request.Equipment == AutoCallEquipmentType.Reefer) {
            conditions.Add($"([Equipment]={(int)AutoCallEquipmentType.Reefer} OR [Equipment]={(int)AutoCallEquipmentType.VanorReefer})");
        }
        if (request.Request.LoadType != null) { // Full / Partial
            conditions.Add($"[LoadType]={request.Request.LoadType}");
        }
        if (String.IsNullOrEmpty(request.Request.LoadId) == false) {
            conditions.Add($"[Index] LIKE '%{Escape(request.Request.LoadId.Trim())}%'");
        }
        if (String.IsNullOrEmpty(request.Request.ProId) == false) {
            conditions.Add($"[RecordId] LIKE '%{Escape(request.Request.ProId.Trim())}%'");
        }
        if (request.Request.BrokerName != null) {
            var name = Escape(request.Request.BrokerName.Replace(" ", "").ToLower());
            conditions.Add($"[DatBrokerId] IN (SELECT [EntityId] FROM [DatCompany] WHERE LOWER(REPLACE([Name], ' ', '')) LIKE '%{name}%')");
        }
        if (request.Request.CreatedBy != null) {
            var name = Escape(request.Request.CreatedBy.Replace(" ", "").ToLower());
            conditions.Add($"[CreatedById] IN (SELECT [Id] FROM [IdentityUsers] WHERE LOWER(REPLACE([Email], ' ', '')) LIKE '%{name}%' OR LOWER(REPLACE([UserName], ' ', '')) LIKE '%{name}%')");
        }
        if (request.Request.ActualCompanyId != null) {
            conditions.Add($"[DispatchCompanyId]='{request.Request.ActualCompanyId}'");
        }

        var total = (await Db.QueryToListAsync<int>($@"
SELECT COUNT([EntityId]) FROM [Load]
WHERE {String.Join(" AND ", conditions)}")).First();

        var pageSize = Math.Max(1, request.Request.PageSize);
        var pageIndex = request.Request.PageIndex * pageSize <= total ? request.Request.PageIndex : total / pageSize;

        var skip = pageSize * request.Request.PageIndex;
        var top = pageSize;

        var loadIds = await Db.QueryToListAsync<Guid>($@"
SELECT [EntityId]
FROM [Load]
WHERE {String.Join(" AND ", conditions)}
ORDER BY [EntityId]
OFFSET ({skip}) ROWS FETCH NEXT ({top}) ROWS ONLY");

        var loads = await Alive<Load>().Where(e => loadIds.Contains(e.EntityId)).ToListAsync();
        var subLoads = await Alive<Load>().Where(e => e.ParentId != null && loadIds.Contains(e.ParentId.Value)).ToListAsync();

        var dtos = await Mapper.Map<List<LoadDto>>(loads.Union(subLoads)).IncludeCompaniesAsync(Db);
        await this.LoadNameOfLocationAsync(dtos);

        var actualCompanyIds = dtos.Select(e => e.ActualCompanyId).Distinct().ToList();
        var actualCompanies = await Alive<DispatchCompany>().Where(e => actualCompanyIds.Contains(e.EntityId)).ToListAsync();
        var actualCompanyDtos = Mapper.Map<List<ActualCompanyDto>>(actualCompanies);

        return new LoadHistory2ResponseData {
            Loads = dtos,
            ActualCompanies = actualCompanyDtos,
            Total = total,
            PageIndex = pageIndex
        };
    }

    private static string Escape(string value) => (value ?? "").Replace("'", "''");

    /// <summary>
    ///     Радиус в милях по гаверсинусу (V1 STDistance по geography) </summary>
    private static string RadiusCondition(string prefix, LocationDto point, int radius) {
        var ci = CultureInfo.InvariantCulture;
        var lat = point.Latitude.ToString(ci);
        var lng = point.Longitude.ToString(ci);
        return $"CAST(3958.8 * ACOS(COS(RADIANS({lat})) * COS(RADIANS([{prefix}Latitude])) * COS(RADIANS([{prefix}Longitude]) - RADIANS({lng})) + SIN(RADIANS({lat})) * SIN(RADIANS([{prefix}Latitude]))) AS INT)<={radius}";
    }

    /// <summary>
    ///     Используется для загрузки грузов из истории и трейлеров по грузам </summary>
    private static Expression<Func<Load, bool>> GetHistoryLoadsLambda(DispatchBoardType[] types, DateTime? historyBegin, DateTime? historyEnd) {
        const int completedStatus = (int)LoadStatus.Completed;
        const int canceledStatus = (int)LoadStatus.Canceled2;
        const int unpaidStatus = (int)LoadStatus.Unpaid;

        var expression = (Expression<Func<Load, bool>>)(l => true);

        if (types != null) {
            var boardType = types.Select(t => (byte)t).ToList();
            expression = ExpressionHelper.CombineLambdas(expression, l => boardType.Contains(l.Board), ExpressionType.AndAlso);
        }
        if (historyBegin.HasValue == false && historyEnd.HasValue == false) {
            expression = ExpressionHelper.CombineLambdas(expression, l => l.Status != unpaidStatus && l.Status != canceledStatus && l.Status != completedStatus && l.IsDeleted == false, ExpressionType.AndAlso);
        } else {
            expression = ExpressionHelper.CombineLambdas(expression, l => (l.Status == unpaidStatus || l.Status == canceledStatus || l.Status == completedStatus) && l.IsDeleted == false, ExpressionType.AndAlso);
            if (historyBegin.HasValue) {
                expression = ExpressionHelper.CombineLambdas(expression, l => l.PickupFrom >= historyBegin, ExpressionType.AndAlso);
            }
            if (historyEnd.HasValue) {
                expression = ExpressionHelper.CombineLambdas(expression, l => l.PickupFrom <= historyEnd, ExpressionType.AndAlso);
            }
        }
        return expression;
    }

    public async Task<LoadTotalHistoryLoadsResponseData> LoadTotalHistoryLoads(LoadTotalHistoryLoadsRequestData request) {
        var entities = await BaseLoadAsync<Load>(request);
        const int completedStatus = (int)LoadStatus.Completed;
        const int canceledStatus = (int)LoadStatus.Canceled2;
        const int unpaidStatus = (int)LoadStatus.Unpaid;
        var boardType = (int)request.Board;
        var groups = await entities
            .Where(l => l.Board == boardType && (l.Status == unpaidStatus || l.Status == canceledStatus || l.Status == completedStatus) && l.IsDeleted == false && l.ParentId == null)
            .GroupBy(l => new { y = l.PickupFrom.Year, m = (l.PickupFrom.Month - 1) - (l.PickupFrom.Month - 1) % 3 })
            .Select(group => new { Y = group.Key.y, M = group.Key.m, Total = group.Count() })
            .ToListAsync();
        return new LoadTotalHistoryLoadsResponseData {
            Histories = groups.ToDictionary(e => new DateTime(e.Y, e.M + 1, 1), e => e.Total)
        };
    }

    public async Task<LoadSetupPacketsResponseData> LoadSetupPackets(LoadSetupPacketsRequestData request) {
        var entities = await (await BaseLoadAsync<SetupPacket>(request)).ToListAsync();
        await Host.GetService<SetupPacketService>().LoadRelationsAsync(entities);
        return new LoadSetupPacketsResponseData { SetupPackets = this.Mapper.Map<List<SetupPacketDto>>(entities) };
    }

    public async Task<LoadDatCompaniesResponseData> LoadDatCompanies(LoadDatCompaniesRequestData request) {
        var entities = await BaseLoadAsync<DatCompany>(request);
        return new LoadDatCompaniesResponseData { DatCompanies = this.Mapper.Map<List<DatCompanyDto>>(await entities.Where(e => e.IsDeleted == false).ToListAsync()) };
    }

    public async Task<LoadDatDriversResponseData> LoadDatDrivers(LoadDatDriversRequestData request) {
        var entities = await BaseLoadAsync<DatDriver>(request);
        var dtos = this.Mapper.Map<List<DatDriverDto>>(await entities.ToListAsync());
        await this.LoadNameOfLocationAsync(dtos);
        return new LoadDatDriversResponseData { DatDrivers = dtos };
    }

    public async Task<LoadShipperReceiverCompaniesResponseData> LoadShipperReceiverCompanies(LoadShipperReceiverCompaniesRequestData request) {
        Guid companyId = await GetCompanyIdByUserIdAsync(request.UserId);
        var entities = await Alive<ShipperReceiverCompany>().Where(d => d.CompanyId == companyId).ToListAsync();
        return new LoadShipperReceiverCompaniesResponseData { Companies = this.Mapper.Map<List<ShipperReceiverCompanyDto>>(entities) };
    }

    public async Task<LoadCustomerCompaniesResponseData> LoadCustomerCompanies(LoadCustomerCompaniesRequestData request) {
        Guid companyId = await GetCompanyIdByUserIdAsync(request.UserId);
        var entities = await Alive<CustomerCompany>().Where(d => d.CompanyId == companyId).ToListAsync();
        return new LoadCustomerCompaniesResponseData { Companies = this.Mapper.Map<List<CustomerCompanyDto>>(entities) };
    }

    /// <summary>
    ///     Живые объекты компании пользователя </summary>
    public async Task<IQueryable<TEntity>> BaseLoadAsync<TEntity>(BaseEventRequestData request) where TEntity : BaseObjectEntity {
        Guid companyId = await GetCompanyIdByUserIdAsync(request.UserId);
        return Alive<TEntity>().Where(d => d.CompanyId == companyId);
    }

    #endregion [ Load ]

    #region [ Statistics ]

    /// <summary>
    ///     Грузы пользователя за неделю с доставкой в West (Z9) и Western Canada (ZW). V1 — пространственный join с StateBorders;
    ///     таблиц StateBorders/State/Zone нет — штат берётся из DestinationCity "City, ST", зона — StateHelper (AI/wiki/plan.md) </summary>
    public async Task<GetStatisticsResponseData> GetStatistics(GetStatisticsRequestData request) {
        var firstDay = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);
        var canceled1 = (byte)LoadStatus.Canceled1;
        var canceled2 = (byte)LoadStatus.Canceled2;
        var cities = await Alive<Load>()
            .Where(l => l.ParentId == null && l.Status != canceled1 && l.Status != canceled2 && l.CreatedById == request.UserId && l.PickupFrom >= firstDay)
            .Select(l => l.DestinationCity).ToListAsync();
        var zones = new HashSet<byte> { StateHelper.GetZoneByCode("Z9").Id, StateHelper.GetZoneByCode("ZW").Id };
        var total = cities.Count(city => {
            var state = StateHelper.GetStateByCode(LocationHelper.GetStateCode(city));
            return state != null && zones.Contains(state.ZoneId);
        });
        return new GetStatisticsResponseData() { Total = total };
    }

    public async Task<GetDatCompanyStatisticsResponseData> GetDatCompanyStatictics(GetDatCompanyStatisticsRequestData request) {
        IQueryable<Load> entities = await BaseLoadAsync<Load>(request);
        const int completedStatus = (int)LoadStatus.Completed;
        const int canceledStatus = (int)LoadStatus.Canceled2;
        const int unpaidStatus = (int)LoadStatus.Unpaid;
        var boardType = (int)DispatchBoardType.BrokerAccount;
        var groups = await entities
            .Where(l => l.Board == boardType && l.IsDeleted == false && l.ParentId == null && l.HistoryDatCompanyId != null &&
                        (l.Status == unpaidStatus || l.Status == canceledStatus || l.Status == completedStatus))
            .GroupBy(l => l.HistoryDatCompanyId)
            .Select(group => new { HistoryDatCompanyId = group.Key.Value, Total = group.Count() })
            .ToListAsync();
        return new GetDatCompanyStatisticsResponseData {
            Totals = groups.ToDictionary(e => e.HistoryDatCompanyId, e => e.Total)
        };
    }

    #endregion [ Statistics ]
}
