using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Win32;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditTrailerDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private DispatchService _service;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public TrailerViewModel Model {
            get { return DataContext as TrailerViewModel; }
            set { DataContext = value; }
        }

        public bool Reload { get; set; }

        private TrailerViewModel _clone;

        /// <summary>
        ///     Constructor </summary>
        public EditTrailerDialog(DispatchService service, TrailerViewModel trailer) {
            _service = service;

            InitializeComponent();

            Load(trailer);
        }

        private void Load(TrailerViewModel trailer) {
            if (trailer != null) {
                Model = trailer;
                Title = "Edit Trailer";
            } else {
                Model = new TrailerViewModel();
                Title = "Add Trailer";
            }
            _clone = (TrailerViewModel)Model.Clone();

        }

        private void EditTrailerDialog_Loaded(object sender, RoutedEventArgs e) {
            filesControl.FilesLoad(_service, tbError, Model.EntityId);
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            if (DispatchBoardViewModel.Instance.Trailers.Any(e => e.Number.Trim().ToLower() == Model.Number.Trim().ToLower() && e.EntityId != Model.EntityId)) {
                MessageBox.Show($"Trailer Number '{Model.Number}' already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _service.SaveTrailer(Model, SaveCompleted);

            base.BusyStart();
        }

        private async Task SaveCompleted(ViewModelWrapper<TrailerViewModel> wrapper) {
            await base.SaveCompleted(wrapper);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

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
            }
        }
        private void OpenMaintenance_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.EntityId,
                Name = Model.Maintenance_File
            });
        }

        private void BrowseMaintenance_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach Maintenance" };
            if (dialog.ShowDialog() == true) {
                Model.Maintenance_File = $"Maintenance_{DateTime.Now:yyyy-MM-dd}{Path.GetExtension(dialog.FileName)}";
                filesControl.FileSave(Model.Maintenance_File, File.ReadAllBytes(dialog.FileName));

                // resets expiration
                if (Model.Maintenance_Expiration == _clone.Maintenance_Expiration) {
                    Model.Maintenance_Expiration = null;
                }
            }
        }

        #endregion [ Files ]

        #region [ Defects ] 

        private void AddDefect_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new EditDefectDialog(_service, null, Model);
            if (dialog.ShowDialog() == true) {
                Load(dialog.TrailerModel);
                Reload = true;
            }
        }

        private void EditDefect_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new EditDefectDialog(_service, (DefectViewModel)((Hyperlink)sender).DataContext, Model);
            if (dialog.ShowDialog() == true) {
                Load(dialog.TrailerModel);
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
                Load(dialog.TrailerModel);
                Reload = true;
            }
        }

        private void LoadAll_OnClick(object sender, RoutedEventArgs e) {
            ((TextBlock)((Hyperlink)sender).Parent).Visibility = Visibility.Collapsed;
            _service.LoadDefects(null, this.Model.EntityId,LoadAllCompleted);
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