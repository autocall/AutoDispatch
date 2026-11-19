using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.Windows.Input;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class DriversDialog : BaseDispatchDialog {

        #region [ Base ]

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        #endregion [ Base ]

        #region [ _fields ]

        private DispatchService _service;
        private SettingsService _settings;
        private DispatchActions dispatchActions;

        #endregion [ _fields ]

        #region [ Propertiens ]

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DriversViewModel Model {
            get { return DataContext as DriversViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Propertiens ]

        #region [ .ctor ]

        public DriversDialog(DispatchService service, SettingsService settings) {
            _service = service;
            _settings = settings;
            dispatchActions = new DispatchActions(null);
            Model = new DriversViewModel();

            InitializeComponent();
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        private void Initialize() {
            _service.LoadDrivers(LoadDrivers_Completed);
            _service.LoadTrucks(LoadTrucks_Completed);
        }

        private async Task LoadDrivers_Completed(ViewModelWrapper<List<DriverViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadDrivers_Completed(wrapper));
                return;
            }
            loadingDrivers.Visibility = Visibility.Collapsed;
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Model.Drivers.Clear();
                foreach (DriverViewModel driver in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Drivers.Add(driver);
                }
            }
        }

        private async Task LoadTrucks_Completed(ViewModelWrapper<List<TruckViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadTrucks_Completed(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Model.Trucks.Clear();
                foreach (TruckViewModel truck in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Trucks.Add(truck);
                }
            }
        }

        #endregion [ Initialize ]

        #region [ Event Handlers ]

        private void DriversDialog_OnLoaded(object sender, RoutedEventArgs e) {
            Initialize();
            try {
                var data = _settings.GetEquipmentsDialogGridViewDrivers();
                if (data != null) {
                    gridDrivers.SetSettings(data);
                }
            } catch {
            }
        }

        private void DriversDialog_OnClosed(object sender, EventArgs e) {
            loadingDrivers.Visibility = Visibility.Visible;
            _settings.SetEquipmentsDialogGridViewDrivers(gridDrivers.GetSettings());
        }

        private void AddDriver_Clicked(object sender, RoutedEventArgs e) {
            AddDriver();
        }

        private void EditDriver_Clicked(object sender, RoutedEventArgs e) {
            if (gridDrivers.SelectedItem != null) {
                EditDriver((DriverViewModel)gridDrivers.SelectedItem);
            }
        }

        private void DeleteDriver_Clicked(object sender, RoutedEventArgs e) {
            if (gridDrivers.SelectedItem != null) {
                DeleteDriver((DriverViewModel)gridDrivers.SelectedItem);
            }
        }

        private void gridDrivers_RowDoubleClick(object sender, MouseButtonEventArgs e) {
            EditDriver((DriverViewModel)((GridViewRow)e.Source).DataContext);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Open_THE_APPLICATION_OF_EMPLOYMENT_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.THE_APPLICATION_OF_EMPLOYMENT_File,
            });
        }

        private void Open_MVR_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.MVR_File,
            });
        }

        private void Open_MEDICAL_CARD_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.MEDICAL_CARD_File,
            });
        }

        private void Open_CDL_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.CDL_File,
            });
        }

        private void Open_DOT_INSPECTIONS_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.DOT_INSPECTIONS_File,
            });
        }

        private void Open_BACKGROUND_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.BACKGROUND_File,
            });
        }

        private void Open_DRUG_TEST_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.DRUG_TEST_File,
            });
        }

        private void Open_RANDOM_DRUG_TEST_OnClick(object sender, RoutedEventArgs e) {
            var driver = (DriverViewModel)((Hyperlink)sender).DataContext;
            dispatchActions.FileLoad(new FileViewModel() {
                EntityId = driver.EntityId,
                Name = driver.RANDOM_DRUG_TEST_File,
            });
        }

        #region open

        private string _fileName;
        private async void NewDriverApplicationPackage_OnClick(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "New Driver Application Package.pdf";
            dialog.DefaultExt = "pdf";
            dialog.Filter = "*.pdf|*.pdf";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                _fileName = dialog.FileName;
                BusyStart("Downloading...");
                progressBar.IsIndeterminate = false;
                using (WebClient wc = new WebClient()) {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    await wc.DownloadFileTaskAsync(
                        // V1: UrlApi + "\\Download\\…" давал "//Download" (IIS принимал); RequestPath /Download на Kestrel — нет
                        new Uri(new Uri(AutoCall.Common.Settings.WebUrl), "Download/New Driver Application Package.pdf"),
                        dialog.FileName
                    );
                }
                BusyFinish();
            }
        }

        // Event to track the progress
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            progressBar.Value = e.ProgressPercentage;

            if (e.ProgressPercentage == 100) {
                progressBar.IsIndeterminate = true;
                BusyFinish();
                var process = new Process {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo(_fileName) { WorkingDirectory = Path.GetDirectoryName(_fileName) }
                };
                process.Start();
            }
        }

        #endregion open

        #endregion [ Event Handlers ]

        #region [ Actions ]

        private void AddDriver() {
            var dialog = new EditDriverDialog(_service, null,
              new ObservableCollection<TruckViewModel>(Model.Trucks.Where(t =>
                Model.Drivers.Any(driver => driver.TruckId == t.EntityId) == false)));
            if (dialog.ShowDialog() == true) {
                Initialize();
            }
        }

        private void EditDriver(DriverViewModel model) {
            var clone = (DriverViewModel)model.Clone();
            var dialog = new EditDriverDialog(_service, clone,
                new ObservableCollection<TruckViewModel>(Model.Trucks.Where(t =>
                   t.EntityId == clone.TruckId || Model.Drivers.Any(driver => driver.TruckId == t.EntityId) == false)));
            if (dialog.ShowDialog() == true) {
                Initialize();
            }
        }

        private void DeleteDriver(DriverViewModel model) {
            if (MessageBox.Show($"Are you sure you want to delete selected driver?\n{model.DisplayName}", "Delete driver",
              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            this._service.DeleteDriver(model.EntityId,
                async (w) => {
                    await this.SaveCompleted(w, false);
                    Initialize();
                });
            base.BusyStart("Deleting");
        }

        #endregion [ Actions ]
    }
}