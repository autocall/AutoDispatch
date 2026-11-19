using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Win32;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditTruckDialog : BaseDispatchDialog {

        #region [ Base ]

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        #endregion [ Base ]

        #region [ _fields ]

        private DispatchService _service;

        #endregion [ _fields ]

        #region [ View Model ]

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public TruckViewModel Model {
            get { return DataContext as TruckViewModel; }
            set { DataContext = value; }
        }

        public bool Reload { get; set; }

        private TruckViewModel _clone;

        #endregion [ View Model ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public EditTruckDialog(DispatchService service, TruckViewModel truck, IList<TrailerViewModel> trailers) {
            _service = service;

            InitializeComponent();

            Load(truck);
            cbTrailers.ItemsSource = trailers.Where(e => e.BnsfUnit == null).OrderBy(e => e.Number).ToList();
        }

        private void Load(TruckViewModel truck) {
            if (truck != null) {
                Model = truck;
                Title = "Edit Truck";
            } else {
                Model = new TruckViewModel();
                Title = "Add Truck";
            }
            _clone = (TruckViewModel)Model.Clone();

        }

        #endregion [ .ctor ]

        #region [ Event Handlers ]

        private void EditTruckDialog_Loaded(object sender, RoutedEventArgs e) {
            filesControl.FilesLoad(_service, tbError, Model.EntityId);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            if (DispatchBoardViewModel.Instance.Trucks.Any(e => e.Number.Trim().ToLower() == Model.Number.Trim().ToLower() && e.EntityId != Model.EntityId)) {
                MessageBox.Show($"Truck Number '{Model.Number}' already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Model.AnnualInspection_File != null && Model.AnnualInspection_Expiration == null) {
                MessageBoxEx.Error(this, "Please enter an expiration date of 'Annual Inspection'");
                return;
            }

            _service.SaveTruck(Model, (w) => SaveCompleted(w));

            base.BusyStart();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        #endregion [ Event Handlers ]

        #region [ Files ]

        private void OpenAnnualInspection_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.AnnualInspection_File
            });
        }

        private void BrowseAnnualInspection_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach Annual Inspection" };
            if (dialog.ShowDialog() == true) {
                Model.AnnualInspection_File = $"AnnualInspection_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.AnnualInspection_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.AnnualInspection_Expiration == _clone.AnnualInspection_Expiration) {
                    Model.AnnualInspection_Expiration = null;
                }
            }
        }

        private void OpenRegistration_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.Registration_File
            });
        }

        private void BrowseRegistration_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach Registration" };
            if (dialog.ShowDialog() == true) {
                Model.Registration_File = $"Registration_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.Registration_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.Registration_Expiration == _clone.Registration_Expiration) {
                    Model.Registration_Expiration = null;
                }
            }
        }

        #endregion [ Files ]

        #region [ Defects ] 

        private void AddDefect_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new EditDefectDialog(_service, null, Model);
            if (dialog.ShowDialog() == true) {
                Load(dialog.TruckModel);
                Reload = true;
            }
        }

        private void EditDefect_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new EditDefectDialog(_service, (DefectViewModel)((Hyperlink)sender).DataContext, Model);
            if (dialog.ShowDialog() == true) {
                Load(dialog.TruckModel);
                Reload = true;
            }
        }

        private void Repair_OnClick(object sender, RoutedEventArgs e) {
            DefectViewModel defectModel =
                (sender as FrameworkContentElement)?.DataContext as DefectViewModel ??
                (sender as Button)?.DataContext as DefectViewModel;
            DefectRepairViewModel repairModel = defectModel.Repair;

            var dialog = new EditDefectRepairDialog(_service, defectModel, (DefectRepairViewModel)repairModel?.Clone(), Model);
            if (dialog.ShowDialog() == true) {
                Load(dialog.TruckModel);
                Reload = true;
            }
        }

        private void LoadAll_OnClick(object sender, RoutedEventArgs e) {
            ((TextBlock)((Hyperlink)sender).Parent).Visibility = Visibility.Collapsed;
            _service.LoadDefects(this.Model.EntityId, null, LoadAllCompleted);
            this.BusyStart();
        }

        private async Task LoadAllCompleted(ViewModelWrapper<List<DefectViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadAllCompleted(wrapper));
                return;
            }

            await SaveCompleted(wrapper, false);

            if (wrapper.Status == AutoDispatch.Dtos.Enums.ResponseStatus.Success) {
                Model.Defects.Clear();
                foreach (var defect in wrapper.Data.OrderByDescending(e => e.CreatedDate)) {
                    Model.Defects.Add(defect);
                }
            }
        }

        #endregion [ Defects ] 
    }
}