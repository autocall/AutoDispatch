using AutoCall.WPF.Helpers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class TrailerInfo : UserControl {
        private DispatchService _dispatchService = App.Services.GetRequiredService<DispatchService>();
        private DetailsTrailerLayoutDialog trailerDialog;

        public TrailerInfo() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            trailerDialog = UIElementHelper.FindVisualParent<DetailsTrailerLayoutDialog>(this);
        }

        private void BrowseScheme_OnClick(object sender, RoutedEventArgs e) {
            if (trailerDialog != null) {
                var dialog = new OpenFileDialog() { Title = "Attach Trailer Scheme" };
                if (dialog.ShowDialog() == true) {
                    SaveLoadAndAttachFile(File.ReadAllBytes(dialog.FileName), Path.GetExtension(dialog.FileName));
                }
            }
        }

        private void ScanScheme_OnClick(object sender, RoutedEventArgs e) {
            if (trailerDialog != null) {
            var dlg = new ScannerDialog();
                var pdfData = dlg.ShowDialog() == true ? dlg.GeneratePdf() : null;
                if (pdfData != null) {
                    SaveLoadAndAttachFile(pdfData, ".pdf");
                }
            }
        }

        private void SaveLoadAndAttachFile(byte[] data, string extension) {
            if (FileSizeHelper.Check20MBSize(data) == false) {
                return;
            }
            // to save file
            var fileName = $"trailer_scheme_{DateTime.Now:yyyyMMddHHmm}{extension}";

            trailerDialog.Model.Trailer.SchemeFile = fileName;
            trailerDialog.Model.Trailer.SchemeDate = DateTime.UtcNow;

            _dispatchService.SaveTrailer((TrailerViewModel)trailerDialog.Model.Trailer.Clone(), this.SaveCompleted);
            _counter++;

            foreach (var load in trailerDialog.Model.Loads) {
                load.Load.TrailerSchemeFile = fileName;
                load.Load.TrailerSchemeDate = DateTime.UtcNow;
                load.Load.TrailerSchemeId = trailerDialog.Model.Trailer.EntityId;

                _dispatchService.SaveLoad((LoadViewModel)load.Load.Clone(), this.SaveCompleted);
                _counter++;
            }

            // to upload file
            var viewModel = new FileViewModel();
            viewModel.EntityId = trailerDialog.Model.Trailer.EntityId;
            viewModel.Name = fileName;
            viewModel.Data = data;

            this._dispatchService.FileSave(viewModel, this.SaveCompleted);
            _counter++;

            trailerDialog.BusyStart();
            hasError = false;
        }

        // a count of requests
        private int _counter = 0;
        private bool  hasError = false;

        private async Task SaveCompleted<T>(ViewModelWrapper<T> wrapper) {
            _counter--;
            if (_counter == 0 && hasError == false) {
                await trailerDialog.SaveCompleted(wrapper);
            }
            if(wrapper.Status == AutoDispatch.Dtos.Enums.ResponseStatus.Failed) {
                await trailerDialog.SaveCompleted(wrapper);
                hasError = true;
            }
        }
    }
}
