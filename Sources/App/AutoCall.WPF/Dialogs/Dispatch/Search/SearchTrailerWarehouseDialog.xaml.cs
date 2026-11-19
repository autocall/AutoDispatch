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
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Properties;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public class SearchTrailerWarehouseViewModel : ViewModel {
        private string _searchTrailerText;
        private string _searchWarehouseText;

        private TrailerViewModel _selectedTrailerWrapper;
        private WarehouseViewModel _selectedWarehouseWrapper;
        private bool _hasWarehouse;

        public bool HasWarehouse {
            get => _hasWarehouse;
            set {
                _hasWarehouse = value;
                OnPropertyChanged();
            }
        }

        public List<TrailerViewModel> Trailers { get; set; }
        public List<WarehouseViewModel> Warehouses { get; set; }

        public string SearchTrailerText {
            get { return _searchTrailerText; }
            set {
                if (_searchTrailerText != value) {
                    _searchTrailerText = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SearchWarehouseText {
            get { return _searchWarehouseText; }
            set {
                if (_searchWarehouseText != value) {
                    _searchWarehouseText = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerViewModel SelectedTrailerWrapper {
            get { return _selectedTrailerWrapper; }
            set {
                if (_selectedTrailerWrapper != value) {
                    _selectedTrailerWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public WarehouseViewModel SelectedWarehouseWrapper {
            get { return _selectedWarehouseWrapper; }
            set {
                if (_selectedWarehouseWrapper != value) {
                    _selectedWarehouseWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public SearchTrailerWarehouseViewModel() {
            Trailers = new List<TrailerViewModel>();
            Warehouses = new List<WarehouseViewModel>();
        }
    }

    public partial class SearchTrailerWarehouseDialog : Window {

        #region [ Protected Fields ]

        /// <summary>
        ///     Flag = true if initialize of trailers was completed </summary>
        protected bool _trailersInitCompleted;

        protected bool _warehousesInitCompleted;

        #endregion [ Protected Fields ]

        private DispatchService _service;
        private MainManager _manager;
        private SettingsService _settings;

        public SearchTrailerWarehouseViewModel Model {
            get { return DataContext as SearchTrailerWarehouseViewModel; }
            set { DataContext = value; }
        }

        private Guid? pendingWarehouseId;

        public SearchTrailerWarehouseDialog(Guid? warehouseId, DispatchService service, MainManager manager, SettingsService settings) {
            _service = service;
            _manager = manager;
            _settings = settings;
            pendingWarehouseId = warehouseId;

            _service.LoadTrailers(LoadTrailers_Completed);
            _service.LoadWarehouses(LoadWarehouses_Completed);

            InitializeComponent();

            Model = new SearchTrailerWarehouseViewModel() { 
                HasWarehouse = settings.GetSearchTrailerWarehouseMode()
            };
            HasWarehouse_Clicked(this, default);
            gridTrailers.ItemsSource = ((CollectionViewSource)Resources["TrailerWrappersViewSource"]).View;
            gridWarehouses.ItemsSource = ((CollectionViewSource)Resources["WarehouseWrappersViewSource"]).View;
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

        private async Task LoadWarehouses_Completed(ViewModelWrapper<List<WarehouseViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadWarehouses_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                _warehousesInitCompleted = true;
                Model.Warehouses.AddRange(wrapper.Data);
                CheckByLoaded();
            } else {
                MessageBoxEx.Error(this, wrapper.MessageError);
            }
        }

        private void CheckByLoaded() {
            if (_trailersInitCompleted) {
                ((ICollectionView)gridTrailers.ItemsSource).Refresh();
            }
            if (_warehousesInitCompleted) {
                ((ICollectionView)gridWarehouses.ItemsSource).Refresh();
            }
            if (_trailersInitCompleted && _warehousesInitCompleted) {
                loading.Visibility = Visibility.Collapsed;

                if (this.pendingWarehouseId.HasValue) {
                    Model.SelectedWarehouseWrapper = Model.Warehouses.Find(x => x.EntityId == this.pendingWarehouseId);
                    Model.HasWarehouse = true;
                    gridWarehouses.Focus();
                    HasWarehouse_Clicked(this, default);
                }
            }
        }

        private void TrailerWrappersViewSource_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (TrailerViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchTrailerText) || wrapper.Filter(Model.SearchTrailerText);
        }

        private void WarehouseWrappersViewSource_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (WarehouseViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchWarehouseText) || wrapper.Filter(Model.SearchWarehouseText);
        }

        private void tbTrailerFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridTrailers.ItemsSource).Refresh();
        }

        private void tbWarehouseFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridWarehouses.ItemsSource).Refresh();
        }

        private void gridTrailers_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Model.HasWarehouse == true) return;
            if (Model.SelectedTrailerWrapper != null) {
                btnOk_Clicked(null, null);
            }
        }

        private void gridWarehouses_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Model.HasWarehouse == true) return;
            if (Model.SelectedWarehouseWrapper != null) {
                btnOk_Clicked(null, null);
            }
        }

        private void CreateTrailer_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditTrailerDialog(_service, null);
            if (dialog.ShowDialog() == true) {
                gridTrailers.Focus();
                Model.Trailers.Insert(0, dialog.Model);
                ((ICollectionView)this.gridTrailers.ItemsSource).Refresh();
                Model.SelectedTrailerWrapper = dialog.Model;
            }
        }

        private void CreateWarehouse_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditWarehouseDialog(_service, null);
            if (dialog.ShowDialog() == true) {
                gridWarehouses.Focus();
                Model.Warehouses.Insert(0, dialog.Model);
                ((ICollectionView)this.gridWarehouses.ItemsSource).Refresh();
                Model.SelectedWarehouseWrapper = dialog.Model;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
            _settings.SetSearchTrailerWarehouseMode(Model.HasWarehouse);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void HasWarehouse_Clicked(object sender, RoutedEventArgs e) {
            this.secondGridColumn.Width = Model.HasWarehouse ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            this.titleTextBox.Text = Model.HasWarehouse ? "Search Trailer/Warehouse" : "Search Trailer";
            if (Model.HasWarehouse == false) {
                Model.SelectedWarehouseWrapper = null;
            }
        }
    }
}
