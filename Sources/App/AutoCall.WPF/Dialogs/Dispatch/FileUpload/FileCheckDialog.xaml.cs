using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Telerik.Windows.Controls;
using Path = System.IO.Path;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class FileCheckDialog : FileBaseDialog {
        // without extension
        protected override string FileName => "check";

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;
        protected override TextBox TbFile => tbFile;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public LoadViewModel Model {
            get { return DataContext as LoadViewModel; }
            set { DataContext = value; }
        }

        public FileCheckDialog(DispatchService service) : base(service) {
            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(Model.CheckNumber)) {
                MessageBoxEx.Alert(this, "Please enter the Check number");
                return;
            }


            //Model.CheckNumber = by binding
            Model.CheckDate = DateTime.Now;
            if (Model.Paid) {
                Model.Status = LoadStatus.Completed;
            }
            Model.CheckFile = null;
            if (FileData != null) {
                Model.CheckFile = Path.GetFileName(tbFile.Text);
                var fileViewModel = new FileViewModel() {
                    EntityId = Model.EntityId,
                    CreatedDate = DateTime.Now,
                    Name = Model.CheckFile,
                    Data = this.FileData,
                };

                if (FileSizeHelper.Check20MBSize(this.FileData)) {
                    _service.SaveLoadAndAttachFile(Model, fileViewModel, SaveLoadAndAttachFile_Completed);
                    BusyStart();
                }
            } else {
                _service.SaveLoad(Model, SaveLoad_Completed);
                BusyStart();
            }
        }

        private async Task SaveLoadAndAttachFile_Completed(ViewModelWrapper<LoadViewModel> wrapper1, ViewModelWrapper<FileViewModel> wrapper2) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                await Application.Current.Dispatcher.InvokeAsync(async () => await SaveLoadAndAttachFile_Completed(wrapper1, wrapper2));
                return;
            }
            await SaveCompleted(wrapper1, wrapper2);
        }
    }
}
