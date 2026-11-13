using LinqToDB;
using LinqToDB.Async;
using Newtonsoft.Json;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Common.Helpers;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Utils;
using AutoDispatch.Web.Logic.WebServices;
using AutoDispatch.Web.Logic.WebServices.Bnsf;
using AutoDispatch.Web.Logic.WebServices.CarrierView;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    #region [ CarrierView Tracker ]

    private static string CombineErrors(IDictionary<string, string[]> errors) {
        try {
            if (errors == null) {
                return "err null";
            }
            return String.Join("\n", errors.Select(e => $"{e.Key}: {String.Join(". ", e.Value)}"));
        } catch (Exception) {
            return DispatchJson.Serialize(errors, TypeNameHandling.None);
        }
    }

    public async Task<EnableCarrierViewTrackerResponseData> EnableCarrierViewTracker(EnableCarrierViewTrackerRequestData request) {
        Load load = await GetByIdAsync<Load>(request.LoadId);
        var actualCompany = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);
        var shipper = await GetByIdAsync<ShipperReceiverCompany>(load.ShipperId);
        var receiver = await GetByIdAsync<ShipperReceiverCompany>(load.ReceiverId);
        var datCompany = await GetByIdAsync<DatCompany>(load.DatCompanyId);

        if (actualCompany?.CarrierViewToken == null) {
            throw new InfoException("Undefined CarrierView Token");
        }
        if (request.Phone == null) {
            throw new InfoException("Undefined Phone");
        }
        var dataRequest = new CreateLoadRequest() {
            driver_phone = "+1" + request.Phone,
            location_type = "single",
            load_id = $"{load.Index}{load.IndexExtra}",
            locations = new CreateLocationRequest[2] {
                new CreateLocationRequest() {
                    address = $"{load.OriginAddress}, {load.OriginCity}",
                    company = shipper?.Name ?? "Undefined",
                    type = "pickup",
                    dateFrom = load.PickupFrom.ToString("yyyy-MM-dd HH:mm"),
                    dateTo = load.PickupTo.ToString("yyyy-MM-dd HH:mm"),
                },
                new CreateLocationRequest() {
                    address = $"{load.DestinationAddress}, {load.DestinationCity}",
                    company = receiver?.Name ?? "Undefined",
                    type = "destination",
                    dateFrom = load.DeliveryFrom.ToString("yyyy-MM-dd HH:mm"),
                    dateTo = load.DeliveryTo.ToString("yyyy-MM-dd HH:mm"),
                }
            }
        };
        if (load.EmailConfirmationSent != null) {
            dataRequest.emails = DispatchJson.Deserialize<List<string>>(load.EmailConfirmationSent).ToArray();
        }
        if (datCompany?.McNumber != null) {
            dataRequest.mc_number = datCompany.McNumber.ToString();
        }
        var response = await Host.GetService<CarrierViewWebService>().CreateLoadAsync(actualCompany.CarrierViewToken, dataRequest);
        if (response.Success == false) {
            throw new InfoException(response.ErrorMessage);
        }
        if (response.Data.success == false) {
            throw new InfoException(CombineErrors(response.Data.errors));
        }
        // Saves tracker Id
        load.TrackerId = response.Data.data.id;
        await Table<Load>().Where(e => e.EntityId == load.EntityId).Set(e => e.TrackerId, load.TrackerId).UpdateAsync();

        var dto = Mapper.Map<LoadDto>(load);
        // V1 ставил CarrierViewLate/Link/Comments/StatusesJson на отсоединённую сущность — в БД они не попадали; их пишет SyncCarrierViewTracker
        dto.CarrierViewPhone = response.Data.data.driver_phone;
        dto.TrackerId = response.Data.data.id;
        if (response.Data.data.locations.Length >= 2) {
            dto.CarrierViewPickupId = response.Data.data.locations[0].id;
            dto.CarrierViewDeliveryId = response.Data.data.locations[1].id;
        } else {
            _l.w($"EnableCarrierViewTracker: dataResponse.Data.data.locations.Length is '{response.Data.data.locations.Length}'");
        }
        var r = await this.SaveLoad(new SaveLoadRequestData(request.UserId) { Load = dto });
        return new EnableCarrierViewTrackerResponseData() { Load = r.Load };
    }

    public async Task<DisableCarrierViewTrackerResponseData> DisableCarrierViewTracker(DisableCarrierViewTrackerRequestData request) {
        Load load = await GetByIdAsync<Load>(request.LoadId);
        if (load.TrackerId == null) {
            throw new InfoException("Undefined CarrierView Id");
        }
        var actualCompany = await GetByIdAsync<DispatchCompany>(load.DispatchCompanyId);
        if (actualCompany?.CarrierViewToken == null) {
            throw new InfoException("Undefined CarrierView Token");
        }
        // DETACH + DISABLE
        var dataResponse = await Host.GetService<CarrierViewWebService>().DeleteLoadAsync(actualCompany.CarrierViewToken, new DeleteLoadRequest() { Id = load.TrackerId.Value });
        if (dataResponse.Success == false) {
            throw new InfoException(dataResponse.ErrorMessage);
        }
        if (dataResponse.Data.success == false) {
            throw new InfoException(CombineErrors(dataResponse.Data.errors));
        }
        var dto = Mapper.Map<LoadDto>(load);
        dto.TrackerId = null;
        dto.CarrierViewPhone = null;
        dto.CarrierViewLink = null;
        dto.CarrierViewLastUpdatedDate = null;
        dto.CarrierViewStatus = null;
        dto.CarrierViewStatusesJson = null;
        dto.CarrierViewLate = false;
        dto.CarrierViewPickupId = null;
        dto.CarrierViewDeliveryId = null;
        dto.CarrierViewPickupComment = null;
        dto.CarrierViewDeliveryComment = null;
        var response = await this.SaveLoad(new SaveLoadRequestData(request.UserId) { Load = dto });
        return new DisableCarrierViewTrackerResponseData() { Load = response.Load };
    }

    public async Task<SyncCarrierViewTrackerResponseData> SyncCarrierViewTracker(SyncCarrierViewTrackerRequestData request) {
        var webService = Host.GetService<CarrierViewWebService>();
        LoadDto load = await GetLoadByTrackerIdAsync(request.TrakerId);
        if (load == null) {
            throw new InfoException("Load was not found");
        }
        var actualCompany = await GetByIdAsync<DispatchCompany>(load.ActualCompanyId);
        if (actualCompany?.CarrierViewToken == null) {
            throw new InfoException("Undefined CarrierView Token");
        }
        var response = await webService.GetLoadAsync(actualCompany.CarrierViewToken, request.TrakerId);
        var response2 = await webService.GetPositionAsync(actualCompany.CarrierViewToken, request.TrakerId);
        _l.i("response: " + DispatchJson.Serialize(response, TypeNameHandling.None));
        _l.i("response2: " + DispatchJson.Serialize(response2, TypeNameHandling.None));
        if (response.Success && response2.Success) {
            if (response.Data.success == true && response2.Data.success) {
                load.CarrierViewStatus = response2.Data.data.position?.type;
                load.CarrierViewLate = response.Data.data.driver_is_late;
                load.CarrierViewPhone = response.Data.data.driver_phone;
                load.CarrierViewLink = response.Data.data.client_url;
                load.CarrierViewPickupComment = response.Data.data.locations.First().comment;
                load.CarrierViewDeliveryComment = response.Data.data.locations.Last().comment;
                load.CarrierViewStatusesJson = DispatchJson.Serialize(response.Data.data.statuses, TypeNameHandling.None);
                await this.LocationLoadChanged(new LocationLoadChangedRequestData(AdUser.SystemUserId) {
                    LoadId = load.EntityId,
                    Location = null,
                    DateTime = DateTime.UtcNow
                });
                await this.SaveLoad(new SaveLoadRequestData(AdUser.SystemUserId) { Load = load });
                return new SyncCarrierViewTrackerResponseData() { TrackerId = response.Data.data.id };
            } else if (response.Data.error_code == "load_not_found") {
                load.CarrierViewLate = false;
                load.TrackerId = null;
                load.CarrierViewStatus = null;
                load.CarrierViewLink = null;
                load.CarrierViewStatusesJson = null;
                load.CarrierViewLastUpdatedDate = null;
                load.CarrierViewPickupComment = null;
                load.CarrierViewDeliveryComment = null;
                await this.SaveLoad(new SaveLoadRequestData(AdUser.SystemUserId) { Load = load });
                return new SyncCarrierViewTrackerResponseData() { TrackerId = null };
            } else {
                var err = CombineErrors(response.Data?.errors ?? response2.Data?.errors);
                _l.e("SyncCarrierViewTracker\n" + err);
                throw new InfoException(err);
            }
        }
        _l.e("SyncCarrierViewTracker\n" + (response.ErrorMessage ?? response2.ErrorMessage));
        throw new InfoException(response.ErrorMessage ?? response2.ErrorMessage);
    }

    public async Task<LoadDto> GetLoadByTrackerIdAsync(int id) {
        var model = await Alive<Load>().FirstOrDefaultAsync(e => e.TrackerId == id);
        return Mapper.Map<LoadDto>(model);
    }

    #endregion [ CarrierView Tracker ]

    #region [ jobs ]

    /// <summary>
    ///     When status is 'at pickup' or 'at delivery' sets waiting in 2 hour (V1 LoadWaitingJob) </summary>
    public async Task LoadSetWaitingStatusAsync() {
        var status1 = (byte)LoadStatus.AtPickUp;
        var status2 = (byte)LoadStatus.AtDelivery;
        var now = DateTime.UtcNow;
#if DEBUG
        var _1hour = TimeSpan.FromMinutes(1);
        var _2hour = TimeSpan.FromMinutes(2);
#else
        var _1hour = TimeSpan.FromHours(1);
        var _2hour = TimeSpan.FromHours(2);
#endif
        var loadEntities = (await Alive<Load>().Where(e => e.Board == 0 && (e.Status == status1 || e.Status == status2)).ToListAsync())
            .Where(e => (e.WasAtPickup == false && now - e.LastStatusChangedDate > _2hour && e.Status == status1) ||
                        (e.WasAtPickup == true && now - e.LastStatusChangedDate > _1hour && e.Status == status1) ||
                        (e.WasAtDelivery == false && now - e.LastStatusChangedDate > _2hour && e.Status == status2) ||
                        (e.WasAtDelivery == true && now - e.LastStatusChangedDate > _1hour && e.Status == status2)).ToList();

        var loads = Mapper.Map<List<LoadDto>>(loadEntities);
        foreach (var load in loads) {
            switch (load.Status) {
                case LoadStatus.AtPickUp:
                    load.Status = LoadStatus.Waiting1;
                    load.WasAtPickup = true;
                    break;
                case LoadStatus.AtDelivery:
                    load.Status = LoadStatus.Waiting2;
                    load.WasAtDelivery = true;
                    break;
                default:
                    continue;
            }
        }
        if (loads.Count == 0) return;
        var groupId = Guid.NewGuid();
        var adminId = AdUser.SystemUserId;
        List<Load> entities = await BaseSavesAsync<LoadDto, Load>(adminId, loads, groupId);
        await this._saveEventHistoriesLoadsAsync(EventTypeModel.BookedLoad, entities.ToDictionary(e => e, e => (int?)null), LoadParentType.None, adminId);
    }

    #endregion [ jobs ]

    #region [ BNSF Railway ]

    private static string GetBnsfMessage(BnsfRailwayWebService.Report result, List<TrailerDto> trailers) {
        // Undefined trailers
        var unTrailers = new List<string>();
        // trailers in yard
        var yardTrailers = new List<string>();

        foreach (var trailer in trailers) {
            if (trailer.YardId != null) {
                yardTrailers.Add(trailer.Number);
            }
        }
        foreach (BnsfRailwayTrailerDto bnsfTrailer in result.Trailers) {
            if (trailers.Any(e => e.Number == bnsfTrailer.Unit) == false) {
                unTrailers.Add(bnsfTrailer.Unit);
            }
        }
        return String.Join("\n", new[] {
            unTrailers.Any() ? $"Trailer(s) {String.Join(", ", unTrailers)} was not found" : null,
            yardTrailers.Any() ? $"Trailer(s) {String.Join(", ", yardTrailers)} in yard" : null,
        }.Where(e => e != null));
    }

    /// <summary>
    ///     Отчёт BNSF для компании; cookies сессии сохраняются, при ошибке — сбрасываются (V1 BnsfRailwayDataSource.SaveBnsfCookiesAsync) </summary>
    private async Task<ApiResult<BnsfRailwayWebService.Report>> GetBnsfReportAsync(DispatchCompany company) {
        var result = await Host.GetService<BnsfRailwayWebService>().GetReportAsync(company.BnsfLogin, company.BnsfPassword, company.BnsfCookies);
        var cookies = result.Success ? result.Data.Cookies : null;
        if (cookies != company.BnsfCookies) {
            company.BnsfCookies = cookies;
            await Table<DispatchCompany>().Where(e => e.EntityId == company.EntityId).Set(e => e.BnsfCookies, cookies).UpdateAsync();
        }
        return result;
    }

    public async Task<BnsfRefreshResponseData> BnsfRefresh(BnsfRefreshRequestData request) {
        try {
            var company = await GetByIdAsync<DispatchCompany>(request.ActualCompanyId);
            var result = await GetBnsfReportAsync(company);
            if (result.Success == false) {
                throw new InfoException(result.ErrorMessage);
            }
            var trailerDtos = new List<TrailerDto>();
            var trailers = Mapper.Map<List<TrailerDto>>(await Alive<Trailer>().Where(e => e.CompanyId == company.CompanyId).ToListAsync());
            foreach (TrailerDto trailerDto in trailers) {
                bool changed = false;
                var bnsfTrailer = result.Data.Trailers.FirstOrDefault(e => trailerDto.Number == e.Unit);
                if (bnsfTrailer != null) {
                    // tries to change current location
                    if (bnsfTrailer.Latitude != 0 && bnsfTrailer.Longitude != 0) {
                        if (trailerDto.CurrentLocation == null ||
                            MathHelper.LocationCompare(bnsfTrailer.Latitude, trailerDto.CurrentLocation.Latitude) == false ||
                            MathHelper.LocationCompare(bnsfTrailer.Longitude, trailerDto.CurrentLocation.Longitude) == false) {
                            await this.LocationTrailerChanged(new LocationTrailerChangedRequestData(request.UserId) {
                                DateTime = DateTime.UtcNow,
                                Location = new LocationDto() { Latitude = bnsfTrailer.Latitude, Longitude = bnsfTrailer.Longitude },
                                TrailerId = trailerDto.EntityId
                            });
                            changed = true;
                        }
                    }
                    // sets properties
                    var tmpChanged = BnsfTrailerConverter.Set(bnsfTrailer, trailerDto, out bool changePickUpNumber);
                    changed = changed || tmpChanged;
                    if (changePickUpNumber && trailerDto.SchemeFile != null && trailerDto.SchemeFile.ToLower().EndsWith(".pdf")) {
                        _l.i("DispatchPdfGenerator.StampScheme[1]");
                        DispatchPdfGenerator.StampScheme(trailerDto);
                        changed = true;
                    }
                    trailerDtos.Add(trailerDto);
                    if (changed) {
                        await this.SaveTrailer(new SaveTrailerRequestData(request.UserId) { Trailer = trailerDto, SkipDefects = true });
                    }
                } else {
                    if (BnsfTrailerConverter.IsEmpty(trailerDto) == false) {
                        BnsfTrailerConverter.SetEmpty(trailerDto);
                        trailerDtos.Add(trailerDto);
                        await this.SaveTrailer(new SaveTrailerRequestData(request.UserId) { Trailer = trailerDto, SkipDefects = true });
                    }
                }
            }

            var message = GetBnsfMessage(result.Data, trailers);
            if (company.BnsfLastMessage != message) {
                if (message != null) {
                    message = $"[{DateTime.Now:d}] {message}";
                }
                await Table<DispatchCompany>().Where(e => e.EntityId == company.EntityId).Set(e => e.BnsfLastMessage, message).UpdateAsync();
            }

            return new BnsfRefreshResponseData() {
                Result = new BnsfRefreshResultDto() { Trailers = trailerDtos, Message = message }
            };
        } catch (Exception e) {
            _l.e("BnsfRefresh", e);
            throw;
        }
    }

    /// <summary>
    ///     BnsfRailwayJob V1: все компании с логином BNSF </summary>
    public async Task BnsfRefreshes() {
        var companies = await Alive<DispatchCompany>().Where(e => e.BnsfLogin != null).ToListAsync();
        foreach (var company in companies) {
            var result = await GetBnsfReportAsync(company);
            if (result.Success == false) {
                continue;
            }
            await Table<DispatchCompany>().Where(e => e.EntityId == company.EntityId).Set(e => e.BnsfLastDone, DateTime.UtcNow).UpdateAsync();
            var trailerDtos = new List<TrailerDto>();
            List<string> usings = result.Data.Trailers.Select(e => e.Unit).ToList();
            foreach (var subUnits in usings.DivideBy()) {
                var trailers = Mapper.Map<List<TrailerDto>>(await Alive<Trailer>()
                    .Where(e => e.CompanyId == company.CompanyId && subUnits.Contains(e.Number) && e.History == false).ToListAsync());
                foreach (TrailerDto trailerDto in trailers) {
                    BnsfRailwayTrailerDto bnsfTrailer = result.Data.Trailers.FirstOrDefault(e => trailerDto.Number == e.Unit);
                    if (bnsfTrailer != null) {
                        if (BnsfTrailerConverter.Set(bnsfTrailer, trailerDto, out var changePickUpNumber)) {
                            trailerDtos.Add(trailerDto);
                            await this.SaveTrailer(new SaveTrailerRequestData(AdUser.SystemUserId) { Trailer = trailerDto });
                            if (changePickUpNumber && trailerDto.SchemeFile != null && trailerDto.SchemeFile.ToLower().EndsWith(".pdf")) {
                                _l.i("DispatchPdfGenerator.StampScheme[2]");
                                DispatchPdfGenerator.StampScheme(trailerDto);
                            }
                        }
                        // tries to change current location
                        if (bnsfTrailer.Latitude != 0 && bnsfTrailer.Longitude != 0) {
                            if (trailerDto.CurrentLocation == null ||
                                MathHelper.LocationCompare(bnsfTrailer.Latitude, trailerDto.CurrentLocation.Latitude) == false ||
                                MathHelper.LocationCompare(bnsfTrailer.Longitude, trailerDto.CurrentLocation.Longitude) == false) {
                                await this.LocationTrailerChanged(new LocationTrailerChangedRequestData(AdUser.SystemUserId) {
                                    DateTime = DateTime.UtcNow,
                                    Location = new LocationDto() { Latitude = bnsfTrailer.Latitude, Longitude = bnsfTrailer.Longitude },
                                    TrailerId = trailerDto.EntityId
                                });
                            }
                        }
                    } else {
                        if (BnsfTrailerConverter.IsEmpty(trailerDto) == false) {
                            BnsfTrailerConverter.SetEmpty(trailerDto);
                            trailerDtos.Add(trailerDto);
                            await this.SaveTrailer(new SaveTrailerRequestData(AdUser.SystemUserId) { Trailer = trailerDto });
                        }
                    }
                }
                var error = GetBnsfMessage(result.Data, trailers);
                string lastMessage;
                if (company.BnsfLastMessage != error) {
                    lastMessage = error;
                } else if (trailerDtos.Count == 0) {
                    lastMessage = "Zero trailers";
                } else {
                    lastMessage = null;
                }
                company.BnsfLastMessage = lastMessage;
                await Table<DispatchCompany>().Where(e => e.EntityId == company.EntityId).Set(e => e.BnsfLastMessage, lastMessage).UpdateAsync();
            }
        }
    }

    #endregion [ BNSF Railway ]
}
