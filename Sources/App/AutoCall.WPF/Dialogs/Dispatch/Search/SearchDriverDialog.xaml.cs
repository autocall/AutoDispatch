using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Logic.Managers;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class SearchDriverDialog : Window {

        #region [ Protected Fields ]

        /// <summary>
        ///     Flag = true if initialize of drivers was completed </summary>
        protected bool _driversInitCompleted;
        /// <summary>
        ///     Flag = true if initialize of trucks was completed </summary>
        protected bool _trucksInitCompleted;
        /// <summary>
        ///     Flag = true if initialize of trailers was completed </summary>
        protected bool _trailersInitCompleted;

        #endregion [ Protected Fields ]

        private DispatchService _service;
        private MainManager _manager;

        public SearchDriverViewModel Model {
            get { return DataContext as SearchDriverViewModel; }
            set { DataContext = value; }
        }

        public SearchDriverDialog(DispatchService service, MainManager manager) {
            _service = service;
            _manager = manager;
            _service.LoadDrivers(LoadDrivers_Completed);
            _service.LoadTrucks(LoadTrucks_Completed);
            _service.LoadTrailers(LoadTrailers_Completed);

            InitializeComponent();

            Model = new SearchDriverViewModel();
            gridDrivers.ItemsSource = ((CollectionViewSource)Resources["WrappersViewSource"]).View;
        }

        private async Task LoadDrivers_Completed(ViewModelWrapper<List<DriverViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadDrivers_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                _driversInitCompleted = true;
                Model.Drivers.AddRange(wrapper.Data);
                CheckByLoaded();
            } else {
                MessageBoxEx.Error(this, wrapper.MessageError);
            }
        }

        private async Task LoadTrucks_Completed(ViewModelWrapper<List<TruckViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadTrucks_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                _trucksInitCompleted = true;
                Model.Trucks.AddRange(wrapper.Data);
                CheckByLoaded();
            } else {
                MessageBoxEx.Error(this, wrapper.MessageError);
            }
        }

        private async Task LoadTrailers_Completed(ViewModelWrapper<List<TrailerViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadTrailers_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                _trailersInitCompleted = true;
                Model.Trailers.AddRange(wrapper.Data);
                CheckByLoaded();
            } else {
                MessageBoxEx.Error(this, wrapper.MessageError);
            }
        }

        private void CheckByLoaded() {
            if (_driversInitCompleted && _trucksInitCompleted && _trailersInitCompleted) {
                loading.Visibility = Visibility.Collapsed;
                Model.BuildWrappers();
                ((ICollectionView)gridDrivers.ItemsSource).Refresh();
            }
        }

        private void WrappersViewSource_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (Wrapper3DriverViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchText) ||
                (wrapper.Driver != null && wrapper.Driver.Filter(Model.SearchText)) ||
                (wrapper.Truck != null && wrapper.Truck.Filter(Model.SearchText)) ||
                (wrapper.Trailer != null && wrapper.Trailer.Filter(Model.SearchText));
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridDrivers.ItemsSource).Refresh();
        }

        private void grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Model.SelectedWrapper != null) {
                btnOk_Clicked(null, null);
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.SelectedWrapper.Truck == null) {
                MessageBoxEx.Alert(this, "Current driver doesn't have a truck");
                return;
            }
            if (Model.SelectedWrapper.Trailer == null) {
                MessageBoxEx.Alert(this, "Current truck doesn't have a trailer");
                return;
            }
            DialogResult = true;
        }



        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
