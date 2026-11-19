using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.Dialogs.Dispatch {
    /// <summary>
    /// Interaction logic for SearchDatCompanyDialog.xaml
    /// </summary>
    public partial class SearchDatCompanyDialog : Window {

        private DispatchService _service;
        private MainManager _manager;
        private SettingsService _settings;

        public DatCompaniesViewModel Model {
            get { return DataContext as DatCompaniesViewModel; }
            set { DataContext = value; }
        }

        public SearchDatCompanyDialog(DispatchService service, MainManager manager, SettingsService settings, ActualCompanyViewModel company) {
            _service = service;
            _manager = manager;
            _settings = settings;

            InitializeComponent();

            Model = new DatCompaniesViewModel() { ActualCompany = company };
            // Assigns Items Source
            gridCompanies.ItemsSource = ((CollectionViewSource)Resources["DatCompaniesViewSource"]).View;
        }

        private void SearchDatCompaniesDialog_OnLoaded(object sender, RoutedEventArgs e) {
            try {
                Initialize();
                var data = _settings.GetDatCompaniesDialogGridView();
                if (data != null) {
                    gridCompanies.SetSettings(data);
                }
            } catch (Exception ex) {
                _webl.e("SearchDatCompaniesDialog_OnLoaded", ex);
                gridCompanies.Columns.Clear();
                MessageBox.Show("an error occurred, reopen the window");
                _settings.SetDatCompaniesDialogGridView(null);
                Close();
            }
        }

        private void Initialize() {
            Model.DatCompanies.AddRange(DispatchBoardViewModel.Instance.DatCompanies.OrderByDescending(c => c.CreatedDate));
            ((ICollectionView)gridCompanies.ItemsSource).Refresh();
            loading.Visibility = Visibility.Collapsed;
        }

        private void SearchDatCompaniesDialog_OnClosed(object sender, EventArgs e) {
            _settings.SetDatCompaniesDialogGridView(gridCompanies.GetSettings());
        }

        private void DatCompaniesViewSource_OnFilter(object sender, FilterEventArgs e) {
            var company = (DatCompanyViewModel)e.Item;
            e.Accepted =
               (Model.ActualCompany == null || company.ActualCompanyId == null ||
               Model.ActualCompany.EntityId == company.ActualCompanyId) &&
               (String.IsNullOrEmpty(Model.SearchText) || company.Filter(Model.SearchText));
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridCompanies.ItemsSource).Refresh();
        }

        private async void Phone_OnClick(object sender, RoutedEventArgs e) {
            var callService = _manager.GetCallService;
            var phone = StringExtension.OnlyNumbers(((GridViewCell)sender).Value as string);
            await callService?.StartCallAsync(callService.ToPhoneNumber(phone));
        }

        private void grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (CheckDoNotUse()) {
                MessageBoxEx.Error("Do Not Use");
                return;
            }
            if (Model.SelectedDatCompany != null) {
                DialogResult = true;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (CheckDoNotUse()) {
                MessageBoxEx.Error("Do Not Use");
                return;
            }
            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private bool CheckDoNotUse() {
            return Model.SelectedDatCompany != null && Model.SelectedDatCompany.DoNotUse;
        }
    }
}
