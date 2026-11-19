using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls;
using Path = System.IO.Path;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class FileConfirmationDialog : FileBaseDialog {
        // without extension
        protected override string FileName => "confirmation";

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;
        protected override TextBox TbFile => tbFile;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public LoadViewModel Model {
            get { return DataContext as LoadViewModel; }
            set {
                DataContext = value;
                tbProId.Text = value.RecordId;
            }
        }

        /// <summary>
        ///     .ctor </summary>
        /// <param name="service">Dispatch Service</param>
        /// <param name="later">Changes Caption of button: True: Late, False: Close</param>
        public FileConfirmationDialog(DispatchService service, bool later = false) : base(service) {
            InitializeComponent();

            if (later) {
                CloseButton.Content = "Later";
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            if (FileData == null) {
                MessageBoxEx.Alert(this, "Please attach a file");
                return;
            }
            if (String.IsNullOrEmpty(tbProId.Text) == true) {
                MessageBoxEx.Alert(this, "Please enter \"Pro#\"");
                return;
            }

            Model.RecordId = tbProId.Text;

            if (String.IsNullOrEmpty(tbProId.Text) == false) {
                LoadViewModel exists;
                if (String.IsNullOrEmpty(Model.RecordId) == false && (exists = DispatchBoardViewModel.Instance.Loads.FirstOrDefault(l => Model.EntityId != l.EntityId && Model.EntityId != l.ParentId && l.RecordId == Model.RecordId)) != null) {
                    MessageBox.Show($"Pro# '{Model.RecordId}' already exists\nLoad#: {exists.IndexString}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            Model.ConfirmationDate = DateTime.Now;
            Model.ConfirmationFile = Path.GetFileName(tbFile.Text);
            var fileViewModel = new FileViewModel() {
                EntityId = Model.EntityId,
                CreatedDate = DateTime.Now,
                Name = Model.ConfirmationFile,
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
