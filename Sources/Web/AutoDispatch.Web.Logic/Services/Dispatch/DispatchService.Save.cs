using System.Text.RegularExpressions;
using LinqToDB;
using LinqToDB.Async;
using AutoCall.Common.Attributes;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Common.Helpers;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Dtos.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Mapping.Dispatch;
using AutoDispatch.Web.Logic.Utils;
using AutoDispatch.Web.Logic.WebServices.CarrierView;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    #region [ Save ]

    public async Task<SaveDriverResponseData> SaveDriver(SaveDriverRequestData request) {
        Driver entity = await BaseSaveAsync<DriverDto, Driver>(request.UserId, request.Driver);
        await this._saveEventHistoryDriverAsync(EventTypeModel.Driver, entity, null, request.UserId);
        DriverDto dto = this.Mapper.Map<DriverDto>(entity);
        await this.LoadNameOfLocationAsync(dto);
        return new SaveDriverResponseData { Driver = dto };
    }

    public async Task<SaveTruckResponseData> SaveTruck(SaveTruckRequestData request) {
        if (request.Truck.TrailerId != null) {
            var trailer = await GetByIdAsync<Trailer>(request.Truck.TrailerId);
            if (trailer?.YardId != null) {
                throw new InfoException("Cannot link trailer with truck and yard");
            }
        }

        Truck entity = await BaseSaveAsync<TruckDto, Truck>(request.UserId, request.Truck);
        await SaveDefectsAsync(request.Truck.Defects, entity.CompanyId, request.UserId);
        await this._saveEventHistoryTruckAsync(EventTypeModel.Truck, entity, null, request.UserId);
        TruckDto dto = this.Mapper.Map<TruckDto>(entity);
        await this.loadDefectsAsync(new List<TruckDto>() { dto });
        await this.LoadNameOfLocationAsync(dto);
        return new SaveTruckResponseData { Truck = dto };
    }

    public async Task<SaveTrailerResponseData> SaveTrailer(SaveTrailerRequestData request) {
        YardDto yardDto = null;
        var oldLocation = request.Trailer.CurrentLocation;
        if (request.Trailer.YardId != null) {
            var yard = await GetByIdAsync<Yard>(request.Trailer.YardId);
            yardDto = Mapper.Map<YardDto>(yard);
            var trucksAny = await Alive<Truck>().AnyAsync(e => e.TrailerId == request.Trailer.EntityId);
            if (trucksAny) {
                throw new InfoException("Cannot link a trailer with truck and yard");
            }
            // V1: MathHelper.LocationCompare(..Latitude) == false || MathHelper.LocationCompare(..Longitude) — сохранено как есть
            if (MathHelper.LocationCompare(oldLocation.Latitude, yardDto.Location.Latitude) == false ||
                MathHelper.LocationCompare(oldLocation.Longitude, yardDto.Location.Longitude)) {
                request.Trailer.CurrentLocation = yardDto.Location;
            }
        }

        Trailer entity = await BaseSaveAsync<TrailerDto, Trailer>(request.UserId, request.Trailer);
        if (request.SkipDefects == false) {
            await SaveDefectsAsync(request.Trailer.Defects, entity.CompanyId, request.UserId);
        }
        await this._saveEventHistoryTrailerAsync(EventTypeModel.Trailer, entity, request.UserId);
        TrailerDto dto = this.Mapper.Map<TrailerDto>(entity);
        if (request.SkipDefects == false) {
            await this.loadDefectsAsync(new List<TrailerDto>() { dto });
        }
        await this.LoadNameOfLocationAsync(dto);

        if (request.Trailer.YardId != null) {
            if (MathHelper.LocationCompare(oldLocation.Latitude, yardDto.Location.Latitude) == false ||
                MathHelper.LocationCompare(oldLocation.Longitude, yardDto.Location.Longitude) == false) {
                request.Trailer.CurrentLocation = yardDto.Location;
                await this.LocationTrailerChanged(new LocationTrailerChangedRequestData(request.UserId) {
                    TrailerId = request.Trailer.EntityId,
                    Location = yardDto.Location
                });
            }
        }

        return new SaveTrailerResponseData { Trailer = dto };
    }

    public async Task<SaveWarehouseResponseData> SaveWarehouse(SaveWarehouseRequestData request) {
        Warehouse entity = await BaseSaveAsync<WarehouseDto, Warehouse>(request.UserId, request.Warehouse);
        await this._saveEventWarehouseAsync(entity, request.UserId);
        return new SaveWarehouseResponseData { Warehouse = this.Mapper.Map<WarehouseDto>(entity) };
    }

    public async Task<SaveYardResponseData> SaveYard(SaveYardRequestData request) {
        Yard entity = await BaseSaveAsync<YardDto, Yard>(request.UserId, request.Yard);
        await this._saveEventYardAsync(entity, request.UserId);
        return new SaveYardResponseData { Yard = this.Mapper.Map<YardDto>(entity) };
    }

    /// <summary>
    ///     Sets status as parent </summary>
    public async Task<FixLoadResponseData> FixLoad(FixLoadRequestData request) {
        var load = await GetByIdAsync<Load>(request.LoadId);
        if (load.ParentId != null) {
            var parent = await GetByIdAsync<Load>(load.ParentId);
            if (parent != null) {
                load.Status = parent.Status;
                load.IsDeleted = parent.IsDeleted;
            }
        }
        var dto = Mapper.Map<LoadDto>(load);
        return new FixLoadResponseData() {
            Load = (await SaveLoad(new SaveLoadRequestData(request.UserId) { Load = dto })).Load
        };
    }

    private async Task SetPickupDeliveryLocation(LoadDto dto, Guid userId) {
        if (dto.TrailerId != null) {
            switch (dto.Status) {
                case LoadStatus.AtPickUp:
                case LoadStatus.PickedUp:
                    await this.LocationTrailerChanged(new LocationTrailerChangedRequestData(userId) {
                        TrailerId = dto.TrailerId.Value,
                        Location = dto.OriginLocation
                    });
                    break;
                case LoadStatus.AtDelivery:
                case LoadStatus.Delivered:
                    await this.LocationTrailerChanged(new LocationTrailerChangedRequestData(userId) {
                        TrailerId = dto.TrailerId.Value,
                        Location = dto.DestinationLocation
                    });
                    break;
            }
        }
    }

    /// <summary>
    ///     Throwable </summary>
    private static void CheckLoad(LoadDto load) {
        var originCity = load.OriginCity;
        if (LocationHelper.CheckLocation(ref originCity) == false) {
            throw new InfoException($"Invalid Origin: '{originCity}'");
        }
        load.OriginCity = originCity;
        var destinationCity = load.DestinationCity;
        if (LocationHelper.CheckLocation(ref destinationCity) == false) {
            throw new InfoException($"Invalid Destination: '{destinationCity}'");
        }
        load.DestinationCity = destinationCity;
    }

    private static void CheckLoads(List<LoadDto> loads) {
        foreach (var load in loads) {
            CheckLoad(load);
        }
    }

    /// <summary>
    ///     Компания груза с кастомизациями нумерации; изменения счётчиков сохраняет вызывающий (SaveDispatchCompanyCountersAsync) </summary>
    private async Task<(DispatchCompany company, List<DispatchCompanyCustom> customizes)> GetDispatchCompanyAsync(Guid? id) {
        var company = await GetByIdAsync<DispatchCompany>(id);
        var customizes = company == null ? new List<DispatchCompanyCustom>()
            : await Alive<DispatchCompanyCustom>().Where(e => e.DispatchCompanyId == company.EntityId).ToListAsync();
        return (company, customizes);
    }

    private async Task SaveDispatchCompanyCountersAsync(DispatchCompany company) {
        await Table<DispatchCompany>().Where(e => e.EntityId == company.EntityId)
            .Set(e => e.NextLoadNumber, company.NextLoadNumber)
            .Set(e => e.NextLoadExtra, company.NextLoadExtra)
            .Set(e => e.NextInvoiceNumber, company.NextInvoiceNumber)
            .UpdateAsync();
    }

    public async Task<SaveLoadResponseData> SaveLoad(SaveLoadRequestData request) {
        var groupId = Guid.NewGuid();
        LoadDto load = request.Load;
        CheckLoad(load);
        // Sets TimeZones
        await SetTimeZoneOffsetAsync(load);

        // Sets index
        if (load.ActualCompanyId != null) {
            if (load.Index == null && load.IndexExtra == null) {
                var (company, customizes) = await GetDispatchCompanyAsync(load.ActualCompanyId);
                load.Index = DispatchHelper.NextLoadNumber(load, company, customizes);
                await SaveDispatchCompanyCountersAsync(company);
            }
        }
        // Sets index for invoice
        if (request.SaveInvoice && load.ActualCompanyId != null) {
            var (company, customizes) = await GetDispatchCompanyAsync(load.ActualCompanyId);
            if (company.InvoiceNumberAsLoadNumber == false) {
                if (load.InvoiceNumber == null) {
                    load.InvoiceNumber = DispatchHelper.NextInvoiceNumber(load, company, customizes);
                    await SaveDispatchCompanyCountersAsync(company);
                }
            } else {
                load.InvoiceNumber = load.IndexString;
            }
        }
        if (request.Load.Status == LoadStatus.Delivered && request.Load.DeliveredDate == null) {
            request.Load.DeliveredDate = DateTime.Now;
        }
        Load entity = await BaseSaveAsync<LoadDto, Load>(request.UserId, load, groupId);
        await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, entity, null, LoadParentType.None, request.UserId);
        LoadDto dto = this.Mapper.Map<LoadDto>(entity);
        await Host.GetService<LoadIndexCacheService>().SaveLoadIndexAsync(entity.CompanyId, LoadIndexSource.From(dto)); // Adds index
        if (dto.ParentId != null) {
            //changing status for container
            await SaveParentAsync(dto.ParentId.Value, request.UserId, groupId);
        }
        if (dto.ShipmentId != null) {
            //changing status for container
            await SaveParentAsync(dto.ShipmentId.Value, request.UserId, groupId);
        }
        if (dto.ContainerType != ContainerType.None) {
            await SaveChildrenAsync(dto, request.UserId, groupId);
        }
        await SetPickupDeliveryLocation(dto, request.UserId);

        if (dto.Status == LoadStatus.Canceled2 || dto.Status == LoadStatus.Unpaid || dto.Status == LoadStatus.Completed) {
            await MoveToHistoryTrailersAsync(request.UserId, dto);
        }

        return new SaveLoadResponseData { Load = dto };
    }

    public async Task<SaveLoadsResponseData> SaveLoads(SaveLoadsRequestData request) {
        try {
            var groupId = Guid.NewGuid();

            var saveLoads = request.Loads.Where(e => e.IsDeleted == false).OrderBy(e => e.ParentId).ToList(); // Sets index
            var deleteLoads = request.Loads.Where(e => e.IsDeleted == true).ToList();

            if (deleteLoads.Any()) {
                var warehouseRoleId = RoleEnum.Warehouse.ToRoleId();
                var isWarehouse = await Table<AdUserRole>().AnyAsync(e => e.UserId == request.UserId && e.RoleId == warehouseRoleId);
                if (isWarehouse) {
                    throw new AccessDeniedException($"Access denied to delete loads for '{RoleEnum.Warehouse}'");
                }
            }

            CheckLoads(saveLoads);

            // marks as delete and saves history
            List<Load> delEntitites = await BaseDeletesAsync<LoadDto, Load>(request.UserId, deleteLoads, groupId);
            await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad,
                delEntitites.ToDictionary(e => e, e => (int?)null), LoadParentType.None, request.UserId);

            foreach (var load in saveLoads) {
                // Sets TimeZones
                await SetTimeZoneOffsetAsync(load);
                if (load.ActualCompanyId != null) {
                    if (load.Index == null) {
                        if (load.ParentId == null) {
                            var match = Regex.Match(load.IndexExtra ?? "", @"^(?<prefix>\D*)(?<index>\d{1,8})(?<extra>.*)$");
                            if (load.IndexExtra != null && match.Success) {
                                load.Index = Int32.Parse(match.Groups["index"].Value);
                                load.IndexPrefix = match.Groups["prefix"].Value;
                                load.IndexExtra = match.Groups["extra"].Value;
                            } else {
                                var (company, customizes) = await GetDispatchCompanyAsync(load.ActualCompanyId);
                                load.Index = DispatchHelper.NextLoadNumber(load, company, customizes);
                                await SaveDispatchCompanyCountersAsync(company);
                            }
                        } else {
                            // a subload
                            var parent = saveLoads.FirstOrDefault(e => e.ParentId == null && load.ParentId == e.EntityId);
                            if (parent != null) {
                                load.Index = parent.Index;
                                load.IndexPrefix = parent.IndexPrefix;
                                if (load.IndexExtra == null) {
                                    var indexExtraList = saveLoads.Where(e => e.ParentId != parent.EntityId).Select(e => e.IndexExtra);
                                    load.IndexExtra = "/" + NextIndexExtra(indexExtraList);
                                }
                            }
                        }
                    }
                }
                // Tag: "EditActualCompany"
                if (load.IndexCorrected) {
                    if (load.IndexExtra?.Last() != 'C') {
                        load.IndexExtra += 'C';
                    }
                }
                if (load.Status == LoadStatus.Delivered && load.DeliveredDate == null) {
                    load.DeliveredDate = DateTime.Now;
                }
            }
            List<Load> entities = await BaseSavesAsync<LoadDto, Load>(request.UserId, saveLoads, groupId);
            await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad,
                entities.ToDictionary(e => e, e => (int?)null), LoadParentType.None, request.UserId);
            // первым Root
            List<LoadDto> dtos = this.Mapper.Map<List<LoadDto>>(entities.OrderBy(e => e.ParentId));
            var result = new List<LoadDto>();
            result.AddRange(dtos);
            foreach (var dto in dtos) {
                if (dto.ParentId != null) {
                    result.AddRange(await SaveParentAsync(dto.ParentId.Value, request.UserId, groupId));
                }
                if (dto.ShipmentId != null) {
                    result.AddRange(await SaveParentAsync(dto.ShipmentId.Value, request.UserId, groupId));
                }
                if (dto.ContainerType != ContainerType.None) {
                    result.AddRange(await SaveChildrenAsync(dto, request.UserId, groupId));
                }
                await SetPickupDeliveryLocation(dto, request.UserId);

                if (dto.Status == LoadStatus.Unpaid || dto.Status == LoadStatus.Canceled2 || dto.Status == LoadStatus.Completed) {
                    await MoveToHistoryTrailersAsync(request.UserId, dto);
                }
            }
            if (entities.Any()) {
                var cache = Host.GetService<LoadIndexCacheService>();
                foreach (var dto in dtos) {
                    await cache.SaveLoadIndexAsync(entities.First().CompanyId, LoadIndexSource.From(dto)); // Adds indexes
                }
            }
            return new SaveLoadsResponseData { Loads = result.Distinct().ToList() };
        } catch (Exception e) {
            _l.e("SaveLoads", e);
            throw;
        }
    }

    private async Task MoveToHistoryTrailersAsync(Guid userId, LoadDto load) {
        if (load.HistoryTrailerId != null) {
            var status1 = (byte)LoadStatus.Unpaid;
            var status2 = (byte)LoadStatus.Canceled2;
            var status3 = (byte)LoadStatus.Completed;
            var actualTrailerAny = await Alive<Load>().AnyAsync(e =>
                e.Status != status1 && e.Status != status2 && e.Status != status3 && // all actual statuses
                (e.TrailerId == load.HistoryTrailerId ||
                 e.HistoryTrailerId == load.HistoryTrailerId ||
                 e.HistoryAssignedTrailerId == load.HistoryTrailerId));
            // этот контейнер должен уйти в историю, т.к у него нет привязанных грузов
            if (actualTrailerAny == false) {
                var trailer = await GetByIdAsync<Trailer>(load.HistoryTrailerId);
                // если контейнер был на railway
                if (trailer != null && trailer.Type == (int)TrailerType.Container && trailer.BnsfUnit != null) {
                    var trailerDto = Mapper.Map<TrailerDto>(trailer);
                    trailerDto.History = true;
                    DispatchPdfGenerator.DeleteOriginScheme(trailerDto);
                    await SaveTrailer(new SaveTrailerRequestData(userId) { Trailer = trailerDto });
                }
            }
        }
    }

    private static string NextIndexExtra(IEnumerable<string> extraIndexes) {
        var nextId = extraIndexes.Select(e => Int32.Parse(DispatchStringExtension.OnlyNumbers(e.Split('/').Last()))).Max() + 1;
        return nextId.ToString();
    }

    private async Task SetTimeZoneOffsetAsync(LoadDto load) {
        // sets origin timezone offset
        var originTimeZoneResult = await Geocode.GetTypedTimeZoneAsync(load.OriginLocation, load.PickupFrom);
        if (originTimeZoneResult.Success == false) {
            throw new InfoException(originTimeZoneResult.ErrorMessage);
        }
        load.CurrentOriginUtcOffset = originTimeZoneResult.Data.RawOffset;
        if (originTimeZoneResult.Data.DstOffset != 0) {
            load.CurrentOriginUtcOffset -= originTimeZoneResult.Data.DstOffset;
        }
        // sets destination timezone offset
        var destinationTimeZoneResult = await Geocode.GetTypedTimeZoneAsync(load.DestinationLocation, load.DeliveryFrom);
        if (destinationTimeZoneResult.Success == false) {
            throw new InfoException(destinationTimeZoneResult.ErrorMessage);
        }
        load.CurrentDestinationUtcOffset = destinationTimeZoneResult.Data.RawOffset;
        if (destinationTimeZoneResult.Data.DstOffset != 0) {
            load.CurrentDestinationUtcOffset -= destinationTimeZoneResult.Data.DstOffset;
        }
    }

    /// <summary>
    ///     Sets status as min statuses of children and saves it </summary>
    private async Task<List<LoadDto>> SaveParentAsync(Guid parentId, Guid userId, Guid? groupId) {
        var parent = await GetByIdAsync<Load>(parentId);

        var oldStatus = parent.Status;
        var oldBoard = parent.Board;
        var oldContainerType = parent.ContainerType;
        var oldDivideType = parent.DivideType;

        parent.CurrentLatitude = null;
        parent.CurrentLongitude = null;
        // When all subloads has been deleted
        var anyChildren = await Alive<Load>().AnyAsync(e => e.ParentId == parentId || e.ShipmentId == parentId);
        if (anyChildren == false) {
            parent.ContainerType = (byte)ContainerType.None;
            parent.DivideType = (byte)DivideType.None;
        } else {
            var carrier = (byte)DispatchBoardType.Carrier;
            var broker = (byte)DispatchBoardType.Broker;
            var anRoute = await Alive<Load>().AnyAsync(e =>
                (e.ParentId == parentId || e.ShipmentId == parentId) && (e.Board == carrier || e.Board == broker));
            var status = (await Alive<Load>().Where(e => e.ParentId == parentId || e.ShipmentId == parentId).Select(e => e.Status).ToListAsync())
                .DefaultIfEmpty((byte)LoadStatus.None).Min();

            if (parent.Status != status) {
                parent.Status = status;
            }
            if (anRoute == false) {
                switch ((DispatchBoardType)parent.Board) {
                    case DispatchBoardType.Carrier:
                        parent.Board = (byte)DispatchBoardType.CarrierAccount;
                        break;
                    case DispatchBoardType.Broker:
                        parent.Board = (byte)DispatchBoardType.BrokerAccount;
                        break;
                }
            }
        }
        if (oldStatus != parent.Status ||
            oldBoard != parent.Board ||
            oldContainerType != parent.ContainerType ||
            oldDivideType != parent.DivideType) {
            LoadDto dto = this.Mapper.Map<LoadDto>(parent);
            parent = await BaseSaveAsync<LoadDto, Load>(userId, dto, groupId);
            var result = await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, parent, null, LoadParentType.None, userId);
            return Mapper.Map<List<LoadDto>>(result.loads);
        }
        return new List<LoadDto>();
    }

    /// <summary>
    ///     Sets status from parent to children </summary>
    private async Task<List<LoadDto>> SaveChildrenAsync(LoadDto parent, Guid userId, Guid? groupId) {
        var children = await Alive<Load>().Where(e => e.ParentId == parent.EntityId || e.ShipmentId == parent.EntityId).ToListAsync();
        var childrenDtos = this.Mapper.Map<List<LoadDto>>(children);
        var changeDtos = new List<LoadDto>();
        // sets Status from parent
        if (childrenDtos.Any()) {
            foreach (var childDto in childrenDtos) {
                if (parent.BoardType == DispatchBoardType.CarrierAccount && childDto.BoardType == DispatchBoardType.Carrier) {
                    childDto.BoardType = DispatchBoardType.CarrierAccount;
                    childDto.BoardChanged = DateTime.Now;
                    changeDtos.Add(childDto);
                }
                if (parent.BoardType == DispatchBoardType.BrokerAccount && childDto.BoardType == DispatchBoardType.Broker) {
                    childDto.BoardType = DispatchBoardType.BrokerAccount;
                    childDto.BoardChanged = DateTime.Now;
                    changeDtos.Add(childDto);
                }
                if (parent.Status != childDto.Status) {
                    // if an route -> delivery, or delivery -> paid
                    // share status and carrier/driver
                    if (DisplayAttribute.GetOrder(parent.Status) > DisplayAttribute.GetOrder(childDto.Status)) {
                        if (childDto.Status == LoadStatus.Canceled1) {
                            childDto.Status = LoadStatus.Canceled2;
                        } else {
                            childDto.Status = parent.Status;
                        }
                        childDto.HistoryDatCompanyId = parent.HistoryDatCompanyId;
                        childDto.HistoryDatDriverId = parent.HistoryDatDriverId;
                        childDto.DatCompanyId = parent.DatCompanyId;
                        childDto.DatDriverId = parent.DatDriverId;
                        changeDtos.Add(childDto);
                    }
                }
                if (parent.CustomStatusId != childDto.CustomStatusId) {
                    childDto.CustomStatusId = parent.CustomStatusId;
                    changeDtos.Add(childDto);
                }
                if (childDto.ContainerType != ContainerType.None) {
                    await SaveChildrenAsync(childDto, userId, groupId);
                }
                await MoveToHistoryTrailersAsync(userId, childDto);
            }
            // propagates appointment
            var appointment = childrenDtos.FirstOrDefault(e => e.NewScheduledAppointment != null)?.NewScheduledAppointment;
            if (appointment != null) {
                foreach (var dto in childrenDtos) {
                    if (dto.NewScheduledAppointment != appointment) {
                        dto.NewScheduledAppointment = appointment;
                        changeDtos.Add(dto);
                    }
                }
            }
            changeDtos = changeDtos.Distinct().ToList();
            var entities = await BaseSavesAsync<LoadDto, Load>(userId, changeDtos, groupId);
            await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad, entities.ToDictionary(e => e, e => (int?)null), LoadParentType.None, userId);
        }

        return changeDtos;
    }

    public async Task<SaveSetupPacketResponseData> SaveSetupPacket(SaveSetupPacketRequestData request) {
        SetupPacket entity = await BaseSaveAsync<SetupPacketDto, SetupPacket>(request.UserId, request.SetupPacket);
        // loads contacts
        await Host.GetService<SetupPacketService>().LoadRelationsAsync(new[] { entity });
        await this._saveEventSetupPacketAsync(entity, request.UserId);
        return new SaveSetupPacketResponseData() { SetupPacket = this.Mapper.Map<SetupPacketDto>(entity) };
    }

    public async Task<SaveDatCompanyResponseData> SaveDatCompany(SaveDatCompanyRequestData request) {
        if (await DatCompanyCheckExists(request.DatCompany, request.UserId) == true) {
            throw new InfoException($"The company with name '{request.DatCompany.Name}' already exists");
        }
        if (request.DatCompany.InsuranceFileData != null) {
            await InsuranceSaveAsync(request.DatCompany);
        }

        #region To Send Email
        var oldDto = Mapper.Map<DatCompanyDto>(await GetByIdAsync<DatCompany>(request.DatCompany.EntityId));
        if (oldDto != null && oldDto.SetupPacketAccept == false && request.DatCompany.SetupPacketAccept == true) {
            request.DatCompany.HistoryId = oldDto.HistoryId;
            request.DatCompany.CompanyId = oldDto.CompanyId;
            await SendSetupPacketAceeptEmailAsync(request.DatCompany);
        }
        #endregion To Send Email
        DatCompany entity = await BaseSaveAsync<DatCompanyDto, DatCompany>(request.UserId, request.DatCompany);

        LoadCompanyNames.RemoveDatBroker(entity.EntityId);
        await this._saveEventDatCompanyAsync(entity, request.UserId);

        return new SaveDatCompanyResponseData { DatCompany = this.Mapper.Map<DatCompanyDto>(entity) };
    }

    public async Task<SaveDatDriverResponseData> SaveDatDriver(SaveDatDriverRequestData request) {
        DatDriver entity = await BaseSaveAsync<DatDriverDto, DatDriver>(request.UserId, request.DatDriver);
        if (request.DatDriver.CarrierViewEdit) {
            await this._saveEventHistoryDatDriverAsync(EventTypeModel.DatDriver, entity, request.UserId);
            if (entity.CarrierViewId != null) {
                var assignedLoads = await Alive<Load>()
                    .Where(e => e.DatDriverId == request.DatDriver.EntityId && e.TrackerId == entity.CarrierViewId.Value).ToListAsync();
                foreach (var load in assignedLoads) {
                    var actualCompany = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);
                    var editRequest = new EditRequest() { locations = new List<EditLocationRequest>() };
                    if (request.DatDriver.CarrierViewPickupId != null) {
                        editRequest.locations.Add(new EditLocationRequest() {
                            id = request.DatDriver.CarrierViewPickupId.Value,
                            comment = request.DatDriver.CarrierViewPickupComment
                        });
                    }
                    if (request.DatDriver.CarrierViewDeliveryId != null) {
                        editRequest.locations.Add(new EditLocationRequest() {
                            id = request.DatDriver.CarrierViewDeliveryId.Value,
                            comment = request.DatDriver.CarrierViewDeliveryComment
                        });
                    }
                    await Host.GetService<CarrierViewWebService>().EditLoadAsync(actualCompany.CarrierViewToken, request.DatDriver.CarrierViewId.Value, editRequest);
                }
            }
        }
        DatDriverDto dto = this.Mapper.Map<DatDriverDto>(entity);
        await this.LoadNameOfLocationAsync(dto);
        return new SaveDatDriverResponseData { DatDriver = dto };
    }

    public async Task<SaveFactoringCompanyResponseData> SaveFactoringCompany(SaveFactoringCompanyRequestData request) {
        var actualCompany = await GetByIdAsync<DispatchCompany>(request.ActualCompanyId);
        if (actualCompany == null) {
            throw new InfoException("Actual company not found");
        }
        await Table<DispatchCompany>().Where(e => e.EntityId == request.ActualCompanyId)
            .Set(e => e.FactoringCompanyJson, request.FactoringCompanyJson)
            .UpdateAsync();
        return new SaveFactoringCompanyResponseData { FactoringCompanyJson = request.FactoringCompanyJson };
    }

    private async Task<bool> DatCompanyCheckExists(DatCompanyDto datCompany, Guid userId) {
        Guid companyId = await GetCompanyIdByUserIdAsync(userId);
        var oldCompany = await GetByIdAsync<DatCompany>(datCompany.EntityId);
        if (oldCompany != null) {
            return false;
        }
        return await Table<DatCompany>().AnyAsync(c => c.IsDeleted == false &&
            c.CompanyId == companyId &&
            c.DispatchCompanyId == datCompany.ActualCompanyId &&
            ((datCompany.McNumber != null && c.McNumber == datCompany.McNumber && c.DotNumber == datCompany.DotNumber) ||
             (datCompany.Name == c.Name)) &&
            c.EntityId != datCompany.EntityId);
    }

    /// <summary>
    ///     Saves companies </summary>
    /// <remarks>Searches by ID or Name</remarks>
    private async Task SaveCompaniesAsync(LoadDto load, Guid userId) {
        if (String.IsNullOrEmpty(load.DatBrokerCompany) == false) {
            DatCompanyDto company = (await DatCompanies.SearchForAsync(
                    e => e.CompanyId == load.CompanyId && (e.EntityId == load.DatBrokerId || (load.McNumber != null && e.McNumber == load.McNumber))))
                .FirstOrDefault() ?? new DatCompanyDto();
            if (company.Name != load.DatBrokerCompany || company.McNumber != load.McNumber ||
                company.Contacts != load.Contacts) {
                company.Name = load.DatBrokerCompany;
                company.McNumber = load.McNumber;
                company.Contacts = load.Contacts;
                company.ActualCompanyId = load.ActualCompanyId;
                company.CompanyId = load.CompanyId;
                company.EntityType = DatCompanyType.Broker;
                // loads DOT#
                if (load.McNumber != null) {
                    var search = new CensusCompanySearchDto() { Limit = 1, ICC = load.McNumber };
                    var censusCompanies = await Host.GetService<CensusService>().SearchCensusCompaniesAsync(search);
                    if (censusCompanies.Any()) {
                        company.DotNumber = censusCompanies.First().DOT_NUMBER;
                    }
                }
                // saving with event
                DatCompany entity = await BaseSaveAsync<DatCompanyDto, DatCompany>(userId, company);
                await this._saveEventDatCompanyAsync(entity, userId);
            }
            load.DatBrokerId = company.EntityId;
            LoadCompanyNames.RemoveDatBroker(company.EntityId);
        }

        if (String.IsNullOrEmpty(load.CustomerCompany) == false) {
            var service = Host.GetService<CustomerCompanyService>();
            CustomerCompanyDto customer = await service.GetByIdAsync(load.CustomerId) ?? new CustomerCompanyDto();
            if (customer.Name != load.CustomerCompany || customer.Contacts != load.Contacts) {
                customer.Name = load.CustomerCompany;
                customer.Contacts = load.Contacts;
                customer.CompanyId = load.CompanyId;
                await service.SaveAsync(customer, userId);
            }
            load.CustomerId = customer.EntityId;
            LoadCompanyNames.RemoveCustomer(customer.EntityId);
        }

        if (String.IsNullOrEmpty(load.ShipperCompany) == false) {
            var service = Host.GetService<ShipperReceiverCompanyService>();
            ShipperReceiverCompanyDto company = (await service.SearchForAsync(c => c.CompanyId == load.CompanyId && (c.EntityId == load.ShipperId || c.Name == load.ShipperCompany)))
                .FirstOrDefault() ?? new ShipperReceiverCompanyDto();
            if (company.Name != load.ShipperCompany || company.Contacts != load.ShipperContacts) {
                company.Name = load.ShipperCompany;
                company.Contacts = load.ShipperContacts;
                company.CompanyId = load.CompanyId;
                await service.SaveAsync(company, userId);
            }
            load.ShipperId = company.EntityId;
            LoadCompanyNames.RemoveCompany(company.EntityId);
        }

        if (String.IsNullOrEmpty(load.ReceiverCompany) == false) {
            var service = Host.GetService<ShipperReceiverCompanyService>();
            ShipperReceiverCompanyDto company = (await service.SearchForAsync(c => c.CompanyId == load.CompanyId && (c.EntityId == load.ReceiverId || c.Name == load.ReceiverCompany)))
                .FirstOrDefault() ?? new ShipperReceiverCompanyDto();
            if (company.Name != load.ReceiverCompany || company.Contacts != load.ReceiverContacts) {
                company.Name = load.ReceiverCompany;
                company.Contacts = load.ReceiverContacts;
                company.CompanyId = load.CompanyId;
                await service.SaveAsync(company, userId);
            }
            load.ReceiverId = company.EntityId;
            LoadCompanyNames.RemoveCompany(company.EntityId);
        }
    }

    // Save history without location
    private async Task<TEntity> BaseSaveAsync<TDto, TEntity>(Guid userId, TDto dto, Guid? groupId = null)
        where TDto : BaseObjectDto, IBaseEventDto where TEntity : BaseObjectEntity {
        if (dto.CompanyId == Guid.Empty) {
            dto.CompanyId = await this.GetCompanyIdByUserIdAsync(userId);
        }
        if (dto is LoadDto lDto) {
            await SaveCompaniesAsync(lDto, userId);
        }
        return await this.SaveWithHistoryAsync<TDto, TEntity>(dto, userId, groupId);
    }

    private async Task<List<TEntity>> BaseSavesAsync<TDto, TEntity>(Guid userId, List<TDto> dtos, Guid? groupId = null)
        where TDto : BaseObjectDto, IBaseEventDto where TEntity : BaseObjectEntity {
        foreach (var dto in dtos) {
            if (dto.CompanyId == Guid.Empty) {
                dto.CompanyId = await this.GetCompanyIdByUserIdAsync(userId);
            }
            if (dto is LoadDto lDto) {
                await SaveCompaniesAsync(lDto, userId);
            }
        }
        return await this.SaveWithHistoriesAsync<TDto, TEntity>(dtos, userId, groupId);
    }

    private Task<List<TEntity>> BaseDeletesAsync<TDto, TEntity>(Guid userId, List<TDto> dtos, Guid? groupId = null)
        where TDto : BaseObjectDto, IBaseEventDto where TEntity : BaseObjectEntity {
        return this.DeleteWithHistoriesAsync<TDto, TEntity>(dtos, userId, groupId);
    }

    /// <summary>
    ///     Снимок DTO в HistoryObject + вставка/полное обновление сущности с HistoryId (V1 SaveWithHistoryAsync) </summary>
    public async Task<TEntity> SaveWithHistoryAsync<TDto, TEntity>(TDto dto, Guid userId, Guid? groupId = null)
        where TDto : BaseObjectDto
        where TEntity : BaseObjectEntity {
        #region Save HistoryObject
        var historyObject = new HistoryObject {
            ReferenceId = dto.EntityId,
            GroupId = groupId,
            CreatedDate = DateTime.UtcNow,
            Data = ZipExtension.SerializeObject(dto) // optional
        };
        historyObject.Id = await Db.InsertWithInt32IdentityAsync(historyObject);
        #endregion
        #region Modify Entity
        var @new = false;
        TEntity entity = await GetByIdAsync<TEntity>(dto.EntityId);
        if (entity == null) {
            @new = true;
            entity = this.Mapper.Map<TEntity>(dto);
            dto.CreatedById = entity.CreatedById = userId;
            dto.CreatedDate = entity.CreatedDate = DateTime.UtcNow;
            dto.LastModifiedById = entity.LastModifiedById = userId;
            dto.LastModifiedDate = entity.LastModifiedDate = DateTime.UtcNow;
            dto.HistoryId = entity.HistoryId = historyObject.Id;
            if (entity.CompanyId == Guid.Empty)
                entity.CompanyId = dto.CompanyId;
            SetSms(entity, @new);
            await InsertAsync(entity);
        } else {
            TEntity entity2 = this.Mapper.Map<TEntity>(dto);
            entity2.CreatedById = entity.CreatedById;
            entity2.CreatedDate = entity.CreatedDate;
            _saveWithHistory_SpecialUpdate(entity, entity2, userId);
            entity = entity2;
            dto.LastModifiedById = entity.LastModifiedById = userId;
            dto.LastModifiedDate = entity.LastModifiedDate = DateTime.UtcNow;
            dto.HistoryId = entity.HistoryId = historyObject.Id;
            if (entity.CompanyId == Guid.Empty)
                entity.CompanyId = dto.CompanyId;
            SetSms(entity, @new);
            await UpdateAsync(entity);
        }
        #endregion
        #region Correct HistoryObject and Save both
        historyObject.Data = ZipExtension.SerializeObject(Mapper.Map<TDto>(entity));
        await UpdateAsync(historyObject);
        #endregion
        return entity;
    }

    public async Task<List<TEntity>> SaveWithHistoriesAsync<TDto, TEntity>(List<TDto> dtos, Guid userId, Guid? groupId = null)
        where TDto : BaseObjectDto
        where TEntity : BaseObjectEntity {
        var result = new List<TEntity>();
        foreach (TDto dto in dtos) {
            result.Add(await SaveWithHistoryAsync<TDto, TEntity>(dto, userId, groupId));
        }
        return result;
    }

    public async Task<List<TEntity>> DeleteWithHistoriesAsync<TDto, TEntity>(List<TDto> dtos, Guid userId, Guid? groupId = null)
        where TDto : BaseObjectDto
        where TEntity : BaseObjectEntity {
        var result = new List<TEntity>();
        foreach (var dto in dtos) {
            TEntity entity = await GetByIdAsync<TEntity>(dto.EntityId);
            if (entity != null) {
                dto.LastModifiedById = entity.LastModifiedById = userId;
                dto.LastModifiedDate = entity.LastModifiedDate = DateTime.UtcNow;
                dto.IsDeleted = entity.IsDeleted = true;
                var historyObject = new HistoryObject {
                    ReferenceId = dto.EntityId,
                    GroupId = groupId,
                    CreatedDate = DateTime.UtcNow,
                    Data = ZipExtension.SerializeObject(dto) // optional
                };
                historyObject.Id = await Db.InsertWithInt32IdentityAsync(historyObject);
                entity.HistoryId = historyObject.Id;
                await UpdateAsync(entity); // saves dates, delete flag and history
                result.Add(entity);
            }
        }
        return result;
    }

    /// <summary>
    ///     SMS складу о новом грузе из WA (одна компания, V1 захардкожено) — отправляет LoadSendSmsService </summary>
    private static void SetSms(object entity, bool @new) {
        if (entity is Load load) {
            if (@new &&
                load.ParentId == null &&
                load.PickupFrom.Date == DateTime.Now.Date &&
                load.OriginCity.Contains("WA") &&
                load.CompanyId == AppSettings.LoadSendSmsCompanyId) {
                load.SendSms = true;
            }
        }
    }

    private static void _saveWithHistory_SpecialUpdate(BaseObjectEntity old, BaseObjectEntity @new, Guid userId) {
        if (@new.CreatedById == Guid.Empty) {
            @new.CreatedById = userId;
        }
        // restore location
        if (@new is BaseDispatchEntity newDispatch && old is BaseDispatchEntity oldDispatch) {
            newDispatch.CurrentLatitude = oldDispatch.CurrentLatitude;
            newDispatch.CurrentLongitude = oldDispatch.CurrentLongitude;
        }
        // saves old value.
        if (@new is Load newLoad && old is Load oldLoad) {
            newLoad.Submited = oldLoad.Submited;
            if (newLoad.Status != oldLoad.Status) {
                newLoad.LastStatusChangedDate = DateTime.UtcNow;
            }
            // a date of changed rate
            if (newLoad.DatBrokerId != null || newLoad.DatCompanyId != null || newLoad.HistoryDatCompanyId != null) {
                // a company was changed
                if (newLoad.DatBrokerId != oldLoad.DatBrokerId || newLoad.DatCompanyId != oldLoad.DatCompanyId ||
                    newLoad.HistoryDatCompanyId != oldLoad.HistoryDatCompanyId) {
                    // resets a date
                    newLoad.LastRateChanged = null;
                } else if (DispatchHelper.GetRate2(newLoad) != DispatchHelper.GetRate2(oldLoad)) {
                    // a rate was changed
                    unchecked {
                        newLoad.LastRateChanged = (byte)(DispatchHelper.GetRate2(newLoad) > DispatchHelper.GetRate2(oldLoad) ? 1 : -1);
                    }
                }
            } else {
                newLoad.LastRateChanged = null;
            }
        }
    }

    #endregion [ Save ]

    #region [ Delete ]

    public async Task<DeleteDriverResponseData> DeleteDriver(DeleteDriverRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<DriverDto, Driver>(request.DriverId, request.UserId);
        await this._saveEventHistoryDriverAsync(EventTypeModel.Driver, entity, null, request.UserId);
        return new DeleteDriverResponseData { Driver = dto };
    }

    public async Task<DeleteTruckResponseData> DeleteTruck(DeleteTruckRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<TruckDto, Truck>(request.TruckId, request.UserId);
        await DeleteDefectsAsync(request.TruckId, null, request.UserId);
        await this._saveEventHistoryTruckAsync(EventTypeModel.Truck, entity, null, request.UserId);
        return new DeleteTruckResponseData { Truck = dto };
    }

    public async Task<DeleteTrailerResponseData> DeleteTrailer(DeleteTrailerRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<TrailerDto, Trailer>(request.TrailerId, request.UserId);
        await DeleteDefectsAsync(null, request.TrailerId, request.UserId);
        await this._saveEventHistoryTrailerAsync(EventTypeModel.Trailer, entity, request.UserId);
        return new DeleteTrailerResponseData { Trailer = dto };
    }

    public async Task<DeleteWarehouseResponseData> DeleteWarehouse(DeleteWarehouseRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<WarehouseDto, Warehouse>(request.WarehouseId, request.UserId);
        await this._saveEventWarehouseAsync(entity, request.UserId);
        return new DeleteWarehouseResponseData { Warehouse = dto };
    }

    public async Task<DeleteYardResponseData> DeleteYard(DeleteYardRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<YardDto, Yard>(request.YardId, request.UserId);
        await this._saveEventYardAsync(entity, request.UserId);
        return new DeleteYardResponseData { Yard = dto };
    }

    public async Task<DeleteLoadResponseData> DeleteLoad(DeleteLoadRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<LoadDto, Load>(request.LoadId, request.UserId);
        await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, entity, null, LoadParentType.None, request.UserId);
        if (dto.ContainerType != ContainerType.None) {
            //changing status for container
            await DeleteChildrenAsync(dto.EntityId, request.UserId);
        }
        return new DeleteLoadResponseData { Load = dto };
    }

    public async Task<DeleteLoadsResponseData> DeleteLoads(DeleteLoadsRequestData request) {
        var (entities, dtos) = await BaseDeletesAsync<LoadDto, Load>(request.LoadIds, request.UserId);
        await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad, entities.ToDictionary(e => e, e => (int?)null), LoadParentType.None, request.UserId);
        foreach (LoadDto dto in dtos) {
            if (dto.ContainerType != ContainerType.None) {
                // changing status for container
                await DeleteChildrenAsync(dto.EntityId, request.UserId);
            }
            if (dto.ParentId.HasValue) {
                await UpdateEmptyContainerAsync(dto.ParentId.Value, request.UserId);
            }
        }
        return new DeleteLoadsResponseData { Loads = dtos };
    }

    /// <summary>
    ///     If container doesn't have children then makes parent as not container </summary>
    private async Task UpdateEmptyContainerAsync(Guid parentId, Guid userId) {
        var anyChildren = await Table<Load>().AnyAsync(e => (e.ParentId == parentId || e.ShipmentId == parentId) && e.IsDeleted == false);
        if (anyChildren == false) {
            Load parent = await GetByIdAsync<Load>(parentId);
            parent.ContainerType = (byte)ContainerType.None;
            var dto = Mapper.Map<LoadDto>(parent);
            Load entity = await BaseSaveAsync<LoadDto, Load>(userId, dto);
            await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, entity, null, LoadParentType.None, userId);
        }
    }

    /// <summary>
    ///     Deletes children if container was deleted </summary>
    private async Task DeleteChildrenAsync(Guid parentId, Guid userId) {
        var children = await Alive<Load>().Where(e => e.ParentId == parentId).ToListAsync();
        if (children.Any()) {
            foreach (var child in children) {
                await DeleteChildrenAsync(child.EntityId, userId);
            }
            var (entities, _) = await BaseDeletesAsync<LoadDto, Load>(children.Select(l => l.EntityId).ToList(), userId);
            await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad, entities.ToDictionary(e => e, e => (int?)null), LoadParentType.None, userId);
        }
    }

    public async Task<DeleteSetupPacketResponseData> DeleteSetupPacket(DeleteSetupPacketRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<SetupPacketDto, SetupPacket>(request.SetupPacketId, request.UserId);
        await this._saveEventSetupPacketAsync(entity, request.UserId);
        return new DeleteSetupPacketResponseData { SetupPacket = dto };
    }

    public async Task<DeleteDatCompanyResponseData> DeleteDatCompany(DeleteDatCompanyRequestData request) {
        var existsLoads = await Table<Load>().AnyAsync(l => l.DatCompanyId == request.DatCompanyId);
        if (existsLoads) {
            throw new InfoException("You can not remove company because it has loads");
        }
        var (dto, entity) = await BaseDeleteAsync<DatCompanyDto, DatCompany>(request.DatCompanyId, request.UserId);
        await this._saveEventDatCompanyAsync(entity, request.UserId);
        return new DeleteDatCompanyResponseData { DatCompany = dto };
    }

    public async Task<DeleteDatDriverResponseData> DeleteDatDriver(DeleteDatDriverRequestData request) {
        var (dto, entity) = await BaseDeleteAsync<DatDriverDto, DatDriver>(request.DatDriverId, request.UserId);
        await this._saveEventHistoryDatDriverAsync(EventTypeModel.DatDriver, entity, request.UserId);
        return new DeleteDatDriverResponseData { DatDriver = dto };
    }

    private async Task<(TDto dto, TEntity entity)> BaseDeleteAsync<TDto, TEntity>(Guid entityId, Guid userId)
        where TDto : BaseObjectDto, IBaseEventDto where TEntity : BaseObjectEntity {
        var entity = await GetByIdAsync<TEntity>(entityId);
        if (entity == null) {
            throw new InfoException($"{typeof(TEntity).Name} was not found: {entityId}");
        }
        entity.IsDeleted = true;
        var dto = Mapper.Map<TDto>(entity);
        entity = await this.SaveWithHistoryAsync<TDto, TEntity>(dto, userId);
        return (dto, entity);
    }

    private async Task<(List<TEntity> entities, List<TDto> dtos)> BaseDeletesAsync<TDto, TEntity>(List<Guid> entityIds, Guid userId)
        where TDto : BaseObjectDto, IBaseEventDto where TEntity : BaseObjectEntity {
        var entities = await Alive<TEntity>().Where(e => entityIds.Contains(e.EntityId)).ToListAsync();
        foreach (var entity in entities) {
            entity.IsDeleted = true;
        }
        var dtos = Mapper.Map<List<TDto>>(entities);
        entities = await this.SaveWithHistoriesAsync<TDto, TEntity>(dtos, userId);
        return (entities, dtos);
    }

    #endregion [ Delete ]
}
