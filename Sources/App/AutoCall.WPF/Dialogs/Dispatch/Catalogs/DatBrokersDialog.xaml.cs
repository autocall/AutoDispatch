using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using AutoCall.Common.Extensions;
using Microsoft.Win32;
using AutoDispatch.Logic.Helpers;
using System.Threading.Tasks;
using System.Collections;
using AutoCall.Common.Collections;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;
using AutoDispatch.Data;
using System.Diagnostics;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class DatBrokersDialog : BaseDispatchDialog {
        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private DispatchService _service;
        private SettingsService _settings;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DatCompaniesViewModel Model {
            get { return DataContext as DatCompaniesViewModel; }
            set { DataContext = value; }
        }

        private ActualCompanyViewModel ActualCompany { get; set; }

        /// <summary>
        ///     Constructor </summary>
        public DatBrokersDialog(DispatchService service, SettingsService settings, ActualCompanyViewModel company, List<DatCompanyViewModel> datCompanies) {
            ActualCompany = company;
            _service = service;
            _settings = settings;

            InitializeComponent();

            // uses existing companies
            Model = new DatCompaniesViewModel() { ActualCompany = company, DatCompanies = datCompanies };
            // Assigns Items Source
            gridCompanies.ItemsSource = (ListCollectionView)((CollectionViewSource)Resources["DatCompaniesViewSource"]).View;
        }

        private void DatCompaniesDialog_OnLoaded(object sender, RoutedEventArgs args) {
            try {
                Initialize();
                //var autoWidthColumns = gridCompanies.Columns.Cast<GridViewColumn>().Where(e => e.Width.IsAuto).ToList();
                //var data = _settings.GetDatCompaniesDialogGridView();
                //if (data != null) {
                //    gridCompanies.SetSettings(data);
                //}
                //autoWidthColumns.ForEach(e => e.Width = GridViewLength.Auto);
            } finally {
            }
        }

        private void Initialize() {
            ((ICollectionView)gridCompanies.ItemsSource).Refresh();
            loading.Visibility = Visibility.Collapsed;

            if (Model.DatCompanies.Any(e => e.IsNew)) {
                gridCompanies.ScrollIntoView(Model.DatCompanies.First(e => e.IsNew));
            }
        }

        private void DatCompaniesDialog_OnClosed(object sender, EventArgs e) {
            //_settings.SetDatCompaniesDialogGridView(gridCompanies.GetSettings());
        }

        private void DatCompaniesViewSource_OnFilter(object sender, FilterEventArgs e) {
            var company = (DatCompanyViewModel)e.Item;
            e.Accepted =
                (Model.ActualCompany == null || 
                 company.ActualCompanyId == null || 
                 Model.ActualCompany.EntityId == company.ActualCompanyId) &&
                (DispatchBoardViewModel.Instance.GetActualCompanyById(company.ActualCompanyId) == null ||
                 DispatchBoardViewModel.Instance.GetActualCompanyById(company.ActualCompanyId).BoardType == DispatchBoardType.Carrier) &&
                (String.IsNullOrEmpty(Model.SearchText) || company.Filter(Model.SearchText));
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridCompanies.ItemsSource).Refresh();
        }

        private void Create_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditBrokerDialog(_service, null, this.ActualCompany, DispatchBoardViewModel.Instance.ActualCompanies);
            if (dialog.ShowDialog() == true) {
                Model.DatCompanies.Add(dialog.Model);
                Initialize();
            }
        }

        private void EditDatCompany() {
            if (gridCompanies.SelectedItem != null) {
                var dialog = new EditBrokerDialog(_service,
                    (DatCompanyViewModel)((DatCompanyViewModel)gridCompanies.SelectedItem).Clone(),
                    this.ActualCompany, DispatchBoardViewModel.Instance.ActualCompanies);
                if (dialog.ShowDialog() == true) {
                    ((DatCompanyViewModel)gridCompanies.SelectedItem).Assign(dialog.Model);
                    Initialize();
                }
            }
        }

        private void Edit_Clicked(object sender, RoutedEventArgs e) {
            EditDatCompany();
        }

        private async void Export_Clicked(object sender, RoutedEventArgs args) {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "xlsx";
            dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            dialog.FileName = "Brokers.xlsx";
            if (dialog.ShowDialog() == true) {
                var columns = gridCompanies.Columns
                        .Cast<GridViewBoundColumnBase>()
                        .Select(e => new ExportExcelColumn() {
                            UniqueName = e.UniqueName,
                            BindingPath = e.DataMemberBinding.Path.Path,
                            StringFormat = e.DataMemberBinding.StringFormat,
                            Width = e.Width.DisplayValue,
                            Group = e.ColumnGroupName,
                            Header = e.Header.ToString()
                        })
                        .ToList();

                BusyStart("Saving...");
                await Task.Delay(TimeSpan.FromSeconds(0.2));

                ExportHelper.CreateFile(
                    dialog.FileName,
                    ((IEnumerable)gridCompanies.ItemsSource).Cast<DatCompanyViewModel>().ToList(),
                    columns);

                BusyFinish();
            }
        }

        private void gridCompanies_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditDatCompany();
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e) {
            if (gridCompanies.SelectedItem != null) {
                if (MessageBox.Show("Are you sure you want to delete selected Company?", "Delete Company",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                base.BusyStart();
                _service.DeleteDatCompany(((DatCompanyViewModel)gridCompanies.SelectedItem).EntityId, DeleteDatCompleted);
            }
        }

        private async Task  DeleteDatCompleted(ViewModelWrapper<DatCompanyViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await DeleteDatCompleted(wrapper));
                return;
            }
            await base.SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                // This dialog has Companies.ToList()
                var oldCompany = Model.DatCompanies.First(e => e.EntityId == wrapper.Data.EntityId);
                Model.DatCompanies.Remove(oldCompany);
                Initialize();
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private async void Phone_OnClick(object sender, RoutedEventArgs e) {
            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            var phone = StringExtension.OnlyNumbers((((GridViewCell)sender).Value as string));
            await callService?.StartCallAsync(callService.ToPhoneNumber(phone));
        }

        private void Link_Click(object sender, RoutedEventArgs e) {
            // V1 входил через brokersnapshot.com/ExternalLogin с токеном AutoCall; токен AutoDispatch Safer не проверяет (аудит B12) —
            // публичная страница поиска, как AutoCall V3 (RecordDetailsBuilders.McUrl)
            var url = ((DatCompanyViewModel)((FrameworkContentElement)sender).DataContext).CarrierViewLink;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}