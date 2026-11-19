using System;
using System.Collections.Generic;
using AutoCall.Common;
using AutoDispatch.Data;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic.DataServices.Wrappers;
using AutoDispatch.Logic.Managers;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.Converters;
using AutoMapper;
using AutoDispatch.Logic.ViewModels.Pages;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDispatch.Logic.Services {
    /// <summary>
    ///     Represents service for google map</summary>
    public class DispatchService {

        #region [ _fields ]

        /// <summary>
        ///     Engine </summary>
        private EngineManager _engine;
        /// <summary>
        ///     Session </summary>
        private UserSession _session => UserSession.Current;
        /// <summary>
        ///     Автоматические маппинги (BllMappingProfile); V1 — поле базового HostService </summary>
        protected readonly IMapper Mapper;

        #endregion [ _fields ]

        #region [ .ctor ]

        public DispatchService(EngineManager engine, IMapper mapper) {
            Mapper = mapper;
            _engine = engine;
            _engine.EventReceived += _engine_EventReceivedAsync;
        }

        private async Task _engine_EventReceivedAsync(EventResponse response) {
            if (response.Status == ResponseStatus.Success) {
                List<DriverViewModel> drivers = new List<DriverViewModel>();
                List<TruckViewModel> trucks = new List<TruckViewModel>();
                List<TrailerViewModel> trailers = new List<TrailerViewModel>();
                List<WarehouseViewModel> warehouses = new List<WarehouseViewModel>();
                List<YardViewModel> yards = new List<YardViewModel>();
                List<LoadViewModel> loads = new List<LoadViewModel>();
                List<DatCompanyViewModel> datCompanies = new List<DatCompanyViewModel>();
                List<DatDriverViewModel> datDrivers = new List<DatDriverViewModel>();
                List<SetupPacketViewModel> setupPackets = new List<SetupPacketViewModel>();

                foreach (IBaseEventDto baseEventDto in ((EventNotificationResponseData)response.Data).Events) {
                    if (baseEventDto is DriverDto) {
                        drivers.Add(DriverConverter.ToVm((DriverDto)baseEventDto));
                    }
                    if (baseEventDto is TruckDto) {
                        trucks.Add(TruckConverter.ToVm((TruckDto)baseEventDto));
                    }
                    if (baseEventDto is TrailerDto) {
                        trailers.Add(TrailerConverter.ToVm((TrailerDto)baseEventDto));
                    }
                    if (baseEventDto is WarehouseDto) {
                        warehouses.Add(WarehouseConverter.ToVm((WarehouseDto)baseEventDto));
                    }
                    if (baseEventDto is YardDto) {
                        yards.Add(YardConverter.ToVm((YardDto)baseEventDto));
                    }
                    if (baseEventDto is LoadDto) {
                        loads.Add(LoadConverter.ToVm((LoadDto)baseEventDto));
                    }
                    if (baseEventDto is DatCompanyDto) {
                        datCompanies.Add(DatCompanyConverter.ToVm((DatCompanyDto)baseEventDto));
                    }
                    if (baseEventDto is DatDriverDto) {
                        datDrivers.Add(DatDriverConverter.ToVm((DatDriverDto)baseEventDto));
                    }
                    if (baseEventDto is SetupPacketDto) {
                        setupPackets.Add(SetupPacketConverter.ToVm((SetupPacketDto)baseEventDto));
                    }
                }
                if (drivers.Count > 0) {
                    await DriversReceivedAsync?.Invoke(drivers);
                }
                if (trucks.Count > 0) {
                    await TrucksReceivedAsync?.Invoke(trucks);
                }
                if (trailers.Count > 0) {
                    await TrailersReceivedAsync?.Invoke(trailers);
                }
                if (warehouses.Count > 0) {
                    await WarehousesReceivedAsync?.Invoke(warehouses);
                }
                if (yards.Count > 0) {
                    await YardsReceivedAsync?.Invoke(yards);
                }
                if (loads.Count > 0) {
                    await LoadsReceivedAsync?.Invoke(loads);
                }
                if (datCompanies.Count > 0) {
                    await DatCompaniesReceivedAsync?.Invoke(datCompanies);
                }
                if (datDrivers.Count > 0) {
                    await DatDriversReceivedAsync?.Invoke(datDrivers);
                }
                if (setupPackets.Count > 0) {
                    await SetupPacketsReceivedAsync?.Invoke(setupPackets);
                }
            } else {
                _webl.e("_engine_EventReceived:\n" + response.ErrorMessage);
            }
        }
        #endregion [ .ctor ]

        #region [ Public ]

        public event DriversReceivedHandler DriversReceivedAsync;
        public event TrucksReceivedHandler TrucksReceivedAsync;
        public event TrailersReceivedHandler TrailersReceivedAsync;
        public event WarehousesReceivedHandler WarehousesReceivedAsync;
        public event YardsReceivedHandler YardsReceivedAsync;
        public event LoadsReceivedHandler LoadsReceivedAsync;
        public event DatCompaniesReceivedHandler DatCompaniesReceivedAsync;
        public event DatDriversReceivedHandler DatDriversReceivedAsync;
        public event SetupPacketsReceivedHandler SetupPacketsReceivedAsync;

        public void LocationDriverChanged(Guid driverId, LocationViewModel location, Func<ViewModelWrapper<DriverViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LocationDriverChanged),
                Data = new LocationDriverChangedRequestData(_session.UserId) {
                    DriverId = driverId,
                    DateTime = DateTime.Now,
                    Location = LocationConverter.ToDto(location)
                }
            };
            var wrapper = new EventRequestWrapper<DriverViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LocationTruckChanged(Guid truckId, LocationViewModel location, Func<ViewModelWrapper<TruckViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LocationTruckChanged),
                Data = new LocationTruckChangedRequestData(_session.UserId) {
                    TruckId = truckId,
                    DateTime = DateTime.Now,
                    Location = LocationConverter.ToDto(location)
                }
            };
            var wrapper = new EventRequestWrapper<TruckViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LocationTrailerChanged(Guid trailerId, LocationViewModel location, Func<ViewModelWrapper<TrailerViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LocationTrailerChanged),
                Data = new LocationTrailerChangedRequestData(_session.UserId) {
                    TrailerId = trailerId,
                    DateTime = DateTime.Now,
                    Location = LocationConverter.ToDto(location)
                }
            };
            var wrapper = new EventRequestWrapper<TrailerViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LocationLoadChanged(Guid loadId, LocationViewModel location, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LocationLoadChanged),
                Data = new LocationLoadChangedRequestData(_session.UserId) {
                    LoadId = loadId,
                    DateTime = DateTime.Now,
                    Location = LocationConverter.ToDto(location)
                }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LocationDatDriverChanged(Guid DatDriverId, LocationViewModel location, Func<ViewModelWrapper<DatDriverViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LocationDatDriverChanged),
                Data = new LocationDatDriverChangedRequestData(_session.UserId) {
                    DatDriverId = DatDriverId,
                    DateTime = DateTime.Now,
                    Location = LocationConverter.ToDto(location)
                }
            };
            var wrapper = new EventRequestWrapper<DatDriverViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryObjects(List<int> ids, Func<ViewModelWrapper<List<HistoryObjectViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryObjects),
                Data = new LoadHistoryObjectsRequestData(_session.UserId) {
                    Ids = ids
                }
            };
            var wrapper = new EventRequestWrapper<List<HistoryObjectViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryLocationDriver(Guid driverId, Func<ViewModelWrapper<List<HistoryLocationDriverViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryLocationDriver),
                Data = new LoadHistoryLocationDriverRequestData(_session.UserId) { DriverId = driverId }
            };
            var wrapper = new EventRequestWrapper<List<HistoryLocationDriverViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryLocationTruck(Guid truckId, Func<ViewModelWrapper<List<HistoryLocationTruckViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryLocationTruck),
                Data = new LoadHistoryLocationTruckRequestData(_session.UserId) { TruckId = truckId }
            };
            var wrapper = new EventRequestWrapper<List<HistoryLocationTruckViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryLocationTrailer(Guid trailerId, Func<ViewModelWrapper<List<HistoryLocationTrailerViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryLocationTrailer),
                Data = new LoadHistoryLocationTrailerRequestData(_session.UserId) { TrailerId = trailerId }
            };
            var wrapper = new EventRequestWrapper<List<HistoryLocationTrailerViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryLocationLoad(Guid loadId, Func<ViewModelWrapper<List<HistoryLocationLoadViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryLocationLoad),
                Data = new LoadHistoryLocationLoadRequestData(_session.UserId) { LoadId = loadId }
            };
            var wrapper = new EventRequestWrapper<List<HistoryLocationLoadViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryLocationDatDriver(Guid DatDriverId, Func<ViewModelWrapper<List<HistoryLocationDatDriverViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryLocationDatDriver),
                Data = new LoadHistoryLocationDatDriverRequestData(_session.UserId) { DatDriverId = DatDriverId }
            };
            var wrapper = new EventRequestWrapper<List<HistoryLocationDatDriverViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryChangesLoad(Guid loadId, Func<ViewModelWrapper<List<HistoryObjectViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistoryChangesLoad),
                Data = new LoadHistoryChangesLoadRequestData(_session.UserId) { LoadId = loadId }
            };
            var wrapper = new EventRequestWrapper<List<HistoryObjectViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadDrivers(Func<ViewModelWrapper<List<DriverViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadDrivers),
                Data = new LoadDriversRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<DriverViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadTrucks(Func<ViewModelWrapper<List<TruckViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadTrucks),
                Data = new LoadTrucksRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<TruckViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadTrailers(Func<ViewModelWrapper<List<TrailerViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadTrailers),
                Data = new LoadTrailersRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<TrailerViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadWarehouses(Func<ViewModelWrapper<List<WarehouseViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadWarehouses),
                Data = new LoadWarehousesRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<WarehouseViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadYards(Func<ViewModelWrapper<List<YardViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadYards),
                Data = new LoadYardsRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<YardViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadLoads(Func<ViewModelWrapper<List<LoadViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadLoads),
                Data = new LoadLoadsRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<LoadViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadLoadsZip(Func<ViewModelWrapper<List<byte[]>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadLoadsZip),
                Data = new LoadLoadsZipRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<byte[]>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryLoads(DispatchBoardType board, DateTime? begin, DateTime? end, Func<ViewModelWrapper<List<byte[]>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadLoadsZip),
                Data = new LoadLoadsRequestData(_session.UserId) {
                    Types = new DispatchBoardType[] { board },
                    HistoryBegin = begin,
                    HistoryEnd = end
                }
            };
            var wrapper = new EventRequestWrapper<List<byte[]>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadHistoryTrailers(DispatchBoardType board, DateTime? begin, DateTime? end, Func<ViewModelWrapper<List<TrailerViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadTrailers),
                Data = new LoadTrailersRequestData(_session.UserId) {
                    Types = new DispatchBoardType[] { board },
                    HistoryBegin = begin,
                    HistoryEnd = end
                }
            };
            var wrapper = new EventRequestWrapper<List<TrailerViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadTotalHistoryLoads(DispatchBoardType board, Func<ViewModelWrapper<Dictionary<DateTime, int>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadTotalHistoryLoads),
                Data = new LoadTotalHistoryLoadsRequestData(_session.UserId) {
                    Board = board
                }
            };
            var wrapper = new EventRequestWrapper<Dictionary<DateTime, int>, LoadTotalHistoryLoadsResponseData>(request, EventCompleted, viewCompleted, response => response.Histories);
            _engine.PushEvent(wrapper);
        }

        public void LoadDatCompanies(Func<ViewModelWrapper<List<DatCompanyViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadDatCompanies),
                Data = new LoadDatCompaniesRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<DatCompanyViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadDatDrivers(Func<ViewModelWrapper<List<DatDriverViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadDatDrivers),
                Data = new LoadDatDriversRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<DatDriverViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadSetupPackets(Func<ViewModelWrapper<List<SetupPacketViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadSetupPackets),
                Data = new LoadSetupPacketsRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<SetupPacketViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadShipperReceiverCompanies(Func<ViewModelWrapper<List<ShipperReceiverCompanyViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadShipperReceiverCompanies),
                Data = new LoadShipperReceiverCompaniesRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<ShipperReceiverCompanyViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void UndoPreview<TViewModel>(Guid entityId, EventTypeModel type, Func<ViewModelWrapper<TViewModel>, Task> viewCompleted) where TViewModel : BaseObjectViewModel {
            var request = new EventRequest() {
                Name = nameof(UndoPreview),
                Data = new UndoPreviewRequestData(_session.UserId) {
                    EntityId = entityId,
                    Type = type
                }
            };
            var wrapper = new EventRequestWrapper<TViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void UndoApply<TViewModel>(Guid entityId, EventTypeModel type, Func<ViewModelWrapper<TViewModel>, Task> viewCompleted) where TViewModel : BaseObjectViewModel {
            var request = new EventRequest() {
                Name = nameof(UndoApply),
                Data = new UndoApplyRequestData(_session.UserId) {
                    EntityId = entityId,
                    Type = type
                }
            };
            var wrapper = new EventRequestWrapper<TViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void ConfirmationSend(Guid loadId, List<string> emails, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(ConfirmationSend),
                Data = new ConfirmationSendRequestData(_session.UserId) {
                    LoadId = loadId,
                    Emails = emails,
                }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void ConfirmedSend(Guid loadId, List<string> emails, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(ConfirmedSend),
                Data = new ConfirmedSendRequestData(_session.UserId) {
                    LoadId = loadId,
                    Emails = emails
                }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadCustomerCompanies(Func<ViewModelWrapper<List<CustomerCompanyViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadCustomerCompanies),
                Data = new LoadCustomerCompaniesRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<List<CustomerCompanyViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveDriver(DriverViewModel viewModel, Func<ViewModelWrapper<DriverViewModel>, Task> viewCompleted) {
            var dto = DriverConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveDriver),
                Data = new SaveDriverRequestData(_session.UserId) { Driver = dto }
            };
            var wrapper = new EventRequestWrapper<DriverViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveTruck(TruckViewModel viewModel, Func<ViewModelWrapper<TruckViewModel>, Task> viewCompleted) {
            var dto = TruckConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveTruck),
                Data = new SaveTruckRequestData(_session.UserId) { Truck = dto }
            };
            var wrapper = new EventRequestWrapper<TruckViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveTrailer(TrailerViewModel viewModel, Func<ViewModelWrapper<TrailerViewModel>, Task> viewCompleted) {
            var dto = TrailerConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveTrailer),
                Data = new SaveTrailerRequestData(_session.UserId) { Trailer = dto }
            };
            var wrapper = new EventRequestWrapper<TrailerViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveWarehouse(WarehouseViewModel viewModel, Func<ViewModelWrapper<WarehouseViewModel>, Task> viewCompleted) {
            var dto = WarehouseConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveWarehouse),
                Data = new SaveWarehouseRequestData(_session.UserId) { Warehouse = dto }
            };
            var wrapper = new EventRequestWrapper<WarehouseViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveYard(YardViewModel viewModel, Func<ViewModelWrapper<YardViewModel>, Task> viewCompleted) {
            var dto = YardConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveYard),
                Data = new SaveYardRequestData(_session.UserId) { Yard = dto }
            };
            var wrapper = new EventRequestWrapper<YardViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveSetupPacket(SetupPacketViewModel viewModel, Func<ViewModelWrapper<SetupPacketViewModel>, Task> viewCompleted) {
            var dto = SetupPacketConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveSetupPacket),
                Data = new SaveSetupPacketRequestData(_session.UserId) { SetupPacket = dto }
            };
            var wrapper = new EventRequestWrapper<SetupPacketViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveLoad(LoadViewModel viewModel, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var dto = LoadConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveLoad),
                Data = new SaveLoadRequestData(_session.UserId) { Load = dto }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
            this.SetSavingFlag(viewModel, true);
        }

        public void FixLoad(Guid loadId, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(FixLoad),
                Data = new FixLoadRequestData(_session.UserId) { LoadId = loadId }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveLoads(List<LoadViewModel> viewModels, Func<ViewModelWrapper<List<LoadViewModel>>, Task> viewCompleted) {
            var dtos = viewModels.Select(LoadConverter.ToDto).ToList();
            var request = new EventRequest() {
                Name = nameof(SaveLoads),
                Data = new SaveLoadsRequestData(_session.UserId) { Loads = dtos }
            };
            var wrapper = new EventRequestWrapper<List<LoadViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
            this.SetSavingFlag(viewModels, true);

        }

        public void SaveDatCompany(DatCompanyViewModel viewModel, Func<ViewModelWrapper<DatCompanyViewModel>, Task> viewCompleted) {
            var dto = DatCompanyConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveDatCompany),
                Data = new SaveDatCompanyRequestData(_session.UserId) { DatCompany = dto }
            };
            var wrapper = new EventRequestWrapper<DatCompanyViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveDatDriver(DatDriverViewModel viewModel, Func<ViewModelWrapper<DatDriverViewModel>, Task> viewCompleted) {
            var dto = DatDriverConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveDatDriver),
                Data = new SaveDatDriverRequestData(_session.UserId) { DatDriver = dto }
            };
            var wrapper = new EventRequestWrapper<DatDriverViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveInvoice(LoadViewModel viewModel, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var dto = LoadConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(SaveLoad),
                Data = new SaveLoadRequestData(_session.UserId) { Load = dto, SaveInvoice = true }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
            this.SetSavingFlag(viewModel, true);
        }

        public void SaveFactoringCompany(string json, Guid actualCompanyId, Func<ViewModelWrapper<string>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(SaveFactoringCompany),
                Data = new SaveFactoringCompanyRequestData(_session.UserId) {
                    FactoringCompanyJson = json,
                    ActualCompanyId = actualCompanyId
                }
            };
            var wrapper = new EventRequestWrapper<string>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void InvoiceSend(Guid loadId,
            List<string> emails,
            string factoringCompanyEmail,
            bool sendInvoice,
            bool sendPOD,
            bool sendConfirmation,
            bool sendLumper,
            bool merged,
            string text,
            Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(InvoiceSend),
                Data = new InvoiceSendRequestData(_session.UserId) {
                    LoadId = loadId,
                    SendInvoice = sendInvoice,
                    SendPOD = sendPOD,
                    SendConfirmation = sendConfirmation,
                    SendLumper = sendLumper,
                    Merged = merged,
                    Text = text,
                    Emails = emails,
                    FactoringCompanyEmail = factoringCompanyEmail
                }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void InvoiceMerge(Guid loadId, bool sendInvoice, bool sendPOD, bool sendConfirmation, bool sendLumper, Func<ViewModelWrapper<FileViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(InvoiceMerge),
                Data = new InvoiceMergeRequestData(_session.UserId) {
                    LoadId = loadId,
                    SendInvoice = sendInvoice,
                    SendPOD = sendPOD,
                    SendConfirmation = sendConfirmation,
                    SendLumper = sendLumper,
                }
            };
            var wrapper = new EventRequestWrapper<FileViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteDriver(Guid driverId, Func<ViewModelWrapper<DriverViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteDriver),
                Data = new DeleteDriverRequestData(_session.UserId) { DriverId = driverId }
            };
            var wrapper = new EventRequestWrapper<DriverViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteTruck(Guid truckId, Func<ViewModelWrapper<TruckViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteTruck),
                Data = new DeleteTruckRequestData(_session.UserId) { TruckId = truckId }
            };
            var wrapper = new EventRequestWrapper<TruckViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteTrailer(Guid trailerId, Func<ViewModelWrapper<TrailerViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteTrailer),
                Data = new DeleteTrailerRequestData(_session.UserId) { TrailerId = trailerId }
            };
            var wrapper = new EventRequestWrapper<TrailerViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteWarehouse(Guid warehouseId, Func<ViewModelWrapper<WarehouseViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteWarehouse),
                Data = new DeleteWarehouseRequestData(_session.UserId) { WarehouseId = warehouseId }
            };
            var wrapper = new EventRequestWrapper<WarehouseViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteYard(Guid warehouseId, Func<ViewModelWrapper<YardViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteYard),
                Data = new DeleteYardRequestData(_session.UserId) { YardId = warehouseId }
            };
            var wrapper = new EventRequestWrapper<YardViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteLoad(Guid loadId, Func<ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteLoad),
                Data = new DeleteLoadRequestData(_session.UserId) { LoadId = loadId }
            };
            var wrapper = new EventRequestWrapper<LoadViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteLoads(List<Guid> loadIds, Func<ViewModelWrapper<List<LoadViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteLoads),
                Data = new DeleteLoadsRequestData(_session.UserId) { LoadIds = loadIds }
            };
            var wrapper = new EventRequestWrapper<List<LoadViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteDatCompany(Guid companyId, Func<ViewModelWrapper<DatCompanyViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteDatCompany),
                Data = new DeleteDatCompanyRequestData(_session.UserId) { DatCompanyId = companyId }
            };
            var wrapper = new EventRequestWrapper<DatCompanyViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteDatDriver(Guid DatDriverId, Func<ViewModelWrapper<DatDriverViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteDatDriver),
                Data = new DeleteDatDriverRequestData(_session.UserId) { DatDriverId = DatDriverId }
            };
            var wrapper = new EventRequestWrapper<DatDriverViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        // Не переносится (решено 2026-11-13, AI/wiki/plan.md «ReportSimilarLoads»): единственный вызов — SimilarLoadsDialog лоадборда AutoCall,
        // на сервере AutoDispatch метода нет
        //public void ReportSimilarLoads(SimilarLoadDto load, Func<ViewModelWrapper<List<SimilarLoadDto>>, Task> viewCompleted) {
        //    var request = new EventRequest() {
        //        Name = nameof(ReportSimilarLoads),
        //        Data = new ReportSimilarLoadsRequestData(_session.UserId) { Load = load }
        //    };
        //    var wrapper = new EventRequestWrapper<List<SimilarLoadDto>, ReportSimilarLoadsResponseData>(
        //        request,
        //        EventCompleted,
        //        viewCompleted,
        //        response => response.Loads);
        //    _engine.PushEvent(wrapper);
        //}

        #region [ Defects ]

        public void DeleteDefect(Guid defectId, Func<ViewModelWrapper<DefectViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteDefect),
                Data = new DeleteDefectRequestData(_session.UserId) { DefectId = defectId }
            };
            var wrapper = new EventRequestWrapper<DefectViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DeleteDefectRepair(Guid repairId, Func<ViewModelWrapper<DefectRepairViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DeleteDefectRepair),
                Data = new DeleteDefectRepairRequestData(_session.UserId) { RepairId = repairId }
            };
            var wrapper = new EventRequestWrapper<DefectRepairViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void LoadDefects(Guid? truckId, Guid? trailerId, Func<ViewModelWrapper<List<DefectViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadDefects),
                Data = new LoadDefectsRequestData(_session.UserId) { TruckId = truckId, TrailerId = trailerId }
            };
            var wrapper = new EventRequestWrapper<List<DefectViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        #endregion [ Defects ]

        public void BindLoads(Guid? parentId, LoadParentType parentType, List<Guid> loadIds, Func<ViewModelWrapper<List<LoadViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(BindLoads),
                Data = new BindLoadsRequestData(_session.UserId) {
                    LoadIds = loadIds,
                    ParentId = parentId,
                    ParentType = parentType,
                }
            };
            var wrapper = new EventRequestWrapper<List<LoadViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SaveLoadAndSaveDatDriver(
            DatDriverViewModel datDriver,
            LoadViewModel load,
            Func<ViewModelWrapper<DatDriverViewModel>,
                   ViewModelWrapper<LoadViewModel>, Task> viewCompleted) {
            var request1 = new EventRequest() {
                Name = nameof(SaveDatDriver),
                Data = new SaveDatDriverRequestData(_session.UserId) { DatDriver = DatDriverConverter.ToDto(datDriver) }
            };
            var request2 = new EventRequest() {
                Name = nameof(SaveLoad),
                Data = new SaveLoadRequestData(_session.UserId) { Load = LoadConverter.ToDto(load) }
            };

            GroupPushEvents(request1, request2, viewCompleted);
            this.SetSavingFlag(load, true);
        }

        public void SaveDatDriverAndBindLoad(
            DatDriverViewModel datDriver,
            LoadViewModel load,
            Func<ViewModelWrapper<DatDriverViewModel>, ViewModelWrapper<List<LoadViewModel>>, Task> viewCompleted) {
            var request1 = new EventRequest() {
                Name = nameof(SaveDatDriver),
                Data = new SaveDatDriverRequestData(_session.UserId) { DatDriver = DatDriverConverter.ToDto(datDriver) }
            };
            var request2 = new EventRequest() {
                Name = nameof(BindLoads),
                Data = new BindLoadsRequestData(_session.UserId) {
                    ParentId = datDriver.EntityId,
                    ParentType = LoadParentType.DatDriver,
                    LoadIds = new List<Guid>() { load.EntityId }
                }
            };

            GroupPushEvents(request1, request2, viewCompleted);
        }

        public void SaveLoadAndAttachFile(
            LoadViewModel load, FileViewModel file,
            Func<ViewModelWrapper<LoadViewModel>, ViewModelWrapper<FileViewModel>, Task> viewCompleted) {

            var request1 = new EventRequest() {
                Name = nameof(SaveLoad),
                Data = new SaveLoadRequestData(_session.UserId) { Load = LoadConverter.ToDto(load) }
            };

            var fileDto = FileConverter.ToDto(file);
            fileDto.Data = file.Data;
            var request2 = new EventRequest() {
                Name = nameof(FileSave),
                Data = new FileSaveRequestData(_session.UserId) { File = fileDto }
            };

            GroupPushEvents(request1, request2, viewCompleted);
            this.SetSavingFlag(load, true);
        }

        public void LoadHistory(
            DispatchBoardType board, DateTime? begin, DateTime? end,
            Func<ViewModelWrapper<List<byte[]>>, ViewModelWrapper<List<TrailerViewModel>>, Task> viewCompleted) {
            var request1 = new EventRequest() {
                Name = nameof(LoadLoadsZip),
                Data = new LoadLoadsZipRequestData(_session.UserId) {
                    Types = new DispatchBoardType[] { board },
                    HistoryBegin = begin,
                    HistoryEnd = end
                }
            };
            var request2 = new EventRequest() {
                Name = nameof(LoadTrailers),
                Data = new LoadTrailersRequestData(_session.UserId) {
                    Types = new DispatchBoardType[] { board },
                    HistoryBegin = begin,
                    HistoryEnd = end
                }
            };
            GroupPushEvents(request1, request2, viewCompleted);
        }

        public void LoadHistory2(RequestHistory2Dto requestDto, Func<ViewModelWrapper<ResponseHistory2Dto>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(LoadHistory2),
                Data = new LoadHistory2RequestData(_session.UserId) {
                    Request = requestDto,
                }
            };
            var wrapper = new EventRequestWrapper<ResponseHistory2Dto, LoadHistory2ResponseData>(request, EventCompleted, viewCompleted,
                converter: (data) => new ResponseHistory2Dto() {
                    Loads = data.Loads,
                    Drivers = data.Drivers,
                    Trailers = data.Trailers,
                    Trucks = data.Trucks,
                    Yards = data.Yards,
                    Warehouses = data.Warehouses,
                    ActualCompanies = data.ActualCompanies,
                    Total = data.Total,
                    PageIndex = data.PageIndex
                });
            _engine.PushEvent(wrapper);
        }

        private void GroupPushEvents<T1, T2>(
            EventRequest request1,
            EventRequest request2,
            Func<ViewModelWrapper<T1>, ViewModelWrapper<T2>, Task> viewCompleted) {
            ViewModelWrapper<T1> wrapper1 = null;
            ViewModelWrapper<T2> wrapper2 = null;

            Func<ViewModelWrapper<T1>, Task> viewCompleted1 = async w => { wrapper1 = w; await ViewCompleted(wrapper1, wrapper2, viewCompleted); };
            Func<ViewModelWrapper<T2>, Task> viewCompleted2 = async w => { wrapper2 = w; await ViewCompleted(wrapper1, wrapper2, viewCompleted); };

            var eventWrapper1 = new EventRequestWrapper<T1>(request1, EventCompleted, viewCompleted1);
            var eventWrapper2 = new EventRequestWrapper<T2>(request2, EventCompleted, viewCompleted2);

            _engine.PushEvent(eventWrapper1);
            _engine.PushEvent(eventWrapper2);
        }

        private void GroupPushEvents<T1, T2, T3>(
            EventRequest request1,
            EventRequest request2,
            EventRequest request3,
            Func<ViewModelWrapper<T1>, ViewModelWrapper<T2>, ViewModelWrapper<T3>, Task> viewCompleted) {
            ViewModelWrapper<T1> wrapper1 = null;
            ViewModelWrapper<T2> wrapper2 = null;
            ViewModelWrapper<T3> wrapper3 = null;

            Func<ViewModelWrapper<T1>, Task> viewCompleted1 = async w => { wrapper1 = w; await ViewCompleted(wrapper1, wrapper2, wrapper3, viewCompleted); };
            Func<ViewModelWrapper<T2>, Task> viewCompleted2 = async w => { wrapper2 = w; await ViewCompleted(wrapper1, wrapper2, wrapper3, viewCompleted); };
            Func<ViewModelWrapper<T3>, Task> viewCompleted3 = async w => { wrapper3 = w; await ViewCompleted(wrapper1, wrapper2, wrapper3, viewCompleted); };

            var eventWrapper1 = new EventRequestWrapper<T1>(request1, EventCompleted, viewCompleted1);
            var eventWrapper2 = new EventRequestWrapper<T2>(request2, EventCompleted, viewCompleted2);
            var eventWrapper3 = new EventRequestWrapper<T3>(request3, EventCompleted, viewCompleted3);

            _engine.PushEvent(eventWrapper1);
            _engine.PushEvent(eventWrapper2);
            _engine.PushEvent(eventWrapper3);
        }

        private async Task ViewCompleted<T1, T2>(ViewModelWrapper<T1> result1, ViewModelWrapper<T2> result2,
                                           Func<ViewModelWrapper<T1>, ViewModelWrapper<T2>, Task> viewCompleted) {
            if (result1 != null && result2 != null) {
                await viewCompleted(result1, result2);
            }
        }
        private async Task ViewCompleted<T1, T2, T3>(ViewModelWrapper<T1> result1, ViewModelWrapper<T2> result2, ViewModelWrapper<T3> result3,
                                               Func<ViewModelWrapper<T1>, ViewModelWrapper<T2>, ViewModelWrapper<T3>, Task> viewCompleted) {
            if (result1 != null && result2 != null && result3 != null) {
                await viewCompleted(result1, result2, result3);
            }
        }

        #region [ files ]

        /// <summary>
        ///     Load files for load or company </summary>
        /// <remarks>Without binary data</remarks>
        public void FilesLoad(Guid entityId, bool preview, Func<ViewModelWrapper<List<FileViewModel>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(FilesLoad),
                Data = new FilesLoadRequestData(_session.UserId) { EntityId = entityId, Preview = preview }
            };
            var wrapper = new EventRequestWrapper<List<FileViewModel>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        /// <summary>
        ///     Load file with binary data </summary>
        public void FileLoad(FileViewModel viewModel, Func<ViewModelWrapper<FileViewModel>, Task> viewCompleted) {
            var dto = FileConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(FileLoad),
                Data = new FileLoadRequestData(_session.UserId) { File = dto }
            };
            var wrapper = new EventRequestWrapper<FileViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        /// <summary>
        ///     Save file with binary data </summary>
        public void FileSave(FileViewModel viewModel, Func<ViewModelWrapper<FileViewModel>, Task> viewCompleted) {
            var dto = FileConverter.ToDto(viewModel);
            dto.Data = viewModel.Data;
            var request = new EventRequest() {
                Name = nameof(FileSave),
                Data = new FileSaveRequestData(_session.UserId) { File = dto }
            };
            var wrapper = new EventRequestWrapper<FileViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        /// <summary>
        ///     Rename file </summary>
        public void FileRename(FileViewModel viewModel, string newName, Func<ViewModelWrapper<FileViewModel>, Task> viewCompleted) {
            var dto = FileConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(FileRename),
                Data = new FileRenameRequestData(_session.UserId) { File = dto, NewName = newName }
            };
            var wrapper = new EventRequestWrapper<FileViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        /// <summary>
        ///     Delete file </summary>
        public void FileDelete(FileViewModel viewModel, Func<ViewModelWrapper<FileViewModel>, Task> viewCompleted) {
            var dto = FileConverter.ToDto(viewModel);
            var request = new EventRequest() {
                Name = nameof(FileDelete),
                Data = new FileDeleteRequestData(_session.UserId) { File = dto }
            };
            var wrapper = new EventRequestWrapper<FileViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        #endregion [ files ]

        #region [ Insurance ]

        /// <summary>
        ///     Load Insurance file with binary data </summary>
        public void InsuranceLoad(DatCompanyViewModel company, Func<ViewModelWrapper<DatCompanyViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(InsuranceLoad),
                Data = new InsuranceLoadRequestData(_session.UserId) { EntityId = company.EntityId }
            };
            var wrapper = new EventRequestWrapper<DatCompanyViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        #endregion [ Insurance ]

        #region [ Setup Packet ]

        /// <summary>
        ///     Send email to invite in registration </summary>
        public void SetupPacketInviteSend(Guid datCompanyId, List<string> emails, Func<ViewModelWrapper<DatCompanyViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(SetupPacketInviteSend),
                Data = new SetupPacketInviteSendRequestData(_session.UserId) {
                    DatCompanyId = datCompanyId,
                    Emails = emails,
                    Resend = true
                }
            };
            var wrapper = new EventRequestWrapper<DatCompanyViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        /// <summary>
        ///     Send an email to edit an insurance conpensation </summary>
        public void SetupPacketEditSend(Guid datCompanyId, Func<ViewModelWrapper<DatCompanyViewModel>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(SetupPacketEditSend),
                Data = new SetupPacketEditSendRequestData(_session.UserId) {
                    DatCompanyId = datCompanyId,
                }
            };
            var wrapper = new EventRequestWrapper<DatCompanyViewModel>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        #endregion [ Setup Packet ]

        public void GetStatistics(Func<ViewModelWrapper<int>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(GetStatistics),
                Data = new GetStatisticsRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<int>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void GetDatCompanyStatictics(Func<ViewModelWrapper<Dictionary<Guid, int>>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(GetDatCompanyStatictics),
                Data = new GetDatCompanyStatisticsRequestData(_session.UserId)
            };
            var wrapper = new EventRequestWrapper<Dictionary<Guid, int>>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void BnsfRefresh(Guid actualCompanyId, Func<ViewModelWrapper<BnsfRefreshResultDto>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(BnsfRefresh),
                Data = new BnsfRefreshRequestData(_session.UserId) {
                    ActualCompanyId = actualCompanyId
                }
            };
            var wrapper = new EventRequestWrapper<BnsfRefreshResultDto>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SendSms(Guid actualCompanyId, string toPhone, string message, Func<ViewModelWrapper<string>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(SendSms),
                Data = new SendSmsRequestData(_session.UserId) {
                    ActualCompanyId = actualCompanyId,
                    ToPhone = toPhone,
                    Message = message
                }
            };
            var wrapper = new EventRequestWrapper<string>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SendSchemeEmail(Guid actualCompanyId, string emails, string trailer, byte[] data, Func<ViewModelWrapper<bool>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(SendSchemeEmail),
                Data = new SendSchemeEmailRequestData(_session.UserId) {
                    ActualCompanyId = actualCompanyId,
                    Emails = emails,
                    TrailerNumber = trailer,
                    Data = data
                }
            };
            var wrapper = new EventRequestWrapper<bool>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void EnableCarrierViewTracker(string phone, Guid loadId, Func<ViewModelWrapper<LoadDto>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(EnableCarrierViewTracker),
                Data = new EnableCarrierViewTrackerRequestData(_session.UserId) {
                    Phone = phone,
                    LoadId = loadId
                }
            };
            var wrapper = new EventRequestWrapper<LoadDto>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void DisableCarrierViewTracker(Guid loadId, Func<ViewModelWrapper<LoadDto>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(DisableCarrierViewTracker),
                Data = new DisableCarrierViewTrackerRequestData(_session.UserId) {
                    LoadId = loadId
                }
            };
            var wrapper = new EventRequestWrapper<LoadDto>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        public void SyncCarrierViewTracker(int trackerId, Func<ViewModelWrapper<int?>, Task> viewCompleted) {
            var request = new EventRequest() {
                Name = nameof(SyncCarrierViewTracker),
                Data = new SyncCarrierViewTrackerRequestData(_session.UserId) {
                    TrakerId = trackerId
                }
            };
            var wrapper = new EventRequestWrapper<int?>(request, EventCompleted, viewCompleted);
            _engine.PushEvent(wrapper);
        }

        #endregion [ Public ]

        private void SetSavingFlag(LoadViewModel load, bool flag) {
            load.Saving = flag;
        }

        private void SetSavingFlag(List<LoadViewModel> loads, bool flag) {
            foreach (var load in loads) {
                SetSavingFlag(load, flag);
            }
        }

        /// <summary>
        ///     Universal method completation </summary>
        /// <typeparam name="T">Converted Type</typeparam>
        /// <param name="response">Event Responce</param>
        /// <param name="viewCompleted">View Completed delegat</param>
        private async Task EventCompleted<T>(EventResponse response, Func<ViewModelWrapper<T>, Task> viewCompleted) {
            if (response.Status == ResponseStatus.Success) {
                var result = ConverterHelper.ConvertEventResponseData<T>(this.Mapper, response.Data);
                var viewModels = result == null ? default(T) : (T)result;
                await viewCompleted.Invoke(new ViewModelWrapper<T>(viewModels));
            } else {
                await viewCompleted.Invoke(new ViewModelWrapper<T>(response.ErrorMessage));
            }
        }

        /// <summary>
        ///     Converter ResponseData to ViewModel with the determinating converter </summary>
        private async Task EventCompleted<TResponseData, T>(EventResponse response, Func<ViewModelWrapper<T>, Task> viewCompleted,
            Func<TResponseData, T> converter) where TResponseData : BaseEventResponseData {
            if (response.Status == ResponseStatus.Success) {
                var viewModels = converter((TResponseData)response.Data);
                await viewCompleted.Invoke(new ViewModelWrapper<T>(viewModels));
            } else {
                await viewCompleted.Invoke(new ViewModelWrapper<T>(response.ErrorMessage));
            }
        }
    }
}
