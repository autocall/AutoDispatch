using AutoCall.Common.Extensions;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class ScanSchemeSendDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;
        private DispatchService _service => App.Services.GetRequiredService<DispatchService>();

        public ScanSchemeSendViewModel Model {
            get { return DataContext as ScanSchemeSendViewModel; }
            set { DataContext = value; }
        }

        private SettingsService settings => App.Services.GetRequiredService<SettingsService>();

        public ScanSchemeSendDialog(ActualCompanyViewModel actualCompany) {
            InitializeComponent();

            this.Model = new ScanSchemeSendViewModel() {
                ActualCompanyId = actualCompany.EntityId,
                Emails = settings.GetScanEmail()
            };
        }

        private void btnScan_OnClick(object sender, RoutedEventArgs e) {
            // аудит D19: V1 в Debug брал PDF с машины разработчика V1 (D:\Develop\AutoCall\…) — теперь сканер и в Debug
            var dlg = new ScannerDialog();
            var pdfData = dlg.ShowDialog() == true ? dlg.GeneratePdf() : null;
            if (pdfData != null) {
                var tmpFile = Path.Combine(Path.GetTempPath(), $"trailer_schemetmp_{Guid.NewGuid()}.pdf");
                Model.FileTempPath = tmpFile;
                Model.FileName = "scheme.pdf";
                File.WriteAllBytes(tmpFile, pdfData);
            }
            Model.PdfData = pdfData;
        }

        #region [ Clicked ]

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Model.Emails)) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Please enter a valid email";
                return;
            }
            if (string.IsNullOrEmpty(Model.Trailer)) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Please enter a valid trailer";
                return;
            }

            SendSchemeEmail();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Scheme_OnClick(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(Model.FileTempPath) { UseShellExecute = true });
        }

        #endregion [ Clicked ]

        #region [ Send Scheme ]

        private void SendSchemeEmail() {
            _service.SendSchemeEmail(Model.ActualCompanyId, Model.Emails, Model.Trailer, Model.PdfData, SendSchemeEmail_Completed);

            base.BusyStart();
        }

        private async Task SendSchemeEmail_Completed(ViewModelWrapper<bool> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SendSchemeEmail_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                MessageBoxEx.Info("Sucessed!");
            }

            await base.SaveCompleted(wrapper, close: false);
            this.settings.SetScanEmail(Model.Emails);

            var trailer = GetAssignedTrailer(Model.Trailer);
            if (String.IsNullOrEmpty(Model.Trailer) == false && trailer != null) {
                SaveLoadAndAttachFile(trailer, File.ReadAllBytes(Model.FileTempPath));
            } else {
                Close();
            }
        }

        #endregion [ Send Scheme ]

        #region [ Helper ]

        private TrailerViewModel GetAssignedTrailer(string trailer) {
            return DispatchBoardViewModel.Instance.Wrappers
                .FirstOrDefault(e =>
                    e.TrailerNumber != null &&
                    e.TrailerNumber.OnlyNumbersAndLetters().ToLower() == trailer.OnlyNumbersAndLetters().ToLower())?.Trailer;
        }

        #endregion [ Helper ]

        #region [ Attach Scheme ]

        private void SaveLoadAndAttachFile(TrailerViewModel trailer, byte[] data) {
            if (FileSizeHelper.Check20MBSize(data) == false) {
                return;
            }
            // to save file
            var fileName = $"trailer_scheme_{DateTime.Now:yyyyMMddHHmm}.pdf";

            trailer.SchemeFile = fileName;
            trailer.SchemeDate = DateTime.UtcNow;

            this._service.SaveTrailer((TrailerViewModel)trailer.Clone(), this.SaveCompleted);
            _counter++;

            // to upload file
            var viewModel = new FileViewModel();
            viewModel.EntityId = trailer.EntityId;
            viewModel.Name = fileName;
            viewModel.Data = data;

            this._service.FileSave(viewModel, this.SaveCompleted);
            _counter++;

            base.BusyStart();
            hasError = false;
        }

        // a count of requests
        private int _counter = 0;
        private bool hasError = false;

        private async Task SaveCompleted<T>(ViewModelWrapper<T> wrapper) {
            _counter--;
            if (_counter == 0 && hasError == false) {
                await base.SaveCompleted(wrapper);
            }
            if (wrapper.Status == ResponseStatus.Failed) {
                await base.SaveCompleted(wrapper);
                hasError = true;
            }
        }

        #endregion [ Attach Scheme ]

    }
}
