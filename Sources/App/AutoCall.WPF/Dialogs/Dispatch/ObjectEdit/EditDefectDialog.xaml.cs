using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AutoCall.Common.Helpers;
using AutoCall.WPF.Converters;
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
    public partial class EditDefectDialog : BaseDispatchDialog {

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
        public DefectViewModel Model {
            get { return DataContext as DefectViewModel; }
            set { DataContext = value; }
        }

        public TruckViewModel TruckModel { get; set; }
        public TrailerViewModel TrailerModel { get; set; }

        private bool _editMode;

        #endregion [ View Model ]

        #region [ .ctor ]

        /// <summary>
        ///     .ctor </summary>
        private EditDefectDialog(DispatchService service, DefectViewModel defect) {
            _service = service;

            InitializeComponent();

            if (defect != null) {
                Model = defect;
                Title = "Edit Defect";
                _editMode = true;
            } else {
                Model = new DefectViewModel();
                Title = "Add Defect";
                _editMode = false;
            }
        }
        /// <summary>
        ///     .ctor </summary>
        public EditDefectDialog(DispatchService service, DefectViewModel defect, TruckViewModel truck) : this(service, defect) {
            cbDefects.ItemsSource = Enum.GetValues(typeof(DefectTruckEnum)).Cast<DefectTruckEnum>().ToList();
            cbDefects.SetBinding(ComboBox.SelectedItemProperty, new Binding(nameof(Model.DefectId)) {
                Converter = new EnumToIntConverter(),
                ConverterParameter = typeof(DefectTruckEnum),
                Mode = BindingMode.TwoWay
            });
            Model.TruckId = truck.EntityId;
            TruckModel = (TruckViewModel)truck.Clone();
            if (_editMode) {
                var oldModel = TruckModel.Defects.FirstOrDefault(e => e.EntityId == Model.EntityId);
                var index = TruckModel.Defects.IndexOf(oldModel);
                TruckModel.Defects[index] = Model;
            } else {
                TruckModel.Defects.Add(Model);
            }
        }
        /// <summary>
        ///     .ctor </summary>
        public EditDefectDialog(DispatchService service, DefectViewModel defect, TrailerViewModel trailer) : this(service, defect) {
            cbDefects.ItemsSource = Enum.GetValues(typeof(DefectTrailerEnum)).Cast<DefectTrailerEnum>().ToList();
            cbDefects.SetBinding(ComboBox.SelectedItemProperty, new Binding(nameof(Model.DefectId)) {
                Converter = new EnumToIntConverter(),
                ConverterParameter = typeof(DefectTrailerEnum),
                Mode = BindingMode.TwoWay
            });
            Model.TrailerId = trailer.EntityId;
            TrailerModel = (TrailerViewModel)trailer.Clone();
            if (_editMode) {
                var oldModel = TrailerModel.Defects.FirstOrDefault(e => e.EntityId == Model.EntityId);
                var index = TrailerModel.Defects.IndexOf(oldModel);
                TrailerModel.Defects[index] = Model;
            } else {
                TrailerModel.Defects.Add(Model);
            }
        }

        #endregion [ .ctor ]

        private void DeleteFile_OnClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Please confirm to delete defect", "Delete Defect", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
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
                await this .SaveCompleted(w, false);
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
            if (MessageBox.Show("Please confirm to delete defect", "Delete Defect", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                return;
            }
            _service.DeleteDefect(Model.EntityId, async (w) => {
                await Dispatcher.InvokeAsync((Action)(() => {
                    if (TruckModel != null) {
                        TruckModel.Defects.Remove(Model);
                    }
                    if (TrailerModel != null) {
                        TrailerModel.Defects.Remove(Model);
                    }
                }));
                await this.SaveCompleted(w);
            });
            base.BusyStart();
        }
    }
}