using AutoCall.Common.Enums;
using AutoDispatch.Logic.Converters;
using AutoCall.Common;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Dialogs;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Settings;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoDispatch.Logic.ViewModels;
using AutoMapper;
using DatCompanyDataService = AutoDispatch.Logic.DataServices.DatCompanyDataService;
using AutoCall.Common.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ResponseStatus = AutoDispatch.Dtos.Enums.ResponseStatus;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Model;
using AutoCall.WPF.Helpers;
using DispatchHelper = AutoDispatch.Logic.Helpers.DispatchHelper;
using AutoCall.Common.Helpers;
using AutoCall.Logic.ViewModels;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Common.Attributes;
using AutoDispatch.Logic.Heplers;
using AutoCall.Common.Extensions;
using System.Runtime.InteropServices;
using AutoCall.Common.Collections;
using AutoCall.Common.Loggers;
using AutoCall.WPF.Properties;
using System.Collections;

namespace AutoCall.WPF.UserControls.Boards {
    /// <summary>
    ///     Container of all commands with property of control </summary>
    public class DispatchActions {

        #region [ _fields ]

        private IDispatchControl _control;
        private DispatchService _dispatchService = App.Services.GetRequiredService<DispatchService>();
        private MainManager _manager = App.Services.GetRequiredService<MainManager>();
        private IMapper _mapper = App.Services.GetRequiredService<IMapper>();
        private ReportService _reportService = App.Services.GetRequiredService<ReportService>();
        private SettingsService _settings = App.Services.GetRequiredService<SettingsService>();

        #endregion [ _fields ]

        #region [ .ctor ]

        public DispatchActions(IDispatchControl control) {
            this._control = control;
        }

        #endregion [ .ctor ]

        #region [ _helpers ]

        public bool CheckParent(LoadViewModel load) {
            var checkParent = load.ParentId == null;
            var result = checkParent ||
                (_control.SelectedCarrierCompany != null &&
                 load.ActualCompanyId != _control.SelectedCarrierCompany.EntityId &&
                 _control.DispatchBoardType == DispatchBoardType.CarrierAccount) ||
                (_control.SelectedBrokerCompany != null &&
                 load.ActualCompanyId != _control.SelectedBrokerCompany.EntityId &&
                 _control.DispatchBoardType == DispatchBoardType.BrokerAccount);
            if (result == false) {
                MessageBoxEx.Alert("you can not edit this load");
            }
            return result;
        }

        public bool CheckSelected(IList<object> items) {
            if (items.Count() == 1) {
                return true;
            } else {
                MessageBoxEx.Alert("Please select one load");
                return false;
            }
        }

        /// <summary>
        ///     Return root load and children </summary>
        /// <returns>Return clones</returns>
        public List<LoadViewModel> GetUnassigningLoads(IList<Wrapper1LoadViewModel> wrappers) {
            var loads = new List<LoadViewModel>();
            foreach (var wrapper in wrappers) {
                loads.Add((LoadViewModel)wrapper.Load.Clone());
                //if (wrapper.Load.ContainerType == ContainerType.Shipment) {
                loads.AddRange(wrapper.Children.Select(w => (LoadViewModel)w.Load.Clone()));
                //}
            }
            return loads;
        }

        /// <summary>
        ///     Replaces containers to subloads </summary>
        public List<Wrapper1LoadViewModel> ReplaceContainers(List<Wrapper1LoadViewModel> wrappers) {
            var containers = wrappers.Where(e => e.Load.ContainerType != ContainerType.None && e.Children.Any()).ToList();
            foreach (var container in containers) {
                var wrapper = DispatchBoardViewModel.Instance.Wrappers.ToList().First(e => e.Load.EntityId == container.Load.EntityId);
                wrappers.Remove(container);
                wrappers.AddRange(wrapper.Children.Where(e => e.Load.Assigned == false));
            }
            return wrappers.Distinct().ToList();
        }

        private LoadViewModel GetParentIfExists(LoadViewModel load) {
            if (load.ParentId != null) {
                var parent = DispatchBoardViewModel.Instance.Loads.FirstOrDefault(e => e.EntityId == load.ParentId);
                return parent ?? load;
            } else {
                return load;
            }
        }

        private Wrapper1LoadViewModel GetParentIfExists(Wrapper1LoadViewModel wrapper) {
            if (wrapper.Load.ParentId != null) {
                var parent = DispatchBoardViewModel.Instance.Wrappers.ToList().FirstOrDefault(e => e.Load.EntityId == wrapper.Load.ParentId);
                return parent ?? wrapper;
            } else {
                return wrapper;
            }
        }

        #endregion [ _helpers ]

        #region [ Actions ]

        public async void AssignCarrierAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AssignCarrierAction");
            if (wrapper.Load.BoardType != _control.DispatchBoardType) {
                MessageBoxEx.Alert("Operation is not available");
                return;
            }
            if (MessageBox.Show($"Do not forget to link cargo with shipments\nDo you want to continue?",
                "Alert",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information) == MessageBoxResult.Cancel) {
                return;
            }

            if (wrapper.Load.ParentId != null) {
                MessageBoxEx.Alert("Operation is not available");
                return;
            }
            var loads = new List<LoadViewModel>() { (LoadViewModel)wrapper.Load.Clone() };

            var dlg = new SearchDatCompanyDialog(_dispatchService, _manager, _settings, wrapper.ActualCompany);
            if (dlg.ShowDialog() == true) {
                var company = dlg.Model.SelectedDatCompany;
                // Check Insurance
                var _dataService = App.Services.GetRequiredService<DatCompanyDataService>();
                _control.BusyStart($"Loading insurance for {company.Name}");

                if (company.McNumber == null && company.DotNumber == null) {
                    throw new Exception("Undefined DOT# and MC#");
                }

                SearchByMcNumberResponse response = await Task.Factory.StartNew(() => _dataService.SearchByMcDotNumber(
                    new SearchByMcDotNumberRequest() {
                        MC = company.McNumber,
                        DOT = company.DotNumber
                    }
                ));

                _control.BusyFinish();
                if (response.IsSuccessed) {
                    if (response.Fields.ContainsKey("Operating Status:")) {
                        if (response.Fields["Operating Status:"] == "ACTIVE") {
                            BindLoads(loads, company);
                        } else {
                            if (MessageBox.Show($"Operating Status: {response.Fields["Operating Status:"]}\nContinue the current operation?",
                                    "Alert",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                                BindLoads(loads, company);
                            }
                        }
                    } else {
                        MessageBoxEx.Alert("'Operating Status' was not found\nThe current operation cannot continue");
                    }
                } else {
                    if (MessageBox.Show(response.ErrorMessage + "\nDo you want to continue?", "", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK) {
                        BindLoads(loads, company);
                    }
                }
            }
        }

        public void AssignDriverAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AssignDriverAction");
            if (CheckConfirmation(GetParentIfExists(wrapper)) == false) {
                return;
            }
            if (CheckMultistops(wrapper) == false) {
                return;
            }

            var dlg = new SearchDriverDialog(_dispatchService, _manager);
            if (dlg.ShowDialog() == true) {
                var trailer = dlg.Model.SelectedWrapper.Trailer;
                BindLoads(
                    ReplaceContainers(new List<Wrapper1LoadViewModel>() { wrapper }).Select(e => (LoadViewModel)e.Load.Clone()).ToList(),
                    trailer);
            }
        }

        public void AssignTrailerAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AssignTrailerAction");
            if (CheckConfirmation(GetParentIfExists(wrapper)) == false) {
                return;
            }
            if (CheckMultistops(wrapper) == false) {
                return;
            }

            var dlg = new SearchTrailerWarehouseDialog(wrapper.Load.DestinationWarehouseId, _dispatchService, _manager, _settings);
            if (dlg.ShowDialog() == true) {
                var destinationWarehouse = dlg.Model.SelectedWarehouseWrapper;
                if (wrapper.Load.DestinationWarehouseId != destinationWarehouse?.EntityId) {
                    wrapper.Load.DestinationWarehouseId = destinationWarehouse?.EntityId;
                    _dispatchService.SaveLoad(wrapper.Load, _control.SaveCompleted);
                }
                var trailer = dlg.Model.SelectedTrailerWrapper;
                BindLoads(
                    ReplaceContainers(new List<Wrapper1LoadViewModel>() { wrapper }).Select(e => (LoadViewModel)e.Load.Clone()).ToList(),
                    trailer);
                // мёртвый код V1 (решено 2026-11-18): ScanFileHelper.FindFile всегда null — файлы trailer_scheme_* создавал только невызываемый RenameTempFile
                //var scanFile = ScanFileHelper.FindFile(trailer.Number);
                //if (scanFile != null) {
                //    SaveLoadAndAttachFile(trailer, File.ReadAllBytes(scanFile), Path.GetExtension(scanFile));
                //}
            }
        }

        public void AssignBrokerLoadAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AssignBrokerLoadAction");
            if (wrapper.Load.BoardType != _control.DispatchBoardType) {
                MessageBoxEx.Alert("Operation is not available");
                return;
            }
            var loads = DispatchBoardViewModel.Instance.Wrappers.ToList().Where(e =>
                (e.Load.ContainerType == ContainerType.None || e.Load.ContainerType == ContainerType.Shipment) &&
                (e.Load.BoardType == DispatchBoardType.Broker || e.Load.BoardType == DispatchBoardType.BrokerAccount) &&
                e.Load.ParentId == null && e.Load.DatCompanyId != null &&
                e.Load.Status != LoadStatus.Canceled2 &&
                e.Load.Status != LoadStatus.Completed &&
                e.Load.Status != LoadStatus.Unpaid
            ).OrderBy(e => e.Load.Age).ToList();
            var dlg = new SearchLoadDialog(_dispatchService, _manager, _settings, wrapper.Load, loads);
            if (dlg.ShowDialog() == true) {
                var selectedLoad = dlg.Model.SelectedItem.Load;
                var initialLoad = wrapper.Load;
                if (MessageBox.Show($@"{selectedLoad.IndexExtra} {selectedLoad.OriginCity} -> {selectedLoad.DestinationCity}
  {initialLoad.IndexExtra} {initialLoad.OriginCity} -> {initialLoad.DestinationCity}
 
Do you want to link the loads?",
                                    "Alert",
                                    MessageBoxButton.OKCancel,
                                    MessageBoxImage.Question) == MessageBoxResult.OK) {
                    BindLoads(new List<LoadViewModel>() { initialLoad }, selectedLoad);
                }
            }
        }

        public void AttachCheckAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AttachCheckAction");
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dialog = new FileCheckDialog(_dispatchService);
            dialog.Model = load;
            dialog.ShowDialog();
        }

        public void MultiAttachCheckAction(List<Wrapper1LoadViewModel> wrappers) {
            StackLogger.Push($"MultiAttachCheckAction");
            if (wrappers.Any() == false) {
                MessageBoxEx.Warning("Select any loads");
                return;
            }
            var loads = wrappers.Select(wrapper => (LoadViewModel)GetParentIfExists(wrapper.Load).Clone()).ToList();
            var dialog = new FileCheckMultipleDialog(_dispatchService);
            dialog.Model = new FileCheckMultiple() { Loads = loads };
            dialog.ShowDialog();
        }

        public void AttachConfirmationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AttachConfirmationAction");
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dlg = new FileConfirmationDialog(_dispatchService) { Model = load };
            dlg.ShowDialog();
        }

        public void AttachInvoiceAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AttachInvoiceAction");
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dialog = new FileInvoiceDialog(_dispatchService);
            dialog.Model = load;
            if (dialog.ShowDialog() == true) {
                dialog.Model.Status = LoadStatus.Billed;
                dialog.Model.InvoiceDate = DateTime.Now;
                _dispatchService.SaveLoad(dialog.Model, _control.SaveCompleted);
                wrapper.Load.Saving = true;
            }
        }

        public void AttachPhotoAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AttachPhotoAction");
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dialog = new OpenFileDialog() {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
                Title = "Upload Photo"
            };
            if (dialog.ShowDialog() == true) {
                load.PhotoFile = Path.GetFileName(dialog.FileName);

                _control.BusyStart("Saving file...");
                var viewModel = new FileViewModel();
                viewModel.EntityId = load.EntityId;
                viewModel.Name = Path.GetFileName(dialog.FileName);
                viewModel.Data = File.ReadAllBytes(dialog.FileName);
                if (FileSizeHelper.Check20MBSize(viewModel.Data)) {
                    _dispatchService.SaveLoadAndAttachFile(load, viewModel, _control.SaveCompleted);
                    wrapper.Load.Saving = true;
                    _control.BusyStart();
                }
            }
        }

        public void AttachBOLAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AttachBOLAction");
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dlg = new FileBOLDialog(_dispatchService) { Model = load };
            dlg.ShowDialog();
        }

        public void AttachPODAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"AttachPODAction");
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dlg = new FilePODDialog(_dispatchService) { Model = load };
            dlg.ShowDialog();
        }

        private void BindLoads(List<LoadViewModel> loads, BaseObjectViewModel target) {
            StackLogger.Push($"BindLoads");
            LoadParentType type = LoadParentType.None;
            if (_control.DispatchBoardType == DispatchBoardType.Broker) {
                if (target is LoadViewModel) {
                    type = LoadParentType.Shipment;
                } else
                if (target is DatCompanyViewModel) {
                    type = LoadParentType.DatCompany;
                } else {
                    throw new Exception($"Undefined type '{target.GetType().Name}'");
                }
            } else
            if (_control.DispatchBoardType == DispatchBoardType.Carrier) {
                if (target is LoadViewModel) {
                    type = LoadParentType.Shipment;
                } else if (target is TrailerViewModel) {
                    type = LoadParentType.Trailer;
                } else if (target is WarehouseViewModel) {
                    type = LoadParentType.Warehouse;
                } else {
                    throw new Exception($"Undefined type '{target.GetType().Name}'");
                }
            }
            if (type == LoadParentType.None) {
                MessageBoxEx.Alert("LoadParentType was not defined");
                return;
            }
            loads.ForEach(e => e.Saving = true);
            _dispatchService.BindLoads(target.EntityId, type, loads.Select(l => l.EntityId).ToList(), _control.SaveCompleted);
            _control.BusyStart("Assigning...");
        }

        private void SaveLoadAndAttachFile(TrailerViewModel trailer, byte[] data, String fileExt) {
            if (FileSizeHelper.Check20MBSize(data) == false) {
                return;
            }
            // to save file
            var fileName = $"trailer_scheme_{DateTime.Now:yyyyMMddHHmm}{fileExt}";

            trailer.SchemeFile = fileName;
            trailer.SchemeDate = DateTime.UtcNow;

            _dispatchService.SaveTrailer((TrailerViewModel)trailer.Clone(), _control.SaveCompleted);

            // to upload file
            var viewModel = new FileViewModel();
            viewModel.EntityId = trailer.EntityId;
            viewModel.Name = fileName;
            viewModel.Data = data;

            _dispatchService.FileSave(viewModel, _control.SaveCompleted);

            _control.BusyStart();
        }

        public void BrokerDispatchDeliveredAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"BrokerDispatchDeliveredAction");
            if (MessageBox.Show("Please confirm that the cargo was delivered", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                clone.Status = LoadStatus.Delivered;
                clone.BoardType = DispatchBoardType.BrokerAccount;
                if (clone.ContainerType == ContainerType.Shipment) {
                    foreach (var load in wrapper.Children) {
                        CarrierDispatchDeliveredAction(load);
                    }
                }
                clone.SaveHistory(wrapper.DatCompany);
                clone.BoardChanged = DateTime.Now;
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
            }
        }

        public object BrokerDispatchAtPickUpAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"BrokerDispatchAtPickUpAction");
            if (MessageBox.Show("Please confirm that the load is on pickup", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                clone.Status = LoadStatus.AtPickUp;
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
                return clone;
            } else {
                return null;
            }
        }

        public void BrokerDispatchPickedUpAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"BrokerDispatchPickedUpAction");
            if (MessageBox.Show("Please confirm that load was picked up", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                clone.Status = LoadStatus.InTransit;
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
            }
        }

        public object CarrierDispatchAtPickUpAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CarrierDispatchAtPickUpAction");
            if (MessageBox.Show("Please confirm that the load is on pickup", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                clone.Status = LoadStatus.AtPickUp;
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
                MoveFromYard(clone.TrailerId);
                return clone;
            } else {
                return null;
            }
        }

        public object CarrierDispatchPickedUpAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CarrierDispatchPickedUpAction");
            if (wrapper.Trailer == null) return null;
            var tClone = (TrailerViewModel)wrapper.Trailer.Clone();
            var lCone = (LoadViewModel)wrapper.Load.Clone();
            var dialog = new TrailerLayoutPickupDialog(
                _dispatchService, _manager.GetCallService, _mapper,
                tClone, wrapper.Load, lCone, DispatchHelper.GetDivideType(lCone));
            dialog.Model.Trailer.GeneratePallets(DispatchBoardViewModel.Instance.Loads.Select(l => (LoadViewModel)l.Clone()).ToList());
            if (dialog.ShowDialog() == true) {
                _control.SetLoads(new List<LoadViewModel>() { dialog.Result });
                MoveFromYard(lCone.TrailerId);
                return dialog.Result;
            } else {
                return null;
            }
        }

        private void MoveFromYard(Guid? trailerId) {
            StackLogger.Push($"CarrierDispatchPickedUpAction");
            var clone = (TrailerViewModel)DispatchBoardViewModel.Instance.Trailers.FirstOrDefault(e => e.EntityId == trailerId)?.Clone();
            if (clone != null && clone.YardId != null) {
                clone.YardId = null;
                _dispatchService.SaveTrailer(clone, _control.SaveCompleted);
            }

        }

        public object CarrierDispatchInTransitAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CarrierDispatchInTransitAction");
            if (MessageBox.Show("Please confirm that the load in transit", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                clone.Status = LoadStatus.InTransit;
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
                MoveFromYard(clone.TrailerId);
                return clone;
            } else {
                return null;
            }
        }

        public void CarrierDispatchMoveToDestinationWarehouseAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CarrierDispatchMoveToDestinationWarehouseAction");
            if (wrapper.Load.DestinationWarehouseId == null || wrapper.DestinationWarehouse == null) {
                MessageBoxEx.Alert("Destination warehouse was not found");
                return;
            }
            if (MessageBox.Show($"Please confirm that the load is moving to the destination warehouse '{wrapper.DestinationWarehouse?.Name}'", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                BindLoads(
                    ReplaceContainers(new List<Wrapper1LoadViewModel>() { wrapper }).Select(e => (LoadViewModel)e.Load.Clone()).ToList(),
                    wrapper.DestinationWarehouse);
            }
        }

        public object CarrierDispatchAtDeliveryAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CarrierDispatchAtDeliveryAction");
            // DestinationWarehouseId
            if (wrapper.Load.DestinationWarehouseId != null) {
                if (wrapper.Load.DestinationWarehouseId != wrapper.Load.WarehouseId) {
                    MessageBoxEx.Alert("To set a status 'at delivery' only from destination warehouse");
                    return null;
                }
            }
            if (MessageBox.Show("Please confirm that the load is on delivery", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                clone.Status = LoadStatus.AtDelivery;
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                MoveFromYard(clone.TrailerId);
                _control.BusyStart();
                return clone;
            } else {
                return null;
            }
        }

        public object CarrierDispatchDeliveredAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CarrierDispatchDeliveredAction");
            // DestinationWarehouseId
            if (wrapper.Load.DestinationWarehouseId != null) {
                if (wrapper.Load.DestinationWarehouseId != wrapper.Load.WarehouseId) {
                    MessageBoxEx.Alert("To set a status 'delivered' only from destination warehouse");
                    return null;
                }
            }
            if (wrapper.Load.Gains.Any(e =>
                 (e.Type == GainType.Lumper || e.Type == GainType.detention) &&
                 e.Cost != 0 && e.Paid == false)) {
                if (MessageBox.Show("PLEASE CHECK IF THERE WAS LUMPER OR DETENTION BEFORE CLOSING THE LOAD", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) {
                    return null;
                }
            } else {
                if (MessageBox.Show("Please confirm that the cargo was delivered", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) {
                    return null;
                }
            }
            Guid? trailerId = wrapper.Trailer?.EntityId;
            if (trailerId != null) {
                // грузы в этом трейлере
                var loadIds = DispatchBoardViewModel.Instance.Loads
                    .Where(e => e.TrailerId == trailerId && e.ParentId != null)
                    .Select(e => e.ParentId).ToList();
                if (DispatchBoardViewModel.Instance.Loads.Any(e => e.TrailerId != trailerId &&
                    e.ParentId != null && loadIds.Contains(e.ParentId) &&
                    DisplayAttribute.GetOrder(e.Status) < DisplayAttribute.GetOrder(LoadStatus.Delivered))) {
                    if (MessageBox.Show("Not all parts of the load are loaded into the trailer!\nAre you sure you want to continue the operation?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK) {
                        return null;
                    }
                }
            }

            var clones = new List<LoadViewModel>() { (LoadViewModel)wrapper.Load.Clone() };
            clones.AddRange(Wrapper1LoadViewModel.GetRecursiveChildren(wrapper).Select(w => (LoadViewModel)w.Load.Clone()));

            foreach (var clone in clones) {
                var w = DispatchBoardViewModel.Instance.Wrappers.ToList().FirstOrDefault(e => e.Load.EntityId == clone.EntityId);
                if (w == null) {
                    MessageBoxEx.Error($"The load '{clone.IndexString}' was not found");
                    return null;
                }
                foreach (LoadPalletViewModel pallet in clone.Layout.ToList()) {
                    if (w.Trailer != null) {
                        DispatchHelper.FindAndRemovePalletFromTrailer(w.Trailer, pallet);
                        clone.Layout.Remove(pallet);
                    }
                }
                clone.Status = LoadStatus.Delivered;
                clone.BoardType = DispatchBoardType.CarrierAccount;
                clone.SaveHistory(w.Driver, w.Truck, w.Trailer, w.Shipment, w.Warehouse);
                clone.BoardChanged = DateTime.Now;
            }
            _dispatchService.SaveLoads(clones, _control.SaveCompleted);
            wrapper.Load.Saving = true;
            _control.BusyStart();
            return clones;

        }

        public void ChangeLocationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ChangeLocationAction");
            if (_control.DispatchBoardType == DispatchBoardType.Carrier) {
                if (wrapper.Trailer == null) {
                    MessageBoxEx.Alert("you can not change the location of this cargo");
                    return;
                }
                var clone = (LoadViewModel)wrapper.Load.Clone();
                var dlg = new EditLocationDialog(false);
                LocationViewModel location = clone.CurrentLocation != null ? (LocationViewModel)clone.CurrentLocation.Clone() : new LocationViewModel();
                dlg.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                    Location = location,
                });
                dlg.Initialize();
                if (dlg.ShowDialog() == true) {
                    if (wrapper.Driver != null) {
                        _dispatchService.LocationDriverChanged(wrapper.Driver.EntityId, dlg.Model.Location.Location, _control.SaveCompleted);
                    } else if (wrapper.Truck != null) {
                        _dispatchService.LocationTruckChanged(wrapper.Truck.EntityId, dlg.Model.Location.Location, _control.SaveCompleted);
                    } else if (wrapper.Trailer != null) {
                        _dispatchService.LocationTrailerChanged(wrapper.Trailer.EntityId, dlg.Model.Location.Location, _control.SaveCompleted);
                    }
                    // TODO: DELETE CODE. you don't use comment for load
                    //// saves comment
                    //var loads = DispatchBoardViewModel.Instance.Wrappers.Where(e => e.Trailer == wrapper.Trailer).Select(e => e.Load).ToList();
                    //if (loads.Any()) {
                    //    loads.ForEach(e => e.Comments = dlg.Model.Comments);
                    //    _dispatchService.SaveLoads(loads,_control.SaveCompleted);
                    //}
                }
            }

            if (_control.DispatchBoardType == DispatchBoardType.Broker) {
                var clone = (LoadViewModel)wrapper.Load.Clone();
                var dlg = new EditLocationDialog(true);
                LocationViewModel location = clone.CurrentLocation != null ? (LocationViewModel)clone.CurrentLocation.Clone() : new LocationViewModel();
                dlg.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                    Location = location,
                });
                dlg.Initialize();
                if (dlg.ShowDialog() == true) {
                    // saves location
                    _dispatchService.LocationLoadChanged(clone.EntityId, dlg.Model.Location.Location, _control.SaveCompleted);
                    // saves comment
                    clone.Comments = dlg.Model.Comments;
                    _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                }
            }
        }

        public bool CheckConfirmation(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CheckConfirmation");
            if (wrapper.Load.BoardType != _control.DispatchBoardType) {
                MessageBoxEx.Alert("Operation is not available");
                return false;
            }
            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            if (load.ConfirmationFile == null) {
                // Add confirmation
                if (MessageBox.Show($"Do you have confirmation for load {load.IndexString}?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    var dialog = new FileConfirmationDialog(_dispatchService);
                    dialog.Model = load;
                    return dialog.ShowDialog() == true;
                } else {
                    MessageBoxEx.Alert("You can't continue without confirmation");
                    return false;
                }
            }
            return true;
        }

        public bool CheckMultistops(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CheCheckMultistopsckConfirmation");
            if (wrapper.PickupCount == 1 && wrapper.DeliveryCount == 1 && wrapper.Load.Container == true && wrapper.Children.Any()) {
                MessageBoxEx.Alert("Assign each part separately");
                return false;
            } else {
                if (wrapper.Load.Container == true) {
                    if (MessageBox.Show($@"Are you sure you want to assign loads?
{String.Join("\n", wrapper.Children.Select(w => $"#{w.Load.IndexString} {w.Load.OriginCity} -> {w.Load.DestinationCity}"))}",
  "Assign Load", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }
            return true;
        }

        public void CreateBOLAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CreateBOLAction");

            if (wrapper.Load.ConfirmationFile == null) {
                MessageBoxEx.Alert("Confirmation was not found");
                return;
            }

            if (!wrapper.Load.Container) {
                var load = (LoadViewModel)wrapper.Load.Clone();

                RadDocument document = _reportService.GenerateBOL(
                    wrapper.DatCompany,
                    wrapper.ActualCompany,
                    load,
                    wrapper.Children.Select(e => e.Load).ToList(),
                    wrapper.DatDriver);

                var editor = new RichTextEditor();
                var provider = new XamlFormatProvider();
                editor.ViewModel = provider.Export(document);
                editor.Title = $"Create BOL {load.IndexString}";

                if (editor.ShowDialog() == true) {
                    var doc = provider.Import(editor.ViewModel);
                    var binary = _reportService.GenerateBOLPdf(doc);

                    var filename = $"bill_of_lading_{_reportService.ToFileName(wrapper.DatCompany.Name)}.pdf";

                    var fileViewModel = new FileViewModel() {
                        EntityId = load.EntityId,
                        Name = Path.GetFileName(filename),
                        Data = binary
                    };

                    load.BOLDate = DateTime.Now;
                    load.BOLFile = filename;

                    if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                        _dispatchService.SaveLoadAndAttachFile(load, fileViewModel, _control.SaveCompleted);
                        _control.BusyStart();
                    }
                }
            } else {
                foreach (Wrapper1LoadViewModel child in wrapper.Children) {
                    var load = (LoadViewModel)child.Load.Clone();

                    RadDocument document = _reportService.GenerateBOL(
                        child.DatCompany,
                        child.ActualCompany,
                        load,
                        new List<LoadViewModel>(),
                        wrapper.DatDriver);

                    var editor = new RichTextEditor();
                    var provider = new XamlFormatProvider();
                    editor.ViewModel = provider.Export(document);
                    editor.Title = $"Create BOL {load.IndexString}";

                    if (editor.ShowDialog() == true) {
                        var doc = provider.Import(editor.ViewModel);
                        var binary = _reportService.GenerateBOLPdf(doc);

                        var indexString = new String(load.IndexString.Replace("/", "_").Where(e => Char.IsLetterOrDigit(e) || e == '_').ToArray());
                        var filename = $"bill_of_lading_{_reportService.ToFileName(child.DatCompany.Name)}_{indexString}.pdf";

                        var fileViewModel = new FileViewModel() {
                            EntityId = wrapper.Load.EntityId,
                            Name = Path.GetFileName(filename),
                            Data = binary
                        };

                        wrapper.Load.BOLDate = DateTime.Now;
                        wrapper.Load.BOLFile = filename;

                        if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                            _dispatchService.SaveLoadAndAttachFile(wrapper.Load, fileViewModel, _control.SaveCompleted);
                            _control.BusyStart();
                        }
                    }
                }
            }
        }

        public void IssuesAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"IssuesAction");
            wrapper = GetParentIfExists(wrapper);
            var clone = (LoadViewModel)wrapper.Load.Clone();
            if (CheckParent(clone) == false) {
                return;
            }
            clone.Status = LoadStatus.Issues;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            _control.BusyStart();
        }

        public void BondAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"BondAction");
            wrapper = GetParentIfExists(wrapper);
            var clone = (LoadViewModel)wrapper.Load.Clone();
            if (CheckParent(clone) == false) {
                return;
            }
            clone.Status = LoadStatus.Bond;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            _control.BusyStart();
        }

        public async void CreateConfirmationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CreateConfirmationAction");
            var datCompanyService = App.Services.GetRequiredService<DatCompanyDataService>();

            var clone = (LoadViewModel)wrapper.Load.Clone();
            clone.ShowPickupAddress = MessageBox.Show("Show pickup address?", "Create Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            _control.BusyStart($"Loading details of '{wrapper.DatCompany.Name}'...");

            if (wrapper.DatCompany.McNumber == null && wrapper.DatCompany.DotNumber == null) {
                throw new Exception("Undefined DOT# and MC#");
            }

            // Gets company details by MC#
            SearchByMcNumberResponse result = await Task.Factory.StartNew(() => datCompanyService.SearchByMcDotNumber(
                new SearchByMcDotNumberRequest() {
                    DOT = wrapper.DatCompany.DotNumber,
                    MC = wrapper.DatCompany.McNumber
                }
            ));

            _control.BusyFinish();
            Dictionary<string, string> companyDetails;
            if (result.IsSuccessed) {
                companyDetails = result.Fields;
            } else {
                MessageBoxEx.Warning(result.ErrorMessage);
                companyDetails = new Dictionary<string, string>() {
                    { "Physical Address:", "-" },
                    { "Mailing Address:", "-" },
                };
            }

            // Generates RadDocument
            var document = _reportService.GenerateConfirmation(wrapper.DatCompany, wrapper.ActualCompany, companyDetails, clone,
                wrapper.Children.Select(e => e.Load).ToList());
            var editor = new RichTextEditor();
            var provider = new XamlFormatProvider();
            editor.ViewModel = provider.Export(document);
            editor.Title = "Create Confirmation";

            if (editor.ShowDialog() == true) {
                // Create order
                clone.ConfirmationDate = DateTime.Now;

                _dispatchService.SaveLoad(
                    clone,
                    async w => {
                        await _control.SaveCompleted(w);

                        #region [ to generate confirmation ]

                        await _control.Dispatcher.InvokeAsync(async () => {
                            if (w.Status == ResponseStatus.Success) {
                                // Sets file for order
                                // Uploads file
                                var l = w.Data;

                                var doc = provider.Import(editor.ViewModel);

                                var binary = _reportService.GenerateConfirmationPdf(doc, l);

                                var filename = $"confirmation_{_reportService.ToFileName(wrapper.DatCompany.Name)}_{MathHelper.DateTimeNowToScale62()}_tmp.pdf";
                                l.ConfirmationFile = Path.GetFileName(filename);

                                var fileViewModel = new FileViewModel() {
                                    EntityId = l.EntityId,
                                    Name = filename,
                                    Data = binary
                                };
                                if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                                    _dispatchService.SaveLoadAndAttachFile(
                                    l,
                                    fileViewModel,
                                    async (w1, w2) => {
                                        await _control.SaveCompleted(w1, w2);

                                        #region [ to send confirmation ]

                                        await _control.Dispatcher.InvokeAsync((Action)(async () => {
                                            var emails = wrapper.DatCompany.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email).ToList();

                                            var dialog = new EmailListDialog($"Please confirm to send email(s) to {String.Join(", ", emails.Select(em => $"'{em}'"))} with load {clone.IndexString} {clone.Direction}", "AutoCall - Send Confirmation", emails.ToList());
                                            if (dialog.ShowDialog() == true) {
                                                _dispatchService.ConfirmationSend(clone.EntityId, dialog.Model.EmailList, _control.SaveCompleted);
                                                _control.BusyStart("sending email...");

                                                #region [ to merge contacts ]

                                                if (dialog.Model.SaveContacts) {
                                                    var contacts = new ContactsViewModel();
                                                    foreach (var email in dialog.Model.EmailList) {
                                                        contacts.AddEmail(email);
                                                    }
                                                    if (wrapper.DatCompany.Contacts.Merge(contacts)) {
                                                        _dispatchService.SaveDatCompany(wrapper.DatCompany, _control.SaveCompleted);
                                                        _control.BusyStart("saving contacts...");
                                                    }
                                                }
                                                return;

                                                #endregion [ to merge contacts ]
                                            }
                                        }));

                                        #endregion [ to send confirmation ]
                                    });
                                    _control.BusyStart("saving confirmation...");
                                }
                            } else {
                                await _control.SaveCompleted(w);
                            }
                        });

                        #endregion [ to generate confirmation ]
                    });

                wrapper.Load.Saving = true;
                _control.BusyStart("saving a cargo...");
            }
        }

        public async void CreateInvoiceAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CreateInvoiceAction");
            wrapper = GetParentIfExists(wrapper);
            var clone = (LoadViewModel)wrapper.Load.Clone();
            if (CheckParent(clone) == false) {
                return;
            }
            if (String.IsNullOrEmpty(clone.Company)) {
                MessageBoxEx.Alert("Please input Company Name");
                return;
            }

            CensusCompanyViewModel company = null;
            var mc = clone.McNumber;
            if (mc != null) {
                try {
                    _control.BusyStart();
                    _control.BusyStart($"Loading details by MC#: '{mc}'...");
                    // Gets company details by MC#
                    var result = await App.Services.GetRequiredService<DatCompanyDataService>()
                        .SearchCensusCompaniesAsync(new SearchCensusCompaniesRequest() {
                            Search = new CensusCompanySearchDto() {
                                ICC = mc,
                                Limit = 1
                            }
                        });
                    if (result.IsSuccessed) {
                        if (result.CensusCompanies.Any()) {
                            company = _mapper.Map<CensusCompanyViewModel>(result.CensusCompanies.First());
                        } else {
                            MessageBoxEx.Alert("Company was not found");
                        }
                    }
                } catch (Exception e) {
                    throw;
                } finally {
                    _control.BusyFinish();
                }
            }

            var factoringCompany = wrapper.ActualCompany.FactoringCompany;

            // Generates RadDocument
            _control.BusyStart($"Creating Invoice...");
            var document = _reportService.GenerateInvoice(wrapper.ActualCompany, company, clone, wrapper.Children.Select(e => e.Load).ToList(), factoringCompany);
            _control.BusyFinish();
            var editor = new RichTextEditor();
            var provider = new XamlFormatProvider();
            editor.ViewModel = provider.Export(document);
            editor.Title = "Create Invoice";

            if (editor.ShowDialog() == true) {
                // Sets invoice number
                _dispatchService.SaveInvoice(
                    clone,
                    async w => {
                        await _control.Dispatcher.InvokeAsync(async () => {
                            if (w.Status == ResponseStatus.Success) {
                                // Sets file for load
                                // Uploads file
                                var l = w.Data;

                                var doc = provider.Import(editor.ViewModel);

                                var binary = _reportService.GenerateInvoicePdf(doc, l);

                                var filename = $"invoice_{_reportService.ToFileName(l.Company)}.pdf";
                                // saves invoice
                                l.InvoiceFile = Path.GetFileName(filename);
                                l.InvoiceDate = DateTime.Now;

                                var fileViewModel = new FileViewModel() {
                                    EntityId = l.EntityId,
                                    Name = filename,
                                    Data = binary
                                };
                                if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                                    _dispatchService.SaveLoadAndAttachFile(l, fileViewModel, _control.SaveCompleted);
                                    _control.BusyStart();
                                }
                            } else {
                                await _control.SaveCompleted(w);
                            }
                        });
                    });
                wrapper.Load.Saving = true;
            }
        }

        public void CreateLoadAction() {
            StackLogger.Push($"CreateLoadAction");
            ActualCompanyViewModel actualCompany;
            switch (_control.DispatchBoardType) {
                case DispatchBoardType.Broker:
                    actualCompany = _control.SelectedBrokerCompany;
                    break;
                case DispatchBoardType.Carrier:
                    actualCompany = _control.SelectedCarrierCompany;
                    break;
                default:
                    throw new Exception();
            }
            // Load default values of pattern
            LoadViewModel viewModel = DefaultLoadSettingsConverter.ToVm((DefaultLoadSettings)_settings.GetDefaultPatternSettings(_control.BoardType));
            if (viewModel == null) {
                viewModel = new LoadViewModel();
            }
            viewModel.BoardType = _control.DispatchBoardType;
            viewModel.ActualCompanyId = actualCompany?.EntityId;

            // Create edit dialog with selected model
            var addDlg = new EditLoadDialog(_dispatchService, _manager.GetCallService, _mapper);
            addDlg.Model = new EditBookLoadViewModel(viewModel, DispatchBoardViewModel.Instance.ActualCompanies, actualCompany);
            // Show dialog
            addDlg.Closed += (sender, e) => {
                if (addDlg.Ok)
                    _settings.SetDefaultPatternSettings(_control.BoardType, DefaultLoadSettingsConverter.ToSettings(addDlg.Model.Load));
            };
            addDlg.Show();
        }

        public async void GenerateLoadAction() {
            StackLogger.Push($"GenerateLoadAction");
            ActualCompanyViewModel actualCompany;
            switch (_control.DispatchBoardType) {
                case DispatchBoardType.Broker:
                    actualCompany = _control.SelectedBrokerCompany;
                    break;
                case DispatchBoardType.Carrier:
                    actualCompany = _control.SelectedCarrierCompany;
                    break;
                default:
                    throw new Exception();
            }
            // Load default values of pattern
            LoadViewModel viewModel = DefaultLoadSettingsConverter.ToVm((DefaultLoadSettings)_settings.GetDefaultPatternSettings(_control.BoardType));
            {
                var random = StaticHelper.GetInstatice<Random>();
                var randomActualCompany = DispatchBoardViewModel.Instance.ActualCompanies.OrderBy(e => random.Next()).First();
                var randomLoad1 = DispatchBoardViewModel.Instance.Loads.Where(e => (DispatchBoardType)((int)e.BoardType % 2) == _control.DispatchBoardType).OrderBy(e => StaticHelper.GetInstatice<Random>().Next()).First();
                var randomLoad2 = DispatchBoardViewModel.Instance.Loads.Where(e => (DispatchBoardType)((int)e.BoardType % 2) == _control.DispatchBoardType).OrderBy(e => StaticHelper.GetInstatice<Random>().Next()).First();
                var randomLoad3 = DispatchBoardViewModel.Instance.Loads.Where(e => (DispatchBoardType)((int)e.BoardType % 2) == _control.DispatchBoardType).OrderBy(e => StaticHelper.GetInstatice<Random>().Next()).First();

                viewModel = new LoadViewModel();

                viewModel.ActualCompanyId = actualCompany?.EntityId ?? randomActualCompany.EntityId;
                viewModel.ActualCompany = actualCompany ?? randomActualCompany;

                viewModel.ShipperId = randomLoad1.ShipperId;
                viewModel.ShipperCompany = randomLoad1.ShipperCompany;
                viewModel.ShipperContacts = randomLoad1.ShipperContacts;

                viewModel.OriginAddress = randomLoad1.OriginAddress;
                viewModel.OriginCity = randomLoad1.OriginCity;
                viewModel.OriginLocation = (LocationViewModel)randomLoad1.OriginLocation.Clone();
                viewModel.CurrentOriginUtcOffset = randomLoad1.CurrentOriginUtcOffset;

                viewModel.ReceiverId = randomLoad2.ReceiverId;
                viewModel.ReceiverCompany = randomLoad2.ReceiverCompany;
                viewModel.ReceiverContacts = randomLoad2.ReceiverContacts;

                viewModel.DestinationAddress = randomLoad2.DestinationAddress;
                viewModel.DestinationCity = randomLoad2.DestinationCity;
                viewModel.DestinationLocation = (LocationViewModel)randomLoad2.DestinationLocation.Clone();
                viewModel.CurrentDestinationUtcOffset = randomLoad2.CurrentDestinationUtcOffset;

                viewModel.PickupDate = DateTime.UtcNow.AddDays(random.Next(-1, +3));
                viewModel.PickupFrom = TimeSpan.FromHours(random.Next(0, 12));
                viewModel.PickupFrom = TimeSpan.FromHours(random.Next(12, 24));

                viewModel.DeliveryDate = viewModel.PickupDate.AddDays(random.Next(4));
                viewModel.DeliveryFrom = TimeSpan.FromHours(random.Next(0, 12));
                viewModel.DeliveryFrom = TimeSpan.FromHours(random.Next(12, 24));

                viewModel.PickupNumber = Guid.NewGuid().ToString().Truncate(4);
                viewModel.DeliveryNumber = Guid.NewGuid().ToString().Truncate(4);

                // мёртвый код V1: DAT rate (IApiService.GetHybridRate) — сервер V1 всегда отвечал «DISABLED», т.е. всегда ветка else;
                // GetRateRequest/RateConverter/DatPowerRateViewModel — модуль AutoCall
                //var apiService = App.Services.GetRequiredService<IApiService>();
                //var request = AutoCall.WPF.Converters.DatPower.Rate.RateConverter.Convert(viewModel);
                //_control.BusyStart();
                //var result = await Task.Factory.StartNew(() => apiService.GetHybridRate(request));
                //_control.BusyFinish();
                //if (result.IsSuccessed) {
                //    var rateModel = new DatPowerRateViewModel() { Data = new DatPowerRateDataViewModel() };
                //    var model = rateModel.Data;
                //    Converters.DatPower.Rate.RateConverter.Convert(result, model);
                //    model.Calc();
                //    viewModel.LineHaul = (decimal?)(model.ShowRates ? model.SpotEstimatedLinehaulRateFuel * model.RateMiles : model.SpotEstimatedLinehaulTotal);
                //    viewModel.LineHaul = viewModel.LineHaul - viewModel.LineHaul % 500 + 1000;
                //} else {
                    viewModel.LineHaul = random.Next(4) * 500;
                //}
                viewModel.Currency = randomLoad3.Currency;
                viewModel.Length = 53;
                viewModel.Weight = 50;
                viewModel.Pallets = (short)random.Next(10, 30);
                viewModel.RecordId = Guid.NewGuid().ToString().Truncate(6);
                viewModel.CustomerId = randomLoad3.CustomerId;
                viewModel.Company = randomLoad3.Company;
                viewModel.DatBrokerId = randomLoad3.DatBrokerId;

                var backgroundBin = new byte[] { 255, 128, 128, 128 };
                var foregroundBin = new byte[] { 255, 255, 255, 255 };
                viewModel.Comments = AutoCall.WPF.Properties.Settings.Default.DispatchCommentTemplate
                    .Replace("$TEXT", " --TEST -- ")
                    .Replace("$FOREGROUND", "#" + BitConverter.ToString(foregroundBin).Replace("-", ""))
                    .Replace("$BACKGROUND", "#" + BitConverter.ToString(backgroundBin).Replace("-", ""));
                viewModel.Comments2 = AutoCall.WPF.Properties.Settings.Default.DispatchCommentTemplate
                    .Replace("$TEXT", " --TEST -- ")
                    .Replace("$FOREGROUND", "#" + BitConverter.ToString(foregroundBin).Replace("-", ""))
                    .Replace("$BACKGROUND", "#" + BitConverter.ToString(backgroundBin).Replace("-", ""));
            }
            viewModel.BoardType = _control.DispatchBoardType;

            // shows all files
            //#if DEBUG
            //            viewModel.BOLFile = "pod.pdf";
            //            viewModel.BOLDate = DateTime.UtcNow;
            //            viewModel.PODFile = "pod.pdf";
            //            viewModel.PODDate = DateTime.UtcNow;
            //            viewModel.LumperFile = "pod.pdf";
            //            viewModel.LumperDate = DateTime.UtcNow;
            //            viewModel.ConfirmationFile = "pod.pdf";
            //            viewModel.ConfirmationDate = DateTime.UtcNow;
            //            viewModel.ConfirmedFile = "pod.pdf";
            //            viewModel.ConfirmedDate = DateTime.UtcNow;
            //            viewModel.InvoiceFile = "pod.pdf";
            //            viewModel.InvoiceDate = DateTime.UtcNow;
            //            viewModel.TrailerSchemeFile = "pod.pdf";
            //            viewModel.TrailerSchemeDate = DateTime.UtcNow;
            //            viewModel.CheckFile = "pod.pdf";
            //            viewModel.CheckDate = DateTime.UtcNow;
            //            viewModel.PhotoFile = "pod.pdf";
            //#endif

            // Create edit dialog with selected model
            var addDlg = new EditLoadDialog(_dispatchService, _manager.GetCallService, _mapper);
            addDlg.Model = new EditBookLoadViewModel(viewModel, DispatchBoardViewModel.Instance.ActualCompanies, viewModel.ActualCompany);
            // Show dialog
            addDlg.Closed += (sender, e) => {
                if (addDlg.Ok)
                    _settings.SetDefaultPatternSettings(_control.BoardType, DefaultLoadSettingsConverter.ToSettings(addDlg.Model.Load));
            };
            addDlg.Show();
        }

        public void DeleteBOLAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"DeleteBOLAction");
            if (wrapper.Load.BOLFile == null) {
                MessageBoxEx.Alert("a file does not exist");
                return;
            }
            if (MessageBox.Show($"Are you sure you want to delete BOL?", "Delete BOL",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            var clone = (LoadViewModel)wrapper.Load.Clone();
            clone.BOLDate = null;
            clone.BOLFile = null;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            _control.BusyStart("Deleting...");
        }

        public void DeletePODAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"DeletePODAction");
            if (wrapper.Load.PODFile == null) {
                MessageBoxEx.Alert("a file does not exist");
                return;
            }
            if (MessageBox.Show($"Are you sure you want to delete POD?", "Delete POD",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            var clone = (LoadViewModel)wrapper.Load.Clone();
            clone.PODDate = null;
            clone.PODFile = null;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            _control.BusyStart("Deleting...");
        }

        public void DeleteConfirmationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"DeleteConfirmationAction");
            if (MessageBox.Show($"Are you sure you want to delete confirmation?", "Delete confirmation",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            var clone = (LoadViewModel)wrapper.Load.Clone();
            clone.ConfirmationDate = null;
            clone.ConfirmationFile = null;
            clone.EmailConfirmationSent = null;
            clone.EmailConfirmationSentDate = null;
            clone.ConfirmedDate = null;
            clone.ConfirmedFile = null;
            clone.EmailConfirmationSentDate = null;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            _control.BusyStart("Deleting...");
        }

        public void DeletePhotoAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"DeletePhotoAction");
            if (MessageBox.Show($"Are you sure you want to delete photo?", "Delete photo",
                         MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            var clone = (LoadViewModel)wrapper.Load.Clone();
            clone.PhotoFile = null;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            _control.BusyStart("Deleting...");
        }

        public void DeleteLoadAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"DeleteLoadAction");
            if (MessageBox.Show($"Are you sure you want to delete selected load?\n{wrapper.Load.IndexString}", "Delete load",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            _dispatchService.DeleteLoads(new List<Guid>() { wrapper.Load.EntityId }, _control.SaveCompleted);
            _control.BusyStart("Deleting...");
        }

        public void DeleteLoadsAction(IList<Wrapper1LoadViewModel> wrappers) {
            StackLogger.Push($"DeleteLoadsAction");
            if (wrappers.Any() == false) {
                MessageBoxEx.Alert("Please choose a load(s) to delete.");
                return;
            }
            if (wrappers.All(w => w.Load.BoardType == _control.DispatchBoardType)) {
                // Check is there selected patterns
                // Show confirmation message
                if (MessageBox.Show($"Are you sure you want to delete selected load(s)?\n\n{String.Join(", ", wrappers.Select(w => $"#{w.Load.IndexString}"))}", "Delete load(s)",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                _dispatchService.DeleteLoads(wrappers.Select(w => w.Load.EntityId).ToList(), _control.SaveCompleted);
                _control.BusyStart("Deleting...");
                return;
            }
            if (wrappers.All(w => w.Load.BoardType != _control.DispatchBoardType)) {
                // Check is there selected patterns
                // Show confirmation message
                if (MessageBox.Show($"Are you sure you want to remove selected load(s)?\n\n{String.Join(", ", wrappers.Select(w => $"#{w.Load.IndexString}"))}", "Remove load(s)",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                _control.BusyStart("Deleting...");

                var clones = wrappers.Select(w => (LoadViewModel)w.Load.Clone()).ToList();
                clones.ForEach(l => {
                    if (_control.DispatchBoardType == DispatchBoardType.Carrier) {
                        l.ParentId = null;
                    }
                    if (_control.DispatchBoardType == DispatchBoardType.Broker) {
                        l.ShipmentId = null;
                    }
                    l.Status = LoadStatus.None;
                });
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                return;
            }
            MessageBoxEx.Alert("Operation is not available");
        }

        public void EditCarrierAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditCarrierAction");
            var carrier = wrapper.DatCompany ?? wrapper.HistoryDatCompany;
            if (carrier == null) return;
            var clone = (DatCompanyViewModel)carrier.Clone();
            var dialog = new EditCarrierDialog(_dispatchService, clone,
                _control.SelectedBrokerCompany, DispatchBoardViewModel.Instance.ActualCompanies);
            dialog.ShowDialog();
        }

        public void EditDatDriverAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditDatDriverAction");
            var clone = (DatDriverViewModel)wrapper.DatDriver.Clone();
            var dialog = new EditDatDriverDialog(_dispatchService, clone);
            dialog.ShowDialog();
        }

        /// <summary>
        ///     Sets comment to a parent and childrens </summary>
        public void EditComments1Action(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditComments1Action");
            // saving loads
            var loads = new List<LoadViewModel>();
            // editing load
            var load = wrapper.Load;
            // parent wrapper
            var parentWrapper = wrapper.Load.Parent1;
            // parent load
            var parent = parentWrapper?.Load;
            if (parent != null) {
                // adds parent
                loads.Add(parent);
                // adds children
                loads.AddRange(parentWrapper.Children.Select(e => e.Load));
            } else {
                // adds target
                loads.Add(load);
            }
            if (wrapper.Children != null) {
                // clones
                loads.AddRange(wrapper.Children.Select(e => e.Load));
            }
            var editor = new RichTextEditor();
            // sets view model
            editor.ViewModel = load.Comments;
            editor.Title = "Edit Comments";

            if (editor.ShowDialog() == true) {
                var clones = loads.Select(e => (LoadViewModel)e.Clone()).ToList();
                // sets edited value to clones
                clones.ForEach(e => e.Comments = editor.ViewModel);
                // saves
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                _control.BusyStart();
                loads.ForEach(e => e.Saving = true);
            }
        }

        /// <summary>
        ///     Sets comment to a parent and childrens </summary>
        public void EditComments2Action(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditComments2Action");
            // saving loads
            var loads = new List<LoadViewModel>();
            // editing load
            var load = wrapper.Load;
            // parent wrapper
            var parentWrapper = wrapper.Load.Parent1;
            // parent load
            var parent = parentWrapper?.Load;
            if (parent != null) {
                // adds parent
                loads.Add(parent);
                // adds children
                loads.AddRange(parentWrapper.Children.Select(e => e.Load));
            } else {
                // adds target
                loads.Add(load);
            }
            if (wrapper.Children != null) {
                // clones
                loads.AddRange(wrapper.Children.Select(e => e.Load));
            }
            var editor = new RichTextEditor();
            // sets view model
            editor.ViewModel = load.Comments2;
            editor.Title = "Edit Comments";

            if (editor.ShowDialog() == true) {
                var clones = loads.Select(e => (LoadViewModel)e.Clone()).ToList();
                // sets edited value to clones
                clones.ForEach(e => e.Comments2 = editor.ViewModel);
                // saves
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                _control.BusyStart();
                loads.ForEach(e => e.Saving = true);
            }
        }

        /// <summary>
        ///     Sets comment to a parent and childrens </summary>
        public void EditConsolidationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditConsolidationAction");
            // saving loads
            var loads = new List<LoadViewModel>();
            // editing load
            var load = wrapper.Load;
            // parent wrapper
            var parentWrapper = wrapper.Load.Parent1;
            // parent load
            var parent = parentWrapper?.Load;
            if (parent != null) {
                // adds parent
                loads.Add(parent);
                // adds children
                loads.AddRange(parentWrapper.Children.Select(e => e.Load));
            } else {
                // adds target
                loads.Add(load);
            }
            if (wrapper.Children != null) {
                // clones
                loads.AddRange(wrapper.Children.Select(e => e.Load));
            }
            var editor = new RichTextEditor();
            // sets view model
            editor.ViewModel = load.Consolidation;
            editor.Title = "Edit Consolidation";

            if (editor.ShowDialog() == true) {
                var clones = loads.Select(e => (LoadViewModel)e.Clone()).ToList();
                // sets edited value to clones
                clones.ForEach(e => e.Consolidation = editor.ViewModel);
                // saves
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                _control.BusyStart();
                loads.ForEach(e => e.Saving = true);
            }
        }

        public async void EditConfirmationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditConfirmationAction");
            var clone = (LoadViewModel)wrapper.Load.Clone();
            if (wrapper.ActualCompany == null) {
                MessageBoxEx.Alert("Actual company was not selected");
                return;
            }

            clone.ShowPickupAddress = MessageBox.Show("Show pickup address?", "Create Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            clone.EmailConfirmationSentDate = null;

            _control.BusyStart($"Loading details of '{wrapper.DatCompany.Name}'...");



            if (wrapper.DatCompany.McNumber == null && wrapper.DatCompany.DotNumber == null) {
                throw new Exception("Undefined DOT# and MC#");
            }

            SearchByMcNumberResponse result = await App.Services.GetRequiredService<DatCompanyDataService>().SearchByMcDotNumberAsync(
                new SearchByMcDotNumberRequest() {
                    DOT = wrapper.DatCompany.DotNumber,
                    MC = wrapper.DatCompany.McNumber
                }
            );

            _control.BusyFinish();
            Dictionary<string, string> companyDetails;
            if (result.IsSuccessed) {
                companyDetails = result.Fields;
            } else {
                MessageBoxEx.Warning(result.ErrorMessage);
                companyDetails = new Dictionary<string, string>() {
                    { "Physical Address:", "-" },
                    { "Mailing Address:", "-" },
                };
            }

            // Generates RadDocument
            var document = _reportService.GenerateConfirmation(wrapper.DatCompany, wrapper.ActualCompany, companyDetails, clone, wrapper.Children.Select(e => e.Load).ToList());
            var editor = new RichTextEditor();
            var provider = new XamlFormatProvider();
            editor.ViewModel = provider.Export(document);
            editor.Title = "Edit Confirmation";

            if (editor.ShowDialog() == true) {
                _dispatchService.SaveLoad(
                    clone,
                    async wrapper2 => {
                        await _control.Dispatcher.InvokeAsync(async () => {
                            if (wrapper2.Status == ResponseStatus.Success) {
                                // Sets file for load
                                // Uploads file
                                var l = wrapper2.Data;

                                var doc = provider.Import(editor.ViewModel);

                                var binary = _reportService.GenerateConfirmationPdf(doc, l);

                                var filename = $"confirmation_{_reportService.ToFileName(wrapper.DatCompany.Name)}_{MathHelper.DateTimeNowToScale62()}_tmp.pdf";
                                l.ConfirmationFile = Path.GetFileName(filename);

                                var fileViewModel = new FileViewModel() {
                                    EntityId = l.EntityId,
                                    Name = filename,
                                    Data = binary
                                };
                                if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                                    _dispatchService.SaveLoadAndAttachFile(l, fileViewModel, _control.SaveCompleted);
                                    _control.BusyStart();
                                }
                            } else {
                                await _control.SaveCompleted(wrapper2);
                            }
                        });
                    });

                wrapper.Load.Saving = true;
                _control.BusyStart();
            }
        }

        public void EditDriverAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditDriverAction");
            if (wrapper.Driver == null) return;
            var clone = (DriverViewModel)wrapper.Driver.Clone();
            var dialog = new EditDriverDialog(_dispatchService, clone,
                new ObservableCollection<TruckViewModel>(DispatchBoardViewModel.Instance.Trucks.Where(trailer =>
                    trailer.EntityId == clone.TruckId || DispatchBoardViewModel.Instance.Drivers.Any(d => d.TruckId == trailer.EntityId) == false)));
            dialog.ShowDialog();
        }

        public void EditExpensesAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditExpensesAction");
            var clone = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dlg = new EditExpensesDialog(clone.LineHaul);
            dlg.ViewModel = clone.Expenses.Where(e => e.Type != ExpenseType.none).Select(e => (ExpenseViewModel)e.Clone()).ToList();
            if (dlg.ShowDialog() == true) {
                clone.Expenses.Where(e => e.Type != ExpenseType.none).ForEach(old => old.Assign(dlg.ViewModel.First(@new => @new.Type == old.Type)));
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
            }
        }

        public void EditGainsAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditGainsAction");
            var clone = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dlg = new EditGainsDialog(clone.LineHaul);
            dlg.ViewModel = clone.Gains.Where(e => e.Type != GainType.none).Select(e => (GainViewModel)e.Clone()).ToList();
            if (dlg.ShowDialog() == true) {
                clone.Gains.Where(e => e.Type != GainType.none).ForEach(old => old.Assign(dlg.ViewModel.First(@new => @new.Type == old.Type)));
                _dispatchService.SaveLoad(clone, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
            }
        }

        public void EditLoadAction(Wrapper1LoadViewModel wrapper, bool isReadOnly = false, DispatchBoardViewModel dispatchViewModel = null) {
            StackLogger.Push($"EditLoadAction");
            if (wrapper == null) return;
            if (dispatchViewModel == null) {
                dispatchViewModel = DispatchBoardViewModel.Instance;
            }
            var clone = GetParentIfExists(wrapper.Load);
            EditLoadDialog dlg;
            var company = _control.DispatchBoardType == DispatchBoardType.Broker ||
                _control.DispatchBoardType == DispatchBoardType.BrokerAccount ?
                _control.SelectedBrokerCompany : _control.SelectedCarrierCompany;
            if (wrapper.Load.BoardType == DispatchBoardType.Carrier ||
                wrapper.Load.BoardType == DispatchBoardType.CarrierAccount) {
                // Carrier Board
                if (clone.ContainerType == ContainerType.None || clone.ContainerType == ContainerType.Shipment) {
                    dlg = new EditLoadDialog(_dispatchService,
                        _manager.GetCallService, _mapper, (LoadViewModel)clone.Clone(),
                        dispatchViewModel.Warehouses,
                        dispatchViewModel.Trailers, dispatchViewModel.Trucks,
                        dispatchViewModel.Drivers, dispatchViewModel.DatCompanies,
                        dispatchViewModel.DatDrivers, dispatchViewModel.ActualCompanies, company);
                } else {
                    List<LoadViewModel> children = dispatchViewModel.Loads
                        .Where(l => l.ParentId == clone.EntityId).Select(l => (LoadViewModel)l.Clone()).OrderBy(e => e.IndexString).ToList();
                    dlg = new EditLoadDialog(_dispatchService, _manager.GetCallService, _mapper, (LoadViewModel)clone.Clone(),
                        children, dispatchViewModel.Warehouses,
                        dispatchViewModel.Trailers, dispatchViewModel.Trucks,
                        dispatchViewModel.Drivers, dispatchViewModel.DatCompanies,
                        dispatchViewModel.DatDrivers, dispatchViewModel.ActualCompanies, company);
                }
            } else {
                // Broker Board
                List<LoadViewModel> children = dispatchViewModel.Loads
                    .Where(l => l.ShipmentId == clone.EntityId || l.ParentId == clone.EntityId).Select(l => (LoadViewModel)l.Clone()).OrderBy(e => e.IndexString).ToList();
                dlg = new EditLoadDialog(_dispatchService, _manager.GetCallService, _mapper, (LoadViewModel)clone.Clone(),
                    children, dispatchViewModel.Warehouses,
                    dispatchViewModel.Trailers, dispatchViewModel.Trucks,
                    dispatchViewModel.Drivers, dispatchViewModel.DatCompanies,
                    dispatchViewModel.DatDrivers, dispatchViewModel.ActualCompanies, company);
            }
            dlg.IsReadOnly = isReadOnly;
            dlg.Show();
            dlg.Closed += (sender, e) => {
                if (dlg.Ok) {
                    wrapper.Load.Saving = true;
                }
            };
        }

        public void EditLineHaulAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EditLineHaulAction");
            var clonedWrapper = new Wrapper1LoadViewModel() {
                Load = (LoadViewModel)wrapper.Load.Clone(),
                ActualCompany = (ActualCompanyViewModel)wrapper.ActualCompany?.Clone(),
                DatCompany = (DatCompanyViewModel)wrapper.DatCompany?.Clone(),
            };
            var dlg = new EditLineHaulDialog(_dispatchService);
            dlg.Model = (LoadViewModel)GetParentIfExists(clonedWrapper.Load).Clone();
            if (dlg.ShowDialog() == true && _control.DispatchBoardType == DispatchBoardType.Broker) {
                clonedWrapper.Load = dlg.Model;
                CreateConfirmationAction(clonedWrapper);
            }
        }

        public void FileLoad(FileViewModel fileViewModel) {
            StackLogger.Push($"FileLoad");
            _dispatchService.FileLoad(fileViewModel, FileLoadCompeleted);
            _control?.BusyStart("File downloading...");
        }

        [DllImport("User32.dll", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        private const uint SW_RESTORE = 0x09;
        [DllImport("User32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        public async Task FileLoadCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"FileLoadCompeleted");
                await Application.Current.Dispatcher.InvokeAsync((Action)(() => FileLoadCompeleted(wrapper)));
                return;
            }
            _control?.BusyFinish();
            if (wrapper.Status == ResponseStatus.Success) {
                if (wrapper.Data.Size == 0) {
                    MessageBoxEx.Alert("File does not exists");
                    return;
                }
                try {
                    var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                    if (Directory.Exists(path) == false) {
                        Directory.CreateDirectory(path);
                    }
                    var fullPath = Path.Combine(path, wrapper.Data.Name);
                    File.WriteAllBytes(fullPath, wrapper.Data.Data);
                    var process = new Process {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path, UseShellExecute = true }
                    };
                    process.Start();
                } catch (IOException exception) {
                    if ((exception.HResult & 0xFFFF) == 0x20) {
                        MessageBoxEx.Error("Your file is already open!");
                        var proc = Process.GetProcesses().FirstOrDefault(e =>
                            wrapper.Data.Name.ToLower().EndsWith(".pdf") &&
                            e.MainWindowTitle.Contains(wrapper.Data.Name));
                        if (proc != null) {
                            IntPtr hWnd = proc.MainWindowHandle;
                            if (hWnd != IntPtr.Zero) {
                                SwitchToThisWindow(hWnd, true);
                                ShowWindow(hWnd, SW_RESTORE);
                            }
                        }
                    } else {
                        MessageBoxEx.Error(exception.Message);
                    }
                } catch (Exception exception) {
                    MessageBoxEx.Error(exception.Message);
                }
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
        }

        public void MoveToAccountingAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"MoveToAccountingAction");
            if (MessageBox.Show($"Do not forget to link cargo with shipments\nDo you want to continue?",
                "Alert",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information) == MessageBoxResult.Cancel) {
                return;
            }
            if (MessageBox.Show("Please confirm that you want to move load(s) to accounting", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clones = new List<LoadViewModel>() { (LoadViewModel)wrapper.Load.Clone() };
                clones.AddRange(Wrapper1LoadViewModel.GetRecursiveChildren(wrapper).Select(w => (LoadViewModel)w.Load.Clone()));
                foreach (var clone in clones) {
                    switch (clone.BoardType) {
                        case DispatchBoardType.Carrier:
                            clone.SaveHistory(wrapper.Driver, wrapper.Truck, wrapper.Trailer, wrapper.Shipment, wrapper.Warehouse);
                            clone.BoardType = DispatchBoardType.CarrierAccount;
                            break;

                        case DispatchBoardType.Broker:
                            clone.SaveHistory(wrapper.DatCompany);
                            clone.BoardType = DispatchBoardType.BrokerAccount;
                            break;
                    }
                    // action #1
                    clone.BoardChanged = DateTime.Now;
                    clone.Status = LoadStatus.Canceled1;
                    clone.LineHaul = null;
                    clone.CarrierViewTracking = null;
                }
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                _control.BusyStart("move to accounting...");
            }
        }

        public void MoveToAccountingTNUAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"MoveToAccountingTNUAction");
            var trailer = DispatchBoardViewModel.Instance.Trailers.FirstOrDefault(e => String.Equals(e.Number, "TNU", StringComparison.OrdinalIgnoreCase));
            if (trailer == null) {
                MessageBoxEx.Error("Please create 'TNU' trailer");
                return;
            }
            if (MessageBox.Show("Please confirm that you want to move load(s) to accounting (TNU)", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clones = new List<LoadViewModel>() { (LoadViewModel)wrapper.Load.Clone() };
                clones.AddRange(Wrapper1LoadViewModel.GetRecursiveChildren(wrapper).Select(w => (LoadViewModel)w.Load.Clone()));
                foreach (var clone in clones) {
                    switch (clone.BoardType) {
                        case DispatchBoardType.Carrier:
                            clone.BoardType = DispatchBoardType.CarrierAccount;
                            clone.TrailerId = trailer.EntityId;
                            clone.HistoryTrailerId = trailer.EntityId;
                            clone.HistoryAssignedTrailerId = trailer.EntityId;
                            clone.AssignedTrailerDate = DateTime.UtcNow;
                            clone.BindDate = DateTime.UtcNow;
                            clone.Status = LoadStatus.Delivered;
                            break;
                        case DispatchBoardType.Broker:
                            throw new Exception();
                    }
                }
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                _control.BusyStart("move to accounting TNU...");
            }
        }

        public void MoveToAccountingAsDeliveredAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"MoveToAccountingAsDeliveredAction");
            if (MessageBox.Show($"Do not forget to link cargo with shipments\nDo you want to continue?",
                "Alert",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information) == MessageBoxResult.Cancel) {
                return;
            }
            if (MessageBox.Show("Please confirm that you want to move load(s) to accounting", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
                var clones = new List<LoadViewModel>() { (LoadViewModel)wrapper.Load.Clone() };
                clones.AddRange(Wrapper1LoadViewModel.GetRecursiveChildren(wrapper).Select(w => (LoadViewModel)w.Load.Clone()));
                foreach (var clone in clones) {
                    var w = DispatchBoardViewModel.Instance.Wrappers.ToList().First(e => e.Load.EntityId == clone.EntityId);
                    switch (clone.BoardType) {
                        case DispatchBoardType.Carrier:
                            clone.SaveHistory(w.Driver, w.Truck, w.Trailer, w.Shipment, w.Warehouse);
                            clone.BoardType = DispatchBoardType.CarrierAccount;
                            break;

                        case DispatchBoardType.Broker:
                            clone.SaveHistory(w.DatCompany);
                            clone.BoardType = DispatchBoardType.BrokerAccount;
                            break;
                    }
                    clone.BoardChanged = DateTime.Now;
                    // action #2
                    if (DisplayAttribute.GetOrder(clone.Status) < DisplayAttribute.GetOrder(LoadStatus.Delivered)) {
                        clone.Status = LoadStatus.Delivered;
                    }
                }
                _dispatchService.SaveLoads(clones, _control.SaveCompleted);
                _control.BusyStart("move to accounting...");
            }
        }

        public void SetDestinationWarehouseAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"SetDestinationWarehouseAction");
            if (CheckConfirmation(GetParentIfExists(wrapper)) == false) {
                return;
            }
            if (CheckMultistops(wrapper) == false) {
                return;
            }
            var dlg = new SearchWarehouseDialog(_dispatchService, _manager, clearable: true);
            if (dlg.ShowDialog() == true) {
                var warehouse = dlg.Model.SelectedWrapper;
                if (warehouse != null) {
                    wrapper.Load.DestinationWarehouseId = warehouse.EntityId;
                } else {
                    wrapper.Load.DestinationWarehouseId = null;
                }
                _dispatchService.SaveLoad(wrapper.Load, _control.SaveCompleted);
            }
        }

        public void MoveToWarehouseAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"MoveToWarehouseAction");
            if (CheckConfirmation(GetParentIfExists(wrapper)) == false) {
                return;
            }
            if (CheckMultistops(wrapper) == false) {
                return;
            }
            var dlg = new SearchWarehouseDialog(_dispatchService, _manager);
            if (dlg.ShowDialog() == true) {
                var warehouse = dlg.Model.SelectedWrapper;
                BindLoads(
                    ReplaceContainers(new List<Wrapper1LoadViewModel>() { wrapper }).Select(e => (LoadViewModel)e.Load.Clone()).ToList(),
                    warehouse);
            }
        }

        public void MoveToYardAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"MoveToYardAction");
            if (wrapper.Trailer == null) return;
            var clone = (TrailerViewModel)wrapper.Trailer.Clone();
            if (clone != null) {
                var dlg = new SearchYardDialog(_dispatchService, _manager);
                if (dlg.ShowDialog() == true) {
                    var yard = dlg.Model.SelectedWrapper;
                    clone.YardId = yard.EntityId;
                    // saves
                    _dispatchService.SaveTrailer(clone, _control.SaveCompleted);
                    _control.BusyStart();
                }
            }
        }

        public void OpenCheckAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenCheckAction");
            if (String.IsNullOrEmpty(wrapper.Load.CheckFile) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.EntityId,
                    Name = wrapper.Load.CheckFile
                });
            }
        }

        public void OpenTrailerSchemeAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenTrailerSchemeAction");
            if (String.IsNullOrEmpty(wrapper.Trailer.SchemeFile) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Trailer.EntityId,
                    Name = wrapper.Trailer.SchemeFile
                });
            }
        }

        public void OpenConfirmationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenConfirmationAction");
            if (String.IsNullOrEmpty(wrapper.Load.ConfirmationFileView) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.ConfirmationEntityIdView,
                    Name = wrapper.Load.ConfirmationFileView
                });
            }
        }

        public void OpenInvoiceAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenInvoiceAction");
            if (String.IsNullOrEmpty(wrapper.Load.InvoiceFileView) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.EntityIdView,
                    Name = wrapper.Load.InvoiceFileView
                });
            }
        }

        public void OpenPhotoAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenPhotoAction");
            if (String.IsNullOrEmpty(wrapper.Load.PhotoFileView) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.EntityIdView,
                    Name = wrapper.Load.PhotoFileView
                });
            }
        }

        public void OpenPODAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenPODAction");
            if (String.IsNullOrEmpty(wrapper.Load.PODFileView) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.EntityIdView,
                    Name = wrapper.Load.PODFileView
                });
            }
        }

        public void OpenBOLAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenBOLAction");
            if (String.IsNullOrEmpty(wrapper.Load.BOLFileView) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.EntityIdView,
                    Name = wrapper.Load.BOLFileView
                });
            }
        }

        public void OpenPaperworkAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenPaperworkAction");
            if (String.IsNullOrEmpty(wrapper.Load.PaperworkFileView) == false) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)wrapper.Load.EntityIdView,
                    Name = wrapper.Load.PaperworkFileView
                });
            }
        }

        public void OpenTrailerAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenTrailerAction");
            if (wrapper.Trailer != null) {
                var dlg = new EquipmentsDialog(this._dispatchService, _settings, wrapper.ActualCompany);
                dlg.Model.SearchTextTrailer = wrapper.Trailer.Number;
                dlg.Show();
            }
        }

        public void OpenWebSetupPacketAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"OpenWebSetupPacketAction");
            Process.Start(new ProcessStartInfo(new Uri(AutoCall.Common.Settings.WebUrl + $"CarrierRegistration/Edit/?CarrierId={(wrapper.DatCompany ?? wrapper.HistoryDatCompany).EntityId}&step=2").AbsoluteUri) { UseShellExecute = true });
        }

        /// <summary>
        ///     Moves to history. Sets status paid or canceled2 </summary>
        /// <param name="wrapper"></param>
        public void PaidAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"PaidAction");
            var load = wrapper.Load;
            // TODO: временно отключаю проверку, что бы canceled1 отправить в историю
            //if (CheckParent(load) == false) {
            //    return;
            //}
            var clone = (LoadViewModel)load.Clone();
            // TODO: --||--
            //if (load.ParentId != null) {
            //    MessageBoxEx.Alert("The shipment will be hidden, when full cargo will be delivered");
            //    return;
            //}
            if (clone.Status == LoadStatus.Canceled1) {
                clone.Status = LoadStatus.Canceled2;
            } else {
                clone.Status = LoadStatus.Completed;
            }
            clone.BoardChanged = DateTime.UtcNow;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            wrapper.Load.Saving = true;
            _control.BusyStart();
            // TODO: Implement observable dictionary. Refresh counts of loads in filter
        }

        public void UnpaidAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"UnpaidAction");
            var load = wrapper.Load;
            if (CheckParent(load) == false) {
                return;
            }
            var clone = (LoadViewModel)load.Clone();
            if (load.ParentId != null) {
                MessageBoxEx.Alert("The shipment will be hidden, when full cargo will be delivered");
                return;
            }
            clone.Status = LoadStatus.Unpaid;
            clone.BoardChanged = DateTime.UtcNow;
            _dispatchService.SaveLoad(clone, _control.SaveCompleted);
            wrapper.Load.Saving = true;
            _control.BusyStart();
        }

        public void MultiPaidAction(List<Wrapper1LoadViewModel> wrappers) {
            StackLogger.Push($"MultiPaidAction");
            var clones = new List<LoadViewModel>();
            foreach (var wrapper in wrappers) {
                var load = wrapper.Load;
                // TODO: временно отключаю проверку, что бы canceled1 отправить в историю
                //if (CheckParent(load) == false) {
                //    return;
                //}
                var clone = (LoadViewModel)load.Clone();
                // TODO: --||--
                //if (load.ParentId != null) {
                //    MessageBoxEx.Alert("The shipment will be hidden, when full cargo will be delivered");
                //    return;
                //}
                if (clone.Status == LoadStatus.Canceled1) {
                    clone.Status = LoadStatus.Canceled2;
                } else {
                    clone.Status = LoadStatus.Completed;
                }
                clone.BoardChanged = DateTime.UtcNow;
                wrapper.Load.Saving = true;
                clones.Add(clone);
            }
            _dispatchService.SaveLoads(clones, _control.SaveCompleted);
            _control.BusyStart();
        }

        public void MultiUnpaidAction(List<Wrapper1LoadViewModel> wrappers) {
            StackLogger.Push($"MultiUnpaidAction");
            var clones = new List<LoadViewModel>();
            foreach (var wrapper in wrappers) {
                var load = wrapper.Load;
                if (CheckParent(load) == false) {
                    return;
                }
                var clone = (LoadViewModel)load.Clone();
                if (load.ParentId != null) {
                    MessageBoxEx.Alert("The shipment will be hidden, when full cargo will be delivered");
                    return;
                }
                clone.Status = LoadStatus.Unpaid;
                clone.BoardChanged = DateTime.UtcNow;
                wrapper.Load.Saving = true;
                clones.Add(clone);
            }
            _dispatchService.SaveLoads(clones, _control.SaveCompleted);
            _control.BusyStart();
        }

        public void ResendInvoiceAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ResendInvoiceAction");
            SendInvoiceAction(wrapper);
        }

        public void SendConfirmationAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"SendConfirmationAction");
            var clone = (LoadViewModel)wrapper.Load.Clone();
            if (clone.ConfirmationFile == null) {
                return;
            }
            var emails = wrapper.DatCompany.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email).ToList();

            var dialog = new EmailListDialog($"Please confirm to send email(s) to {String.Join(", ", emails.Select(em => $"'{em}'"))} with load {clone.IndexString} {clone.Direction}", "AutoCall - Send Confirmation", emails.ToList());
            if (dialog.ShowDialog() == true) {
                _dispatchService.ConfirmationSend(clone.EntityId, dialog.Model.EmailList, _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();

                // merges contacts
                if (dialog.Model.SaveContacts) {
                    var contacts = new ContactsViewModel();
                    foreach (var email in dialog.Model.EmailList) {
                        contacts.AddEmail(email);
                    }
                    if (wrapper.DatCompany.Contacts.Merge(contacts)) {
                        _dispatchService.SaveDatCompany(wrapper.DatCompany, async (model) => { });
                    }
                }
                return;
            }
        }

        public void SendInvoiceAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"SendInvoiceAction");
            if (wrapper.Load.InvoiceFile == null) {
                return;
            }

            List<string> emails = new List<string>();
            // TODO: СДЕЛАЙ СВЯЗЬ С DATCOMPANIES!!!
            /*if (wrapper.Company != null) {
                // uses contacts from Company Instance
                emails.AddRange(wrapper.Company.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email));
            } else*/
            {
                // uses contacts from Load Instance, old version, without Company object
                emails.AddRange(wrapper.Load.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email));
                if (wrapper.DatCompany != null) {
                    emails.AddRange(wrapper.DatCompany.Contacts.Contacts.OfType<ContactEmailViewModel>().Select(em => em.Email));
                }
            }

            var dialog = new SendInvoiceDialog(emails.ToList(), wrapper.Load, wrapper.ActualCompany.FactoringCompany);
            if (dialog.ShowDialog() == true) {
                // to Send Invoice and to Change Status to BILLED
                _dispatchService.InvoiceSend(
                    wrapper.Load.EntityId,
                    dialog.Model.EmailList,
                    (dialog.Model.FactoringCompanyEmail != null && dialog.Model.FactoringCompanyEmail.Item2) ?
                        dialog.Model.FactoringCompanyEmail?.Item : null,
                    dialog.Model.SendInvoice,
                    dialog.Model.SendPOD,
                    dialog.Model.SendConfirmation,
                    dialog.Model.SendLumper,
                    dialog.Model.Merged,
                    dialog.Model.Text,
                    _control.SaveCompleted);
                wrapper.Load.Saving = true;
                _control.BusyStart();
                // merges contacts
                if (dialog.Model.SaveContacts) {
                    // saves broker
                    var broker = DispatchBoardViewModel.Instance.DatCompanies.FirstOrDefault(e => e.EntityId == wrapper.Load.DatBrokerId);
                    if (broker != null) {
                        var clone = (DatCompanyViewModel)broker.Clone();
                        var contacts = new ContactsViewModel();
                        foreach (var email in dialog.Model.EmailList) {
                            contacts.AddEmail(email);
                        }
                        if (clone.Contacts.Merge(contacts)) {
                            _dispatchService.SaveDatCompany(clone, async (model) => { });
                        }
                    }
                }
                return;
            }
        }

        public void CopySummaryAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"CopySummaryAction");
            var lines = new List<string>();
            if (wrapper.Children.Any()) {
                var pickupStr = "Pickup";
                var deliveryStr = "Delivery";
                for (int i = 0; i < wrapper.Children.Count; i++) {
                    Wrapper1LoadViewModel child = wrapper.Children[i];
                    lines.Insert(i, $"{pickupStr}({{0}}): {child.Load.OriginCity}; {child.Load.PickupFullDateTime}");
                    lines.Add($"{deliveryStr}({{0}}): {child.Load.OriginCity}; {child.Load.PickupFullDateTime}"); // addes to last
                }
                lines = lines.Distinct().ToList();
                var oIndex = 1;
                var dIndex = 1;
                for (int i = 0; i < lines.Count; i++) {
                    if (lines[i].StartsWith(pickupStr)) {
                        lines[i] = String.Format(lines[i], oIndex++);
                    } else if (lines[i].StartsWith(deliveryStr)) {
                        lines[i] = String.Format(lines[i], dIndex++);
                    } else {
                        throw new Exception();
                    }
                }
            } else {
                var l = wrapper.Load;
                var line1 = $"Pickup: {l.OriginCity}; {l.PickupFullDateTime}";
                var line2 = $"Delivery: {l.DestinationCity}; {l.DeliveryFullDateTime}";
                lines.Add(line1);
                lines.Add(line2);
            }
            Clipboard.SetText(String.Join("\n", lines));
        }

        public void ShowTripAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ShowTripAction");
            var load = wrapper.Load;
            // V1: MapRouteDialog (EO.WebBrowser, Dispatch/RouteLocation?t=…&DispatchLoadId=) — теперь /Map сервера в WebView2 (MapRouteHelper)
            _ = MapRouteHelper.OpenAsync(origin: load.OriginLocation, destination: load.DestinationLocation);
        }

        /// <summary>
        ///     Show Rates — QuickRateSearchDialog (ставки DAT) модуля AutoCall, вырезан (AI/wiki/plan.md этап 7); пункты меню закомментированы </summary>
        public void ShowRatesAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ShowRatesAction");
            //var load = wrapper.Load;

            //QuickRateSearchDialog dialog = new QuickRateSearchDialog() {
            //    Model = new QuickRateSearchViewModel() {
            //        Origin = new DatPowerLocationViewModel() {
            //            Lat = load.OriginLocation.Latitude,
            //            Long = load.OriginLocation.Longitude,
            //            Name = load.OriginCity,
            //        },
            //        Destination = new DatPowerLocationViewModel() {
            //            Lat = load.DestinationLocation.Latitude,
            //            Long = load.DestinationLocation.Longitude,
            //            Name = load.DestinationCity,
            //        },
            //        SelectedEquipment = load.Equipment,
            //    }
            //};
            //dialog.Show();
        }

        public void PreviewImagesAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"PreviewImagesAction");
            var carusel = new RadCaruselWindow(this._dispatchService);
            _control.BusyStart();
            _dispatchService.FilesLoad(wrapper.Load.EntityId, true, async viewModelWrapper => {
                await _control.Dispatcher.InvokeAsync(() => {
                    _control.BusyFinish();
                    foreach (var f in viewModelWrapper.Data) {
                        f.DataImage = ImageHelper.ToImage(f.Data);
                    }
                    if (carusel.SetImageItemsSource(viewModelWrapper.Data)) {
                        carusel.ShowDialog();
                    } else {
                        MessageBoxEx.Warning("Pictures were not found.");
                    }
                });
            });
        }

        public void UnassignAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"UnassignAction");
            if (wrapper.Load.BoardType != _control.DispatchBoardType) {
                MessageBoxEx.Alert("Operation is not available");
                return;
            }
            var clones = GetUnassigningLoads(new List<Wrapper1LoadViewModel>() { wrapper });
            clones.ForEach(c => {
                c.Layout.Clear();
                c.CurrentLocation = null;
                c.ShipmentId = null;
                c.HistoryShipmentId = null;
                c.TrailerId = null;
                c.HistoryTrailerId = null;
                c.HistoryAssignedTrailerId = null;
                c.TrailerSchemeDate = null;
                c.TrailerSchemeFile = null;
                c.TrailerSchemeId = null;
                c.WarehouseId = null;
                c.DatDriverId = null;
                c.DatCompanyId = null;
                c.OldOrderId = null;
                c.WasAtPickup = false;
                c.WasAtDelivery = false;
                if (c.BoardType == DispatchBoardType.Broker) {
                    c.BOLDate = null;
                    c.BOLFile = null;
                    c.ConfirmationDate = null;
                    c.ConfirmationFile = null;
                    c.EmailConfirmationSent = null;
                    c.EmailConfirmationSentDate = null;
                    c.ConfirmedDate = null;
                    c.ConfirmedFile = null;
                }
                c.Status = LoadStatus.None;
            });
            _dispatchService.SaveLoads(clones, _control.SaveCompleted);
            wrapper.Load.Saving = true;
            _control.BusyStart("Unassigning...");
        }

        public void UndoAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"UndoAction");
            _dispatchService.UndoApply<LoadViewModel>(wrapper.Load.EntityId, EventTypeModel.BookedLoad, _control.SaveCompleted);
        }

        public void ViewCarrierAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ViewCarrierAction");
            if ((DispatchHelper.GetDatCompanyWrapper(wrapper) is var w) && w.DatCompany != null) {
                var dialog = new DetailsCarrierDialog(
                    _dispatchService, _manager.GetCallService, _mapper,
                    w.DatCompany,
                    DispatchBoardViewModel.Instance.DatDrivers,
                    DispatchBoardViewModel.Instance.Loads,
                    DispatchBoardViewModel.Instance.ActualCompanies,
                    w.ActualCompany);
                dialog.ShowDialog();
            }
        }

        public void ViewHistoryCarrierAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ViewHistoryCarrierAction");
            if (wrapper.HistoryDatCompany != null) {
                var dialog = new DetailsCarrierDialog(
                    _dispatchService,
                    _manager.GetCallService,
                    _mapper,
                    wrapper.HistoryDatCompany,
                    DispatchBoardViewModel.Instance.DatDrivers,
                    DispatchBoardViewModel.Instance.Loads,
                    DispatchBoardViewModel.Instance.ActualCompanies,
                    wrapper.ActualCompany);
                dialog.ShowDialog();
            }
        }

        public void ViewHistoryLocationsAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ViewHistoryLocationsAction");
            var load = wrapper.Load;
            var dialog = new HistoryLoadLocationsDialog(_dispatchService, load);
            dialog.Show();
        }

        public void ViewHistoryChangesAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ViewHistoryChangesAction");
            var load = wrapper.Load;
            var dialog = new HistoryLoadChangesDialog(_dispatchService, load);
            dialog.Show();
        }

        public void ViewTrailerAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"ViewTrailerAction");
            if (wrapper.Load.Parent2 != null) {
                ViewCarrierAction(wrapper.Load.Parent2);
            } else
            if ((DispatchHelper.GetTrailerWrapper(wrapper) is var w) && w.Trailer != null) {
                var window = new DetailsTrailerLayoutDialog(
                    _dispatchService, _manager.GetCallService, _mapper,
                    w.Driver,
                    w.Truck,
                    w.Trailer,
                    DispatchBoardViewModel.Instance.Loads,
                    w.ActualCompany);
                window.ShowDialog();
            }
        }

        public void EnableCarrierViewTrackerAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"EnableCarrierViewTrackerAction");

            var load = (LoadViewModel)GetParentIfExists(wrapper.Load).Clone();
            var dialog = new EnableTrackerDialog(_dispatchService, _manager.GetCallService, _mapper);
            if (dialog.ShowDialog() == true) {
                this.AddSaving(wrapper);
                _control.BusyStart();
                _dispatchService.EnableCarrierViewTracker(dialog.Model.TextPhone, wrapper.Load.EntityId, this.SaveCompleted);
            }
        }

        public void DisableCarrierViewTrackerAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"DisableCarrierViewTrackerAction");
            if (wrapper.Load.TrackerId == null) {
                throw new Exception("CarrierView# was not defined");
            }
            this.AddSaving(wrapper);
            _dispatchService.DisableCarrierViewTracker(wrapper.Load.EntityId, this.SaveCompleted);
        }

        public void SyncCarrierViewTrackerAction(Wrapper1LoadViewModel wrapper) {
            StackLogger.Push($"SyncCarrierViewTrackerAction");
            if (wrapper.Load.TrackerId == null) {
                throw new Exception("TrackerId was not defined");
            }
            this.AddSaving(wrapper);
            _dispatchService.SyncCarrierViewTracker(wrapper.Load.TrackerId.Value, async (w) => {
                if (w.Data == null) {
                    MessageBoxEx.Warning("Tracker was deleted");
                }
                await this.SaveCompleted(w);
            });
        }

        public void GoToLinkedWrapperAction(Wrapper1LoadViewModel wrapper) {
            if (wrapper.LinkedWrapperCount == 1) {
                var mainWindow = (MainWindow)App.Current.MainWindow;
                mainWindow.gridBoards.ActivePane = mainWindow._boards.First(e =>
                                                            e is DispatchBoardBase b && b.DispatchBoardType == wrapper.LinkedWrapper.Load.BoardType);
                if (mainWindow.gridBoards.ActivePane is DispatchBoard d) {
                    if (d.gridLoads != null && ((IEnumerable)d.gridLoads.ItemsSource).Cast<Wrapper1LoadViewModel>().Contains(wrapper.LinkedWrapper)) {
                        d.gridLoads.Select(new List<Wrapper1LoadViewModel>() { wrapper.LinkedWrapper });
                        d.gridLoads.ScrollIntoView(wrapper.LinkedWrapper);
                    }
                    if (d.gridPatterns != null && ((IEnumerable)d.gridPatterns.ItemsSource).Cast<Wrapper1LoadViewModel>().Contains(wrapper.LinkedWrapper)) {
                        d.gridPatterns.Select(new List<Wrapper1LoadViewModel>() { wrapper.LinkedWrapper });
                        d.gridPatterns.ScrollIntoView(wrapper.LinkedWrapper);
                    }
                }
                if (mainWindow.gridBoards.ActivePane is DispatchAccountBoard a) {
                    if (a.gridLoads != null && ((IEnumerable)a.gridLoads.ItemsSource).Cast<Wrapper1LoadViewModel>().Contains(wrapper.LinkedWrapper)) {
                        a.gridLoads.Select(new List<Wrapper1LoadViewModel>() { wrapper.LinkedWrapper });
                        a.gridLoads.ScrollIntoView(wrapper.LinkedWrapper);
                    }
                }
            }
        }

        public void ParseConfirmationAction(DispatchBoardType board) {
            StackLogger.Push($"ParseConfirmationAction");
            var dialog = new ParseConfirmationDialog(board);
            dialog.ShowDialog();
        }

        private async Task SaveCompleted<T>(ViewModelWrapper<T> w) {
            RemoveSaving();
            await _control.SaveCompleted(w);
            _control.BusyFinish();
        }

        private Wrapper1LoadViewModel SavingWrapper = null;

        private void AddSaving(Wrapper1LoadViewModel wrapper) {
            SavingWrapper = wrapper;
            wrapper.Load.Saving = true;
        }

        private void RemoveSaving() {
            if (SavingWrapper != null) {
                SavingWrapper.Load.Saving = false;
            }
        }

        #endregion [ Actions ]
    }
}