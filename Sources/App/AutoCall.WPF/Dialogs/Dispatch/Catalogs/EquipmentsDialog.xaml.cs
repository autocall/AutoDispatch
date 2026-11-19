using System;
using AutoDispatch.Logic.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using AutoDispatch.Data;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EquipmentsDialog : BaseDispatchDialog {

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
        public EquipmentsViewModel Model {
            get { return DataContext as EquipmentsViewModel; }
            set { DataContext = value; }
        }

        #endregion [ View Model ]

        #region [ .ctor ]

        public EquipmentsDialog(DispatchService service, SettingsService settings, ActualCompanyViewModel carrierCompany) {
            _carrierCompany = carrierCompany;
            _service = service;
            _settings = settings;
            dispatchActions = new DispatchActions(null);
            Model = new EquipmentsViewModel();
            _mapper = App.Services.GetRequiredService<IMapper>();

            InitializeComponent();

            gridTrucks.ItemsSource = ((CollectionViewSource)Resources["WrappersViewSourceTruck"]).View;
            gridTrailers.ItemsSource = ((CollectionViewSource)Resources["WrappersViewSourceTrailer"]).View;
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        private async void Initialize() {
            loadingTrucks.Visibility = Visibility.Visible;
            loadingTrailers.Visibility = Visibility.Visible;
            Model.Trucks?.Clear();
            Model.Trailers?.Clear();
            _service.LoadTrailers(LoadTrailers_Completed);
            _service.LoadTrucks(LoadTrucks_Completed);
            _service.LoadYards(LoadYards_Completed);
            var service = App.Services.GetRequiredService<ActualCompanyDataService>();

            var result = await service.SearchActualCompaniesAsync(new SearchActualCompaniesRequest());
            foreach (ActualCompanyDto company in result.ActualCompanies) {
                if (String.IsNullOrEmpty(company.BnsfLastMessage) == false) {
                    tbError.Text = company.BnsfLastMessage;
                    tbError.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion [ Initialize ]

        #region [ Event Handlers ]

        private void EquipmentsDialog_OnLoaded(object sender, RoutedEventArgs e) {
            Initialize();
            try {
                var data = _settings.GetEquipmentsDialogGridViewTrucks();
                if (data != null) {
                    gridTrucks.SetSettings(data);
                }
            } finally {
            }
            try {
                var data = _settings.GetEquipmentsDialogGridViewTrailers();
                if (data != null) {
                    gridTrailers.SetSettings(data);
                }
            } finally {
            }

            var session = UserSession.Current;
            if (session.HasBnsfRailway == false) {
                bnsfButton.Visibility = Visibility.Collapsed;
                BnsfOwnerColumn.IsVisible = false;
                BnsfShipperColumn.IsVisible = false;
                BnsfUnitColumn.IsVisible = false;
                BnsfLoadEmptyStatusColumn.IsVisible = false;
                BnsfCurrentLocationColumn.IsVisible = false;
                BnsfEventCodeColumn.IsVisible = false;
                BnsfEventDateColumn.IsVisible = false;
                BnsfETA_DerampColumn.IsVisible = false;
                BnsfOriginColumn.IsVisible = false;
                BnsfDestinationColumn.IsVisible = false;
                BnsfBeyondGoalHoursColumn.IsVisible = false;
                BnsfTimeDerampColumn.IsVisible = false;
                BnsfTimeNotifyColumn.IsVisible = false;
                BnsfFinalTrainArrivalColumn.IsVisible = false;
                BnsfGoalDateColumn.IsVisible = false;
                BnsfNexteventColumn.IsVisible = false;
                BnsfPickUpNumberColumn.IsVisible = false;
                BnsfStorageDayColumn.IsVisible = false;
                BnsfTrainColumn.IsVisible = false;
                BnsfTruckerColumn.IsVisible = false;
                BnsfWaybillDateColumn.IsVisible = false;
                BnsfRateAuthorityColumn.IsVisible = false;
                BnsfAmountDueColumn.IsVisible = false;
                BnsfDueDateColumn.IsVisible = false;
                BnsfWaybillNumberColumn.IsVisible = false;
                BnsfBillNumberColumn.IsVisible = false;
                BnsfAdjustmentsColumn.IsVisible = false;
            } else {
                bnsfButton.Visibility = Visibility.Visible;
                BnsfOwnerColumn.IsVisible = true;
                BnsfShipperColumn.IsVisible = true;
                BnsfUnitColumn.IsVisible = true;
                BnsfLoadEmptyStatusColumn.IsVisible = true;
                BnsfCurrentLocationColumn.IsVisible = true;
                BnsfEventCodeColumn.IsVisible = true;
                BnsfEventDateColumn.IsVisible = true;
                BnsfETA_DerampColumn.IsVisible = true;
                BnsfOriginColumn.IsVisible = true;
                BnsfDestinationColumn.IsVisible = true;
                BnsfBeyondGoalHoursColumn.IsVisible = true;
                BnsfTimeDerampColumn.IsVisible = true;
                BnsfTimeNotifyColumn.IsVisible = true;
                BnsfFinalTrainArrivalColumn.IsVisible = true;
                BnsfGoalDateColumn.IsVisible = true;
                BnsfNexteventColumn.IsVisible = true;
                BnsfPickUpNumberColumn.IsVisible = true;
                BnsfStorageDayColumn.IsVisible = true;
                BnsfTrainColumn.IsVisible = true;
                BnsfTruckerColumn.IsVisible = true;
                BnsfWaybillDateColumn.IsVisible = true;
                BnsfRateAuthorityColumn.IsVisible = true;
                BnsfAmountDueColumn.IsVisible = true;
                BnsfDueDateColumn.IsVisible = true;
                BnsfWaybillNumberColumn.IsVisible = true;
                BnsfBillNumberColumn.IsVisible = true;
                BnsfAdjustmentsColumn.IsVisible = true;
            }
        }

        private void EquipmentsDialog_OnClosed(object sender, EventArgs e) {
            loadingTrucks.Visibility = Visibility.Visible;
            loadingTrailers.Visibility = Visibility.Visible;
            _settings.SetEquipmentsDialogGridViewTrucks(gridTrucks.GetSettings());
            _settings.SetEquipmentsDialogGridViewTrailers(gridTrailers.GetSettings());
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Open_TruckAnnualInspection_OnClick(object sender, RoutedEventArgs e) {
            var truck = (TruckViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = truck.EntityId,
                Name = truck.AnnualInspection_File,
            });
        }

        private void Open_TruckRegistration_OnClick(object sender, RoutedEventArgs e) {
            var truck = (TruckViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = truck.EntityId,
                Name = truck.Registration_File,
            });
        }

        private void Open_TrailerAnnualInspection_OnClick(object sender, RoutedEventArgs e) {
            var trailer = (TrailerViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = trailer.EntityId,
                Name = trailer.AnnualInspection_File,
            });
        }

        private void Open_TrailerRegistration_OnClick(object sender, RoutedEventArgs e) {
            var trailer = (TrailerViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = trailer.EntityId,
                Name = trailer.Registration_File,
            });
        }

        private void Open_TrailerMaintenance_OnClick(object sender, RoutedEventArgs e) {
            var trailer = (TrailerViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = trailer.EntityId,
                Name = trailer.Maintenance_File,
            });
        }

        private void Open_TrailerScheme_OnClick(object sender, RoutedEventArgs e) {
            var trailer = (TrailerViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = trailer.EntityId,
                Name = trailer.SchemeFile,
            });
        }

        #endregion [ Event Handlers ]

        #region [ Trucks ]

        private async Task LoadTrucks_Completed(ViewModelWrapper<List<TruckViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadTrucks_Completed(wrapper));
                return;
            }
            loadingTrucks.Visibility = Visibility.Collapsed;
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                foreach (TruckViewModel truck in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Trucks.Add(truck);
                }
            }
        }

        private void CreateTruck_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditTruckDialog(_service, null, Model.Trailers);
            if (dialog.ShowDialog() == true) {
                Initialize();
            }
        }

        private void EditTruck() {
            if (gridTrucks.SelectedItem != null) {
                var clone = (TruckViewModel)((TruckViewModel)gridTrucks.SelectedItem).Clone();
                var dialog = new EditTruckDialog(_service, clone,
                    new ObservableCollection<TrailerViewModel>(Model.Trailers.Where(trailer =>
                        trailer.EntityId == clone.TrailerId || Model.Trucks.Any(truck => truck.TrailerId == trailer.EntityId) == false)));
                if (dialog.ShowDialog() == true || dialog.Reload) {
                    Initialize();
                }
            }
        }

        private void EditTruck_Clicked(object sender, RoutedEventArgs e) {
            EditTruck();
        }

        private void gridTrucks_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditTruck();
        }

        private void DeleteTruck_Clicked(object sender, RoutedEventArgs e) {
            if (gridTrucks.SelectedItem != null) {
                if (MessageBox.Show("Are you sure you want to delete selected truck?", "Delete truck",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                _service.DeleteTruck(((TruckViewModel)gridTrucks.SelectedItem).EntityId, DeleteTruckCompleted);
            }
        }

        private async Task DeleteTruckCompleted(ViewModelWrapper<TruckViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await DeleteTruckCompleted(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Initialize();
            }
        }

        private void WrappersViewSourceTruck_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (TruckViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchTextTruck) ||
                (wrapper.Filter(Model.SearchTextTruck));
        }

        private void tbFilterTruck_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridTrucks.ItemsSource).Refresh();
        }

        #endregion [ Trucks ]

        #region [ Trailers ]

        private async Task LoadTrailers_Completed(ViewModelWrapper<List<TrailerViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadTrailers_Completed(wrapper));
                return;
            }
            loadingTrailers.Visibility = Visibility.Collapsed;
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                foreach (TrailerViewModel trailer in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Trailers.Add(trailer);
                }
            }
        }

        private void CreateTrailer_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditTrailerDialog(_service, null);
            if (dialog.ShowDialog() == true) {
                Initialize();
            }
        }

        private void EditTrailer() {
            if (gridTrailers.SelectedItem != null) {
                var dialog = new EditTrailerDialog(_service, (TrailerViewModel)((TrailerViewModel)gridTrailers.SelectedItem).Clone());
                if (dialog.ShowDialog() == true || dialog.Reload) {
                    Initialize();
                }
            }
        }

        private void EditTrailer_Clicked(object sender, RoutedEventArgs e) {
            EditTrailer();
        }

        private void gridTrailers_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditTrailer();
        }

        private void DeleteTrailer_Clicked(object sender, RoutedEventArgs e) {
            if (gridTrailers.SelectedItem != null) {
                if (MessageBox.Show("Are you sure you want to delete selected Trailer?", "Delete Trailer",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                _service.DeleteTrailer(((TrailerViewModel)gridTrailers.SelectedItem).EntityId, SaveTrailerCompleted);
            }
        }

        private void MoveToHistoryTrailer_Clicked(object sender, RoutedEventArgs e) {
            if (gridTrailers.SelectedItem != null) {
                if (MessageBox.Show("Are you sure you want to move to history selected Trailer?", "Move to History",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var trailer = (TrailerViewModel)gridTrailers.SelectedItem;
                trailer.History = true;
                _service.SaveTrailer(trailer, SaveTrailerCompleted);
            }
        }

        private async Task SaveTrailerCompleted(ViewModelWrapper<TrailerViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveTrailerCompleted(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Initialize();
            }
        }

        private void BnsfRefresh_Clicked(object sender, RoutedEventArgs e) {
            _service.BnsfRefresh(this._carrierCompany.EntityId, BnsfRefresh_Completed);
            this.BusyStart("BNSF Loading data...");
            loadingTrailers.Visibility = Visibility.Visible;
        }

        private async Task BnsfRefresh_Completed(ViewModelWrapper<BnsfRefreshResultDto> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await BnsfRefresh_Completed(wrapper));
                return;
            }
            this.BusyFinish();
            loadingTrailers.Visibility = Visibility.Collapsed;

            if (wrapper.Status == ResponseStatus.Success) {
                foreach (TrailerDto dto in wrapper.Data.Trailers) {
                    TrailerViewModel trailer = this.Model.Trailers.FirstOrDefault(e => e.EntityId == dto.EntityId);
                    if (trailer != null) {
                        trailer.Assign(TrailerConverter.ToVm(dto));
                    }
                }
                if (String.IsNullOrEmpty(wrapper.Data.Message) == false) {
                    tbError.Visibility = Visibility.Visible;
                    tbError.Text = wrapper.Data.Message.Trim();
                }
            } else {
                tbError.Visibility = Visibility.Visible;
                tbError.Text = wrapper.MessageError;
            }
        }

        private void ExportTrailers_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "xls";
            dialog.Filter = "Excel files (*.xls)|*.xls";
            dialog.FileName = "trailers.xls";

            if (dialog.ShowDialog() == true) {
                using (System.IO.FileStream fileStream = new System.IO.FileStream(dialog.FileName, FileMode.Create, FileAccess.Write)) {
                    gridTrailers.Export(fileStream, new GridViewExportOptions());
                    fileStream.Close();
                }
            }
        }

        private void WrappersViewSourceTrailer_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (TrailerViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchTextTrailer) ||
                (wrapper.Filter(Model.SearchTextTrailer));
        }

        private void tbFilterTrailer_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridTrailers.ItemsSource).Refresh();
        }

        #endregion [ Trailers ]

        #region [ Yards ]

        private async Task LoadYards_Completed(ViewModelWrapper<List<YardViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadYards_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                foreach (YardViewModel yard in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Yards.Add(yard);
                }
            }
        }

        private void SetYardsToTrailers() {
            if (Model.Yards.Any() && Model.Trailers.Any()) {
                foreach (var trailer in Model.Trailers) {
                    foreach (var yard in Model.Yards) {
                        if (trailer.YardId == yard.EntityId) {
                            trailer.YardName = yard.Name;
                        }
                    }
                }
            }
        }

        #endregion [ Yards ]  
    }
}