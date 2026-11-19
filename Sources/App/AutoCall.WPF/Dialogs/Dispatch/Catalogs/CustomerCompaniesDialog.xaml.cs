using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class CustomerCompaniesDialog : BaseDispatchDialog {
        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private DispatchService _service;
        private SettingsService _settings;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public CustomerCompaniesViewModel Model {
            get { return DataContext as CustomerCompaniesViewModel; }
            set { DataContext = value; }
        }

        private ActualCompanyViewModel ActualCompany { get; set; }

        /// <summary>
        ///     Constructor </summary>
        public CustomerCompaniesDialog(DispatchService service, SettingsService settings, ActualCompanyViewModel company) {
            ActualCompany = company;
            _service = service;
            _settings = settings;

            InitializeComponent();

            Model = new CustomerCompaniesViewModel();
            // Assigns Items Source
            gridCompanies.ItemsSource = ((CollectionViewSource)Resources["CustomerCompaniesViewSource"]).View;
        }

        private void Initialize() {
            _service.LoadCustomerCompanies(LoadCustomerCompanies_Completed);
            base.BusyStart();
        }

        private void CustomerCompaniesDialog_OnLoaded(object sender, RoutedEventArgs e) {
            Initialize();
            // NOTE: loads settings
            //try {
            //    var data = _settings.GetCustomerCompaniesDialogGridView();
            //    if (data != null) {
            //        gridCompanies.SetSettings(data);
            //    }
            //} finally {
            //}
        }

        private void CustomerCompaniesDialog_OnClosed(object sender, EventArgs e) {
            // NOTE: saves settings
            //_settings.SetCustomerCompaniesDialogGridView(gridCompanies.GetSettings());
        }

        private async Task LoadCustomerCompanies_Completed(ViewModelWrapper<List<CustomerCompanyViewModel>> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                await this.Dispatcher.InvokeAsync((Action)(async () => await LoadCustomerCompanies_Completed(wrapper)));
                return;
            }
            await base.SaveCompleted(wrapper, false);
            loading.Visibility = Visibility.Collapsed;
            if (wrapper.Status == ResponseStatus.Success) {
                Model.CustomerCompanies.Clear();
                Model.CustomerCompanies.AddRange(wrapper.Data);
                ((ICollectionView) gridCompanies.ItemsSource).Refresh();
            }
        }
        
        private void CustomerCompaniesViewSource_OnFilter(object sender, FilterEventArgs e) {
            var company = (CustomerCompanyViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchText) || company.Filter(Model.SearchText);
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridCompanies.ItemsSource).Refresh();
        }

        private void Create_Clicked(object sender, RoutedEventArgs e) {
        //    var dialog = new EditCustomerCompanyDialog(_service, null, this.ActualCompany);
        //    if (dialog.ShowDialog() == true) {
        //        Initialize();
        //    }
        }

        //private void EditCustomerCompany() {
        //    if (gridCompanies.SelectedItem != null) {
        //        var dialog = new EditCustomerCompanyDialog(_service, (CustomerCompanyViewModel)((CustomerCompanyViewModel)gridCompanies.SelectedItem).Clone(), this.ActualCompany);
        //        if (dialog.ShowDialog() == true) {
        //            Initialize();
        //        }
        //    }
        //}

        private void Edit_Clicked(object sender, RoutedEventArgs e) {
            //EditCustomerCompany();
        }

        private void gridCompanies_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            //EditCustomerCompany();
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e) {
        //    if (gridCompanies.SelectedItem != null) {
        //        if (MessageBox.Show("Are you sure you want to delete selected Company?", "Delete Company",
        //            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
        //            return;
        //        base.BusyStart();
        //        _service.DeleteCustomerCompany(((CustomerCompanyViewModel)gridCompanies.SelectedItem).EntityId, DeleteDatCompleted);
        //    }
        }

        //private void DeleteDatCompleted(ViewModelWrapper<CustomerCompanyViewModel> wrapper) {
        //    if (Dispatcher.CheckAccess() == false) {
        //        Dispatcher.Invoke(() => DeleteDatCompleted(wrapper));
        //        return;
        //    }
        //    await base.SaveCompleted(wrapper, false);
        //    if (wrapper.Status == ResponseStatus.Success) {
        //        Initialize();
        //    }
        //}

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void Phone_OnClick(object sender, RoutedEventArgs e) {
            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            var phone = StringExtension.OnlyNumbers((((GridViewCell)sender).Value as string));
            await callService?.StartCallAsync(callService.ToPhoneNumber(phone));
        }
    }
}