using LinqToDB;
using LinqToDB.Async;
using Newtonsoft.Json;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    /// <summary>
    ///     Long-polling Api/Dispatch/Response (V1 EventNotificationAsync): ждёт события компании новее request.EventId до ReconnectInterval,
    ///     опрашивая кэш последнего Id раз в EventPollInterval; ts — отмена при новом запросе того же пользователя (этап 4).
    ///     Снимки HistoryObject — DTO с $type для клиента. Чат (Message) не переносится </summary>
    public async Task<EventNotificationResponseData> EventNotificationAsync(EventNotificationRequestData request, CancellationTokenSource ts) {
        Guid companyId = await GetCompanyIdByUserIdAsync(request.UserId);
        try {
            DateTime utcNow = DateTime.UtcNow;
            var list = new List<IBaseEventDto>();

            if (request.EventId == null) {
                return new EventNotificationResponseData {
                    Events = new List<IBaseEventDto> { new EmptyEventDto { EventId = await _getCurrentEventIdAsync(companyId) } }
                };
            }

            int currentEventId = 0;
            while (DateTime.UtcNow - utcNow < ReconnectInterval) {
                ts?.Token.ThrowIfCancellationRequested();
                if (request.EventId < (currentEventId = await _getCurrentEventIdAsync(companyId))) {
                    var types = request.EventTypes.Select(e => (byte)e).ToList();
                    var eventsList = await (
                        from e in Table<Event>()
                        join h in Table<HistoryObject>() on e.HistoryId equals h.Id into hs
                        from h in hs.DefaultIfEmpty()
                        where e.Id > request.EventId && e.CompanyId == companyId && types.Contains(e.Type)
                        orderby e.Id
                        select new { Event = e, History = h }
                    ).ToListAsync();
                    foreach (var item in eventsList) {
                        item.Event.HistoryObject = item.History;
                    }
                    var events = eventsList.Select(e => e.Event)
                        .GroupBy(e => e.Type)
                        .ToDictionary(e => (EventTypeModel)e.Key, e => e.ToList());

                    foreach (byte t in request.EventTypes) {
                        var eventTypeDto = (EventTypeDto)t;
                        var key = (EventTypeModel)t;
                        if (events.ContainsKey(key) == false) continue;
                        switch (eventTypeDto) {
                            case EventTypeDto.Message:
                                // чат не переносится (решено 2026-11-11)
                                break;
                            case EventTypeDto.BookedLoad: {
                                    foreach (Event @event in events[key]) {
                                        try {
                                            if (@event.HistoryObject?.Data?.Length > 0) {
                                                LoadDto loadDto = ZipExtension.DeserializeObject<LoadDto>(@event.HistoryObject.Data);
                                                await loadDto.IncludeCompaniesAsync(Db);
                                                loadDto.EventId = @event.Id;
                                                list.Add(loadDto);
                                            }
                                        } catch (JsonSerializationException) {
                                            _l.w($"Deleting Load Event (1): {@event.Id}");
                                            await Table<Event>().Where(e => e.Id == @event.Id).DeleteAsync();
                                            throw;
                                        }
                                    }
                                    break;
                                }
                            case EventTypeDto.Driver:
                                AddSnapshots<DriverDto>(list, events[key]);
                                break;
                            case EventTypeDto.Truck:
                                // await this.loadDefectsAsync(...) высокая нагрузка. вроде бы необязательно
                                AddSnapshots<TruckDto>(list, events[key]);
                                break;
                            case EventTypeDto.Trailer:
                                AddSnapshots<TrailerDto>(list, events[key]);
                                break;
                            case EventTypeDto.Warehouse:
                                AddSnapshots<WarehouseDto>(list, events[key]);
                                break;
                            case EventTypeDto.Yard:
                                AddSnapshots<YardDto>(list, events[key]);
                                break;
                            case EventTypeDto.SetupPacket:
                                AddSnapshots<SetupPacketDto>(list, events[key]);
                                break;
                            case EventTypeDto.DatCompany:
                                AddSnapshots<DatCompanyDto>(list, events[key]);
                                break;
                            case EventTypeDto.DatDriver:
                                AddSnapshots<DatDriverDto>(list, events[key]);
                                break;
                            case EventTypeDto.HistoryBookedLoad: {
                                    var ids = events[key].Select(e => e.Id).ToList();
                                    var histories = await Table<HistoryLocationLoad>().Where(h => ids.Contains(h.EventId)).ToListAsync();
                                    foreach (HistoryLocationLoad history in histories) {
                                        try {
                                            var @event = events[key].First(e => e.Id == history.EventId);
                                            LoadDto dto = ZipExtension.DeserializeObject<LoadDto>(@event.HistoryObject.Data);
                                            await dto.IncludeCompaniesAsync(Db);
                                            SetHistory(dto, history.Latitude, history.Longitude, history.CreatedById, history.CreatedDate, @event.Id);
                                            list.Add(dto);
                                        } catch (JsonSerializationException) {
                                            _l.w($"Deleting Load Event (2): {history.Id}");
                                            await Table<HistoryLocationLoad>().Where(e => e.Id == history.Id).DeleteAsync();
                                            throw;
                                        }
                                    }
                                    break;
                                }
                            case EventTypeDto.HistoryDriver: {
                                    var ids = events[key].Select(e => e.Id).ToList();
                                    foreach (var history in await Table<HistoryLocationDriver>().Where(h => ids.Contains(h.EventId)).ToListAsync()) {
                                        var @event = events[key].First(e => e.Id == history.EventId);
                                        DriverDto dto = ZipExtension.DeserializeObject<DriverDto>(@event.HistoryObject.Data);
                                        SetHistory(dto, history.Latitude, history.Longitude, history.CreatedById, history.CreatedDate, @event.Id);
                                        list.Add(dto);
                                    }
                                    break;
                                }
                            case EventTypeDto.HistoryTruck: {
                                    var ids = events[key].Select(e => e.Id).ToList();
                                    foreach (var history in await Table<HistoryLocationTruck>().Where(h => ids.Contains(h.EventId)).ToListAsync()) {
                                        var @event = events[key].First(e => e.Id == history.EventId);
                                        TruckDto dto = ZipExtension.DeserializeObject<TruckDto>(@event.HistoryObject.Data);
                                        SetHistory(dto, history.Latitude, history.Longitude, history.CreatedById, history.CreatedDate, @event.Id);
                                        list.Add(dto);
                                    }
                                    break;
                                }
                            case EventTypeDto.HistoryTrailer: {
                                    var ids = events[key].Select(e => e.Id).ToList();
                                    foreach (var history in await Table<HistoryLocationTrailer>().Where(h => ids.Contains(h.EventId)).ToListAsync()) {
                                        var @event = events[key].First(e => e.Id == history.EventId);
                                        TrailerDto dto = ZipExtension.DeserializeObject<TrailerDto>(@event.HistoryObject.Data);
                                        if (dto != null) {
                                            SetHistory(dto, history.Latitude, history.Longitude, history.CreatedById, history.CreatedDate, @event.Id);
                                            list.Add(dto);
                                        }
                                    }
                                    break;
                                }
                            case EventTypeDto.HistoryDatDriver: {
                                    var ids = events[key].Select(e => e.Id).ToList();
                                    foreach (var history in await Table<HistoryLocationDatDriver>().Where(h => ids.Contains(h.EventId)).ToListAsync()) {
                                        var @event = events[key].First(e => e.Id == history.EventId);
                                        DatDriverDto dto = ZipExtension.DeserializeObject<DatDriverDto>(@event.HistoryObject.Data);
                                        SetHistory(dto, history.Latitude, history.Longitude, history.CreatedById, history.CreatedDate, @event.Id);
                                        list.Add(dto);
                                    }
                                    break;
                                }
                            default:
                                throw new InfoException($"EventType was not implemented: {eventTypeDto}");
                        }
                    }
                    if (list.Any()) {
                        // Load name of location
                        foreach (IBaseEventDto baseEventDto in list) {
                            if (baseEventDto is BaseDispatchDto dispatchDto) {
                                await this.LoadNameOfLocationAsync(dispatchDto);
                            }
                        }
                        return new EventNotificationResponseData { Events = list.OrderBy(e => e.EventId).ToList() };
                    }
                }
                await Task.Delay(AppSettings.EventPollInterval, ts?.Token ?? CancellationToken.None);
            }
            return new EventNotificationResponseData {
                Events = new List<IBaseEventDto> { new EmptyEventDto { EventId = currentEventId } }
            };
        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            _l.e("EventNotificationAsync\nUserId: " + request.UserId, ex);
            throw;
        }
    }

    private static void AddSnapshots<TDto>(List<IBaseEventDto> list, List<Event> events) where TDto : class, IBaseEventDto {
        foreach (var @event in events) {
            if (@event.HistoryObject?.Data == null) continue;
            var dto = ZipExtension.DeserializeObject<TDto>(@event.HistoryObject.Data);
            dto.EventId = @event.Id;
            list.Add(dto);
        }
    }

    private static void SetHistory(BaseDispatchDto dto, decimal? latitude, decimal? longitude, Guid createdById, DateTime createdDate, int eventId) {
        dto.CurrentLocation = LocationConverter.ToDto(latitude, longitude);
        dto.LastModifiedById = createdById;
        dto.LastModifiedByName = UserNameCache.Get(createdById);
        dto.LastModifiedDate = createdDate;
        ((IBaseEventDto)dto).EventId = eventId;
    }
}
