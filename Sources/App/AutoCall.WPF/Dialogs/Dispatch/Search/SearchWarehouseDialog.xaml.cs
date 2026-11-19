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
    public partial class SearchWarehouseDialog : Window {

        #region [ Protected Fields ]

        /// <summary>
        ///     Flag = true if initialize of warehouses was completed </summary>
        protected bool _warehousesInitCompleted;

        #endregion [ Protected Fields ]

        private DispatchService _service;
        private MainManager _manager;

        public SearchWarehouseViewModel Model {
            get { return DataContext as SearchWarehouseViewModel; }
            set { DataContext = value; }
        }

        public SearchWarehouseDialog(DispatchService service, MainManager manager, bool clearable = false) {
            _service = service;
            _manager = manager;
            _service.LoadWarehouses(LoadWarehouses_Completed);

            InitializeComponent();

            Model = new SearchWarehouseViewModel() { Clearable = clearable };
            gridWarehouses.ItemsSource = ((CollectionViewSource)Resources["WrappersViewSource"]).View;
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
            if (_warehousesInitCompleted) {
                loading.Visibility = Visibility.Collapsed;
                ((ICollectionView)gridWarehouses.ItemsSource).Refresh();
            }
        }

        private void WrappersViewSource_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (WarehouseViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchText) ||
                (wrapper.Filter(Model.SearchText));
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridWarehouses.ItemsSource).Refresh();
        }

        private void grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Model.SelectedWrapper != null) {
                btnOk_Clicked(null, null);
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void btnClear_Clicked(object sender, RoutedEventArgs e) {
            Model.SelectedWrapper = null;
            DialogResult = true;
        }
    }
}
