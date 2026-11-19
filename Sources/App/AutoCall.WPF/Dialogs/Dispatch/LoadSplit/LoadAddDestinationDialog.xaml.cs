using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common;
using AutoCall.Common.Enums;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class LoadAddDestinationDialog : LoadAddLocationDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        public LoadAddDestinationDialog(DispatchService service, LoadViewModel load, LoadViewModel parent) : base(service, load, parent) {

            InitializeComponent();

            Title += $" #{load.IndexString}";

            Model.Load2.DestinationLocation = null;
            Model.Load2.DestinationAddress = null;
            Model.Load2.DestinationCity = null;

            Model.Load2.Length = Model.InitialLoad.Length / 2;
            Model.Load2.Weight = Model.InitialLoad.Weight / 2;
            Model.Load2.Pallets = (short?)(Model.InitialLoad.Pallets / 2);
            Model.Load2.LineHaul = Model.InitialLoad.LineHaul / 2;

            var ts = new TimeSpan((
                (Model.Load2.DeliveryDate.Date + Model.Load2.DeliveryTo - Model.Load2.DeliveryFrom).Ticks -
                (Model.Load2.PickupDate.Date + Model.Load2.PickupTo - Model.Load2.PickupFrom).Ticks) / 2);

            var from = Model.Load2.DeliveryDate.Date + Model.Load2.DeliveryFrom - ts;
            var to = Model.Load2.DeliveryDate.Date + Model.Load2.DeliveryTo - ts;

            Model.Load2.DeliveryDate = new DateTime(from.Year, from.Month, from.Day);
            Model.Load2.DeliveryTo = to.TimeOfDay;
            Model.Load2.DeliveryFrom = from.Day == 0 ? from.TimeOfDay : to.TimeOfDay;
        }

        private void cbFullUnload_OnClick(object sender, RoutedEventArgs e) {
            if (((CheckBox)sender).IsChecked == true) {
                Model.Load2.Length = Model.InitialLoad.Length;
                Model.Load2.Weight = Model.InitialLoad.Weight;
                Model.Load2.Pallets = Model.InitialLoad.Pallets;
                Model.Load2.LineHaul = Model.InitialLoad.LineHaul / 2;
            } else {
                Model.Load2.Length = Model.InitialLoad.Length / 2;
                Model.Load2.Weight = Model.InitialLoad.Weight / 2;
                Model.Load2.Pallets = (short?)(Model.InitialLoad.Pallets / 2);
                Model.Load2.LineHaul = Model.InitialLoad.LineHaul / 2;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (CheckProperties() == false) {
                return;
            }
            if (Model.Load2.DestinationLocation == null) {
                MessageBoxEx.Alert(this, "Please enter a delivery");
                return;
            }

            if (Model.FullUnload == false) {
                if (Model.InitialLoad.ParentId == null) {
                    if (MessageBox.Show($"Please confirm the operation\n\n{Model.InitialLoad.OriginCity} → {Model.InitialLoad.DestinationCity}\n (new) {Model.Load1.OriginCity} → {Model.Load1.DestinationCity}\n  (new) {Model.Load2.OriginCity} → {Model.Load2.DestinationCity}",
                        "Alert", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK) {
                        return;
                    }
                    Model.InitialLoad.ContainerType = ContainerType.Quantity;
                    Model.Load1.EntityId = Guid.NewGuid();
                    Model.Load2.EntityId = Guid.NewGuid();
                    Model.Load1.ParentId = Model.InitialLoad.EntityId;
                    Model.Load2.ParentId = Model.InitialLoad.EntityId;

                    var loads = new List<LoadViewModel>() {
                        Model.InitialLoad,
                        Model.Load1,
                        Model.Load2,
                    };
                    //base.BusyStart();
                    //_service.SaveLoads(loads, SaveLoads_Completed);
                    DialogResult = true;
                } else {
                    if (Model.Parent != null && Model.Parent.ContainerType != ContainerType.Quantity) {
                        MessageBoxEx.Alert(this, "Operation is not available.");
                        return;
                    }
                    if (MessageBox.Show($"Please confirm the operation\n\n(edit) {Model.Load1.OriginCity} → {Model.Load1.DestinationCity}\n(new) {Model.Load2.OriginCity} → {Model.Load2.DestinationCity}",
                        "Alert", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK) {
                        return;
                    }
                    // unasignes this parent
                    Model.InitialLoad.DatCompanyId = Model.InitialLoad.DatDriverId = Model.InitialLoad.TrailerId = null;

                    Model.Load1.EntityId = Model.InitialLoad.EntityId;
                    Model.Load2.EntityId = Guid.NewGuid();

                    var loads = new List<LoadViewModel>() {
                        Model.Load1,
                        Model.Load2,
                    };
                    //base.BusyStart();
                    //_service.SaveLoads(loads, SaveLoads_Completed);
                    DialogResult = true;
                }
            } else {
                if (Model.InitialLoad.ParentId == null) {
                    if (MessageBox.Show($"Please confirm the operation\n\n{Model.InitialLoad.OriginCity} → {Model.InitialLoad.DestinationCity}\n (new) {Model.Load2.OriginCity} → {Model.Load2.DestinationCity}\n  (new) {Model.Load2.DestinationCity} → {Model.Load1.DestinationCity}",
                        "Alert", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK) {
                        return;
                    }
                    Model.InitialLoad.ContainerType = ContainerType.Distance;
                    Model.Load1.EntityId = Guid.NewGuid();
                    Model.Load2.EntityId = Guid.NewGuid();
                    Model.Load1.ParentId = Model.InitialLoad.EntityId;
                    Model.Load2.ParentId = Model.InitialLoad.EntityId;

                    Model.Load1.OriginLocation = Model.Load2.DestinationLocation;
                    Model.Load1.OriginAddress = Model.Load2.DestinationAddress;
                    Model.Load1.OriginCity = Model.Load2.DestinationCity;

                    Model.Load1.PickupDate = Model.Load2.DeliveryDate;
                    Model.Load1.PickupFrom = Model.Load2.DeliveryFrom;
                    Model.Load1.PickupTo = Model.Load2.DeliveryTo;

                    var loads = new List<LoadViewModel>() {
                        Model.InitialLoad,
                        Model.Load2,
                        Model.Load1,
                    };
                    //base.BusyStart();
                    //_service.SaveLoads(loads, SaveLoads_Completed);
                    DialogResult = true;
                } else {
                    if (Model.Parent != null && Model.Parent.ContainerType != ContainerType.Distance) {
                        MessageBoxEx.Alert(this, "Operation is not available.");
                        return;
                    }
                    if (MessageBox.Show(this, $"Please confirm the operation\n\n(new) {Model.Load2.OriginCity} → {Model.Load2.DestinationCity}\n(edit) {Model.Load2.DestinationCity} → {Model.Load1.DestinationCity}",
                        "Alert", MessageBoxButton.OKCancel, MessageBoxImage.Information) != MessageBoxResult.OK) {
                        return;
                    }
                    Model.Load1.EntityId = Model.InitialLoad.EntityId;
                    Model.Load2.EntityId = Guid.NewGuid();

                    Model.Load1.OriginLocation = Model.Load2.DestinationLocation;
                    Model.Load1.OriginAddress = Model.Load2.DestinationAddress;
                    Model.Load1.OriginCity = Model.Load2.DestinationCity;

                    Model.Load1.PickupDate = Model.Load2.DeliveryDate;
                    Model.Load1.PickupFrom = Model.Load2.DeliveryFrom;
                    Model.Load1.PickupTo = Model.Load2.DeliveryTo;

                    var loads = new List<LoadViewModel>() {
                        Model.Load2,
                        Model.Load1,
                    };
                    //base.BusyStart();
                    //_service.SaveLoads(loads, SaveLoads_Completed);
                    DialogResult = true;
                }
            }
        }

        private void Load2Length_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model.FullUnload == false) {
                Model.Load1.Length = Model.InitialLoad.Length - Model.Load2.Length;
            } else {
                Model.Load1.Length = Model.Load2.Length = Model.InitialLoad.Length;
            }
        }
        private void Load2Weight_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model.FullUnload == false) {
                Model.Load1.Weight = Model.InitialLoad.Weight - Model.Load2.Weight;
            } else {
                Model.Load1.Weight = Model.Load2.Weight = Model.InitialLoad.Weight;
            }
        }
        private void Load2Pallets_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            if (Model.FullUnload == false) {
                Model.Load1.Pallets = (short?)(Model.InitialLoad.Pallets - Model.Load2.Pallets);
            } else {
                Model.Load1.Pallets = Model.Load2.Pallets = Model.InitialLoad.Pallets;
            }
        }
        private void Load2LineHaul_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e) {
            Model.Load1.LineHaul = Model.InitialLoad.LineHaul - Model.Load2.LineHaul;
        }

        private void btnDestinationLocation_OnClick(object sender, RoutedEventArgs e) {
            var loadViewModel = ((LoadAddLocationViewModel)((FrameworkElement)sender).DataContext).Load2;
            var dlg = new EditLocationDialog();

            LocationViewModel location;
            if (loadViewModel.DestinationLocation != null) {
                location = (LocationViewModel)loadViewModel.DestinationLocation.Clone();
            } else {
                location = new LocationViewModel();
            }

            dlg.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                Location = location,
                Address = loadViewModel.DestinationAddress,
                City = loadViewModel.DestinationCity
            });
            dlg.Initialize();
            if (dlg.ShowDialog() == true) {
                loadViewModel.DestinationCity = dlg.Model.Location.City;
                loadViewModel.DestinationAddress = dlg.Model.Location.Address;
                loadViewModel.DestinationLocation = dlg.Model.Location.Location;
            }
        }

        private async void RadAutoCompleteBox_SearchAddressTextChanged(object sender, EventArgs e) {
            await RadAutoCompleteBox_SearchAddressTextChanged(sender, e, this.busy);
        }

        private async void RadAutoCompleteBox_SearchAddressSelectionChanged(object sender, SelectionChangedEventArgs e) {
            await RadAutoCompleteBox_SearchAddressSelectionChanged(sender, e, this.busy);
        }
    }
}
