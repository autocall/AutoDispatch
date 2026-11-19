using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Attributes;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using log4net.Layout;
using Telerik.Windows.Controls;
using Path = System.IO.Path;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public class FileCheckMultiple : ViewModel {
        private string _checkNumber;
        private string _checkFile;
        private DateTime? _checkDate;
        private float? _payout;

        public string Header => $"Attrach Check ({this.Loads.Count} loads)";

        public List<LoadViewModel> Loads { get; set; }

        [CheckValidation]
        public string CheckNumber {
            get { return _checkNumber; }
            set {
                if (_checkNumber != value) {
                    _checkNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CheckFile {
            get { return _checkFile; }
            set {
                if (_checkFile != value) {
                    _checkFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? CheckDate {
            get { return _checkDate; }
            set {
                if (_checkDate != value) {
                    _checkDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public float? Payout {
            get { return _payout; }
            set {
                if (_payout != value) {
                    _payout = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public partial class FileCheckMultipleDialog : FileBaseDialog {
        // without extension
        protected override string FileName => "check";

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;
        protected override TextBox TbFile => tbFile;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public FileCheckMultiple Model {
            get { return DataContext as FileCheckMultiple; }
            set { DataContext = value; }
        }

        public FileCheckMultipleDialog(DispatchService service) : base(service) {
            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.Loads.Any() == false) {
                MessageBoxEx.Alert(this, "No Loads");
                return;
            }
            if (String.IsNullOrEmpty(Model.CheckNumber)) {
                MessageBoxEx.Alert(this, "Please enter the Check number");
                return;
            }

            this.ProcessCounter = 0;
            foreach (var load in Model.Loads) {
                load.CheckNumber = Model.CheckNumber;
                load.CheckDate = DateTime.Now;
                if (load.Paid) {
                    load.Status = LoadStatus.Completed;
                }
                load.CheckFile = null;
                if (FileData != null) {
                    load.CheckFile = Path.GetFileName(tbFile.Text);
                    var fileViewModel = new FileViewModel() {
                        EntityId = load.EntityId,
                        CreatedDate = DateTime.Now,
                        Name = load.CheckFile,
                        Data = this.FileData,
                    };

                    if (FileSizeHelper.Check20MBSize(this.FileData)) {
                        this.ProcessCounter++;
                        _service.SaveLoadAndAttachFile(load, fileViewModel, SaveLoadAndAttachFile_Completed);
                    } else {
                        return;
                    }
                } else {
                    this.ProcessCounter++;
                    _service.SaveLoad(load, this.SaveLoad_Completed2);
                }
            }
            BusyStart();
        }

        private int ProcessCounter = 0;

        private async Task SaveLoadAndAttachFile_Completed(ViewModelWrapper<LoadViewModel> wrapper1, ViewModelWrapper<FileViewModel> wrapper2) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                await Application.Current.Dispatcher.InvokeAsync(async () => await SaveLoadAndAttachFile_Completed(wrapper1, wrapper2));
                return;
            }
            this.ProcessCounter--;
            if (wrapper1.Status == ResponseStatus.Failed || wrapper2.Status == ResponseStatus.Failed || this.ProcessCounter == 0) {
                await SaveCompleted(wrapper1, wrapper2, this.ProcessCounter == 0);
            }
        }

        private async Task SaveLoad_Completed2(ViewModelWrapper<LoadViewModel> wrapper) {
            this.ProcessCounter--;
            if (wrapper.Status == ResponseStatus.Failed || this.ProcessCounter == 0) {
                await base.SaveCompleted(wrapper, this.ProcessCounter == 0);
            }
        }
    }
}
