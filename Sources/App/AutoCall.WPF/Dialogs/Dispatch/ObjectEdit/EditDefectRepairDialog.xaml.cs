using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Helpers;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Win32;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditDefectRepairDialog : BaseDispatchDialog {

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
        public DefectRepairViewModel Model {
            get { return DataContext as DefectRepairViewModel; }
            set { DataContext = value; }
        }

        public TruckViewModel TruckModel { get; set; }
        public TrailerViewModel TrailerModel { get; set; }
        public DefectViewModel DefectModel { get; set; }

        #endregion [ View Model ]

        #region [ .ctor ]

        /// <summary>
        ///     .ctor </summary>
        private EditDefectRepairDialog(DispatchService service, DefectViewModel defect, DefectRepairViewModel repair) {
            _service = service;

            InitializeComponent();

            if (repair != null) {
                Model = (DefectRepairViewModel)repair.Clone();
                Title = "Edit Repair";
            } else {
                Model = new DefectRepairViewModel() {
                    Date = DateTime.Now.Date
                };
                Title = "Add Repair";
            }

            this.DefectModel = (DefectViewModel)defect.Clone();
            DefectModel.RepairId = Model.EntityId;
            DefectModel.Repair = Model;
        }
        /// <summary>
        ///     .ctor </summary>
        public EditDefectRepairDialog(DispatchService service, DefectViewModel defect, DefectRepairViewModel repair, TruckViewModel truck)
            : this(service, defect, repair) {

            TruckModel = (TruckViewModel)truck.Clone();
            var oldModel = TruckModel.Defects.First(e => e.EntityId == defect.EntityId);
            var index = TruckModel.Defects.IndexOf(oldModel);
            TruckModel.Defects[index] = DefectModel;
        }
        /// <summary>
        ///     .ctor </summary>
        public EditDefectRepairDialog(DispatchService service, DefectViewModel defect, DefectRepairViewModel repair, TrailerViewModel trailer)
            : this(service, defect, repair) {

            TrailerModel = (TrailerViewModel)trailer.Clone();
            var oldModel = TrailerModel.Defects.First(e => e.EntityId == defect.EntityId);
            var index = TrailerModel.Defects.IndexOf(oldModel);
            TrailerModel.Defects[index] = DefectModel;
        }

        #endregion [ .ctor ]

        private void DeleteFile_OnClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Please confirm to delete file", "Delete File", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                return;
            }

            var model = new FileViewModel() {
                EntityId = Model.EntityId,
                Name = (string)((FrameworkElement)sender).DataContext
            };

            _service.FileDelete(model, async (w) => {
                await Dispatcher.InvokeAsync((Action)(() => {
                    Model.Files.Remove(w.Data.Name);
                }));
                await this.SaveCompleted(w, false);
            });
        }

        private void AddFile_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Upload File" };
            if (dialog.ShowDialog() == true) {
                var viewModel = new FileViewModel();
                viewModel.EntityId = Model.EntityId;
                var fileName = Path.GetFileName(dialog.FileName);
                while (Model.Files.Contains(fileName)) {
                    fileName = AutoDispatch.Logic.Heplers.FileHelper.IncFileName(fileName);
                }
                viewModel.Name = fileName;
                viewModel.Data = BinaryHelper.ReadFully(dialog.OpenFile());

                if (FileSizeHelper.Check20MBSize(viewModel.Data)) {
                    this._service.FileSave(viewModel, async (w) => {
                        await Dispatcher.InvokeAsync((Action)(() => {
                            Model.Files.Add(w.Data.Name);
                        }));
                        await this.SaveCompleted(w, false);
                    });
                    BusyStart();
                }
            }
        }

        private void OpenFile_OnClick(object sender, RoutedEventArgs e) {
            var fileViewModel = new FileViewModel() {
                EntityId = Model.EntityId,
                Name = (string)((FrameworkContentElement)sender).DataContext
            };
            _service.FileLoad(fileViewModel, FileLoadCompeleted);
            this.BusyStart("File downloading...");
        }

        private async Task FileLoadCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            await this.Dispatcher.InvokeAsync((Action)(() => {
                base.BusyFinish();
                if (wrapper.Status == ResponseStatus.Success) {
                    try {
                        var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                        if (Directory.Exists(path) == false) {
                            Directory.CreateDirectory(path);
                        }
                        var fullPath = Path.Combine(path, wrapper.Data.Name);
                        File.WriteAllBytes(fullPath, wrapper.Data.Data);
                        var process = new Process {
                            EnableRaisingEvents = true,
                            StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path, UseShellExecute = true }
                        };
                        process.Start();
                    } catch (Exception exception) {
                        MessageBoxEx.Error(exception.Message);
                    }
                } else {
                    MessageBoxEx.Error(wrapper.MessageError);
                }
            }));
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (TruckModel != null) {
                _service.SaveTruck(TruckModel, (w) => this.SaveCompleted(w));
            }
            if (TrailerModel != null) {
                _service.SaveTrailer(TrailerModel, (w) => this.SaveCompleted(w));
            }
            base.BusyStart();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void btnDelete_Clicked(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Please confirm to delete repair", "Delete Repair", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                return;
            }
            _service.DeleteDefectRepair(Model.EntityId, async (w) => {
                DefectModel.RepairId = null;
                DefectModel.Repair = null;
                await this.SaveCompleted(w);
            });
            base.BusyStart();
        }
    }
}