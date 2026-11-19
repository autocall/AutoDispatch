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
    public partial class FilePODDialog : FileBaseDialog {
        // without extension
        protected override string FileName => "POD";

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;
        protected override TextBox TbFile => tbFile;

        public LoadViewModel Model {
            get { return DataContext as LoadViewModel; }
            set { DataContext = value; }
        }

        public FilePODDialog(DispatchService service) :base(service) {
            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (FileData == null) {
                MessageBoxEx.Alert(this, "Please attach a file");
                return;
            }


            Model.PODDate = DateTime.Now;
            if (Model.Paid) {
                Model.Status = LoadStatus.Completed;
            }
            Model.PODFile = Path.GetFileName(tbFile.Text);
            var fileViewModel = new FileViewModel() {
                EntityId = Model.EntityId,
                CreatedDate = DateTime.Now,
                Name = Model.PODFile,
                Data = this.FileData,
            };

            if (FileSizeHelper.Check20MBSize(this.FileData)) {
                _service.SaveLoadAndAttachFile(Model, fileViewModel, SaveLoadAndAttachFile_Completed);
                BusyStart();
            }
        }

        private async Task SaveLoadAndAttachFile_Completed(ViewModelWrapper<LoadViewModel> wrapper1, ViewModelWrapper<FileViewModel> wrapper2) {
            await SaveCompleted(wrapper1, wrapper2);
        }
    }
}
