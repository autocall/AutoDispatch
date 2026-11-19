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
    public partial class FileInvoiceDialog : FileBaseDialog {
        // without extension
        protected override string FileName => "invoce";

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

        public FileInvoiceDialog(DispatchService service) : base(service) {
            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(Model.InvoiceNumber)) {
                MessageBoxEx.Alert(this, "Please enter the invoice number");
                return;
            }

            //Model.InvoiceNumber = by binding
            Model.InvoiceDate = DateTime.Now;
            if (Model.Paid) {
                Model.Status = LoadStatus.Completed;
            }
            Model.InvoiceFile = null;
            if (FileData != null) {
                Model.InvoiceFile = Path.GetFileName(tbFile.Text);
                var fileViewModel = new FileViewModel() {
                    EntityId = Model.EntityId,
                    CreatedDate = DateTime.Now,
                    Name = Model.InvoiceFile,
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
            await SaveCompleted(wrapper1, wrapper2);
        }
    }
}
