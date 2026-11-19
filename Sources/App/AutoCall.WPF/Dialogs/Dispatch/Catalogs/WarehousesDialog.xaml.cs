using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class WarehousesDialog : BaseDispatchDialog {

        #region [ Base ]

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        #endregion [ Base ]

        #region [ _fields ]

        private DispatchService _service;
        private SettingsService _settings;
        private IMapper _mapper;
        private DispatchActions dispatchActions;
        private ActualCompanyViewModel _carrierCompany;

        #endregion [ _fields ]

        #region [ View Model ]

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public WarehousesViewModel Model {
            get { return DataContext as WarehousesViewModel; }
            set { DataContext = value; }
        }

        #endregion [ View Model ]

        #region [ .ctor ]

        public WarehousesDialog(DispatchService service, SettingsService settings, ActualCompanyViewModel carrierCompany) {
            _carrierCompany = carrierCompany;
            _service = service;
            _settings = settings;
            dispatchActions = new DispatchActions(null);
            Model = new WarehousesViewModel();
            _mapper = App.Services.GetRequiredService<IMapper>();

            InitializeComponent();
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        private void Initialize() {
            loadingWarehouses.Visibility = Visibility.Visible;
            Model.Warehouses?.Clear();
            _service.LoadWarehouses(LoadWarehouses_Completed);
        }

        #endregion [ Initialize ]

        #region [ Event Handlers ]

        private void WarehousesDialog_OnLoaded(object sender, RoutedEventArgs e) {
            Initialize();
            try {
                var data = _settings.GetEquipmentsDialogGridViewWarehouses();
                if (data != null) {
                    gridWarehouses.SetSettings(data);
                }
            } finally {
            }
        }

        private void WarehousesDialog_OnClosed(object sender, EventArgs e) {
            loadingWarehouses.Visibility = Visibility.Visible;
            _settings.SetEquipmentsDialogGridViewWarehouses(gridWarehouses.GetSettings());
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        #endregion [ Event Handlers ]

        #region [ Warehouses ]

        private async Task LoadWarehouses_Completed(ViewModelWrapper<List<WarehouseViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadWarehouses_Completed(wrapper));
                return;
            }
            loadingWarehouses.Visibility = Visibility.Collapsed;
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                foreach (WarehouseViewModel companyViewModel in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Warehouses.Add(companyViewModel);
                }
            }
        }

        private void CreateWarehouse_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditWarehouseDialog(_service, null);
            if (dialog.ShowDialog() == true) {
                Initialize();
            }
        }

        private void EditWarehouse() {
            if (gridWarehouses.SelectedItem != null) {
                var dialog = new EditWarehouseDialog(_service, (WarehouseViewModel)((WarehouseViewModel)gridWarehouses.SelectedItem).Clone());
                if (dialog.ShowDialog() == true) {
                    Initialize();
                }
            }
        }

        private void EditWarehouse_Clicked(object sender, RoutedEventArgs e) {
            EditWarehouse();
        }

        private void gridWarehouses_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditWarehouse();
        }

        private void DeleteWarehouse_Clicked(object sender, RoutedEventArgs e) {
            if (gridWarehouses.SelectedItem != null) {
                if (MessageBox.Show("Are you sure you want to delete selected Warehouse?", "Delete Warehouse",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                _service.DeleteWarehouse(((WarehouseViewModel)gridWarehouses.SelectedItem).EntityId, DeleteWarehouseCompleted);
            }
        }

        private async Task DeleteWarehouseCompleted(ViewModelWrapper<WarehouseViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await DeleteWarehouseCompleted(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Initialize();
            }
        }

        #endregion [ Warehouses ]
    }
}