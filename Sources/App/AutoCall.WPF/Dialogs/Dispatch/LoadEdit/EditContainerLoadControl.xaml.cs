using System;
using AutoDispatch.Logic.Converters;
using AutoDispatch.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoCall.WPF.UserControls;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoCall.Common.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class EditContainerLoadControl : EditBaseLoadControl {

        public EditContainerLoadControl() {
            InitializeComponent();
        }

        private async void EditBaseLoadControl_Loaded(object sender, RoutedEventArgs e) {
            var dispatchSession = UserSession.Current;
            if (((RoleName)dispatchSession.Roles).CheckPermission(RoleName.Agent) == false) {
                documentsGrid.Visibility =
                    a08.Visibility =
                    a09.Visibility =
                    a10.Visibility =
                    a11.Visibility =
                    a12.Visibility =
                    //a13.Visibility =
                    a14.Visibility =
                    //a15.Visibility =
                    //a16.Visibility =
                    Visibility.Collapsed;
            }

            //#if DEBUG
            //            while (Model == null) {
            //                await Task.Delay(TimeSpan.FromSeconds(1));
            //            }
            //            PreviewImages_Click(null, null);
            //#endif
        }

        private void EditBaseLoadControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs args) {
            var actualCompany = Model.Root.ActualCompanies.FirstOrDefault(e => e.EntityId == Model.Load.ActualCompanyId);
            if (actualCompany == null || actualCompany.CarrierViewToken == null) {
                a15.Visibility = a16.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadType_OnClicked(object sender, RoutedEventArgs e) {
            Model.Load.LoadType = Model.Load.LoadType == LoadType.Full ? LoadType.Partial : LoadType.Full;
        }

        private void Equipment_OnClicked(object sender, RoutedEventArgs e) {
            switch (Model.Load.Equipment) {
                case AutoCallEquipmentType.Van:
                    Model.Load.Equipment = AutoCallEquipmentType.Reefer;
                    break;
                case AutoCallEquipmentType.Reefer:
                    Model.Load.Equipment = AutoCallEquipmentType.VanorReefer;
                    Model.Load.Temperature = null;
                    break;
                case AutoCallEquipmentType.VanorReefer:
                    Model.Load.Equipment = AutoCallEquipmentType.StraightBoxTruck;
                    Model.Load.Temperature = null;
                    break;
                case AutoCallEquipmentType.StraightBoxTruck:
                    Model.Load.Equipment = AutoCallEquipmentType.Van;
                    Model.Load.Temperature = null;
                    break;
                default:
                    Model.Load.Equipment = AutoCallEquipmentType.Van;
                    Model.Load.Temperature = null;
                    break;
            }
        }

        private void PalletType_OnClicked(object sender, RoutedEventArgs e) {
            switch (Model.Load.PalletType) {
                case PalletType.Standard:
                    Model.Load.PalletType = PalletType.Oversize;
                    break;
                case PalletType.Oversize:
                    Model.Load.PalletType = PalletType.FloorLoaded;
                    break;
                case PalletType.FloorLoaded:
                    Model.Load.PalletType = PalletType.Standard;
                    break;
                default:
                    Model.Load.PalletType = PalletType.Standard;
                    break;
            }
        }

        private void BrowseFile_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach File" };
            if (dialog.ShowDialog() == true) {
                filesControl.FileSave(dialog.FileName);
            }
        }

        private void BrowseConfirmation_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach Confirmation" };
            if (dialog.ShowDialog() == true) {
                Model.Load.ConfirmationFile = Path.GetFileName(dialog.FileName);
                Model.Load.ConfirmationDate = DateTime.Now;
                filesControl.FileSave(dialog.FileName, undoAction: () => {
                    Model.Load.ConfirmationFile = null;
                    Model.Load.ConfirmationDate = null;
                });
            }
        }

        private void BrowseBOL_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach BOL" };
            if (dialog.ShowDialog() == true) {
                Model.Load.BOLFile = Path.GetFileName(dialog.FileName);
                Model.Load.BOLDate = DateTime.Now;
                filesControl.FileSave(dialog.FileName);
            }
        }

        private void BrowsePOD_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach POD" };
            if (dialog.ShowDialog() == true) {
                Model.Load.PODFile = Path.GetFileName(dialog.FileName);
                Model.Load.PODDate = DateTime.Now;
                filesControl.FileSave(dialog.FileName);
            }
        }

        private void BrowseLumper_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach Lumper" };
            if (dialog.ShowDialog() == true) {
                Model.Load.LumperFile = Path.GetFileName(dialog.FileName);
                Model.Load.LumperDate = DateTime.Now;
                filesControl.FileSave(dialog.FileName);
            }
        }

        private void BrowsePaperwork_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() { Title = "Attach Paperwork" };
            if (dialog.ShowDialog() == true) {
                Model.Load.PaperworkFile = Path.GetFileName(dialog.FileName);
                Model.Load.PaperworkDate = DateTime.Now;
                filesControl.FileSave(dialog.FileName);
            }
        }

        private void BrowsePhoto_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog() {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
                Title = "Upload Photo"
            };
            if (dialog.ShowDialog() == true) {
                Model.Load.PhotoFile = Path.GetFileName(dialog.FileName);
                filesControl.FileSave(dialog.FileName);
            }
        }

        private async void RadAutoCompleteBox_SearchAddressTextChanged(object sender, EventArgs e) {
            await RadAutoCompleteBox_SearchAddressTextChanged(sender, e,
               ((FrameworkElement)sender).Tag.Equals(CacheLoadIndexType.Origin) ? this.originBusy : this.descriptionBusy);
        }

        protected async void RadAutoCompleteBox_SearchAddressSelectionChanged(object sender, SelectionChangedEventArgs e) {
            await RadAutoCompleteBox_SearchAddressSelectionChanged(sender, e,
               ((FrameworkElement)sender).Tag.Equals(CacheLoadIndexType.Origin) ? this.originBusy : this.descriptionBusy);
        }

        /// <summary>
        ///     Synchronizes dates </summary>
        private void Pickup_DropDownClosed(object sender, RoutedEventArgs e) {
            if (Model.Root.Load.ContainerType == ContainerType.Quantity) {
                foreach (WrapperLoadViewModel subLoad in Model.Root.Loads) {
                    subLoad.Load.PickupDate = Model.Root.Load.PickupDate;
                    subLoad.Load.PickupFrom = Model.Root.Load.PickupFrom;
                    subLoad.Load.PickupTo = Model.Root.Load.PickupTo;
                }
            }
        }

        /// <summary>
        ///     Synchronizes dates </summary>
        private void Delivery_DropDownClosed(object sender, RoutedEventArgs e) {
            if (Model.Root.Load.ContainerType == ContainerType.Quantity) {
                foreach (WrapperLoadViewModel subLoad in Model.Root.Loads) {
                    subLoad.Load.DeliveryDate = Model.Root.Load.DeliveryDate;
                    subLoad.Load.DeliveryFrom = Model.Root.Load.DeliveryFrom;
                    subLoad.Load.DeliveryTo = Model.Root.Load.DeliveryTo;
                }
            }
        }

        /// <summary>
        ///     Begins to load files </summary>
        public void FilesLoad() {
            filesControl.FilesLoad(_dispatchService, _errorTextBlock, Model.Load);
        }

        public void filesControl_FileDeleted(FileViewModel file) {
            if (Model.Load.ConfirmationFile == file.Name) {
                Model.Load.ConfirmationFile = null;
                Model.Load.ConfirmationDate = null;
            }
            if (Model.Load.BOLFile == file.Name) {
                Model.Load.BOLFile = null;
                Model.Load.BOLDate = null;
            }
            if (Model.Load.PODFile == file.Name) {
                Model.Load.PODFile = null;
                Model.Load.PODDate = null;
            }
            if (Model.Load.LumperFile == file.Name) {
                Model.Load.LumperFile = null;
                Model.Load.LumperDate = null;
            }
            if (Model.Load.PaperworkFile == file.Name) {
                Model.Load.PaperworkFile = null;
                Model.Load.PaperworkDate = null;
            }
            if (Model.Load.InvoiceFile == file.Name) {
                Model.Load.InvoiceFile = null;
                Model.Load.InvoiceDate = null;
            }
            if (Model.Load.PhotoFile == file.Name) {
                Model.Load.PhotoFile = null;
            }
            if (Model.Load.ConfirmedFile == file.Name) {
                Model.Load.ConfirmedFile = null;
                Model.Load.ConfirmedDate = null;
            }
        }

        private void Confirmation_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.ConfirmationFile
            });
        }

        private void BOL_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.BOLFile
            });
        }

        private void POD_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.PODFile
            });
        }

        private void Lumper_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.LumperFile
            });
        }

        private void Paperwork_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.PaperworkFile
            });
        }

        private void Invoice_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.InvoiceFile
            });
        }

        private void Photo_OnClick(object sender, RoutedEventArgs e) {
            filesControl.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.PhotoFile
            });
        }

        private void EditExpenses_OnClick(object sender, RoutedEventArgs args) {
            var dlg = new EditExpensesDialog(Model.Load.LineHaul);
            dlg.ViewModel = Model.Load.Expenses.Where(e => e.Type != ExpenseType.none).Select(e => (ExpenseViewModel)e.Clone()).ToList();
            if (dlg.ShowDialog() == true) {
                Model.Load.Expenses.Where(e => e.Type != ExpenseType.none).ForEach(old => old.Assign(dlg.ViewModel.First(@new => @new.Type == old.Type)));
                Model.Load.ExpensesChanged();
            }
        }

        private void EditGains_OnClick(object sender, RoutedEventArgs args) {
            var dlg = new EditGainsDialog(Model.Load.LineHaul);
            dlg.ViewModel = Model.Load.Gains.Where(e => e.Type != GainType.none).Select(e => (GainViewModel)e.Clone()).ToList();
            if (dlg.ShowDialog() == true) {
                Model.Load.Gains.Where(e => e.Type != GainType.none).ForEach(old => old.Assign(dlg.ViewModel.First(@new => @new.Type == old.Type)));
                Model.Load.GainsChanged();
            }
        }

        private void btnSendSmsPickup_OnClick(object sender, RoutedEventArgs args) {
            var dialog = new SendSmsDialog(StopType.Pickup, Model.Load, Model.Root.ActualCompanies.First(e => e.EntityId == Model.Load.ActualCompanyId));
            dialog.ShowDialog();
        }

        private void btnSendSmsDelivery_OnClick(object sender, RoutedEventArgs args) {
            var dialog = new SendSmsDialog(StopType.Delivery, Model.Load, Model.Root.ActualCompanies.First(e => e.EntityId == Model.Load.ActualCompanyId));
            dialog.ShowDialog();
        }

        /// <summary>
        ///     Обновление DAT rate. Мёртвый код V1: сервер V1 Dispatch/GetHybridRate всегда бросал «DISABLED», пользователь видел это
        ///     сообщение; DatRateDataService на сервер V3 не перенесён, IApiService/RateConverter/DatPowerRateViewModel — модуль AutoCall.
        ///     Поведение V1 сохранено: DatRate сбрасывается, показывается то же сообщение </summary>
        private async void RefreshDatRate_OnClicked(object sender, RoutedEventArgs e) {
            //var _apiService = App.Services.GetRequiredService<IApiService>();
            //((Button)sender).IsEnabled = false;
            Model.Load.DatRate = null;
            //var request = AutoCall.WPF.Converters.DatPower.Rate.RateConverter.Convert(Model);
            //var result = await Task.Factory.StartNew(() => _apiService.GetHybridRate(request));
            //((Button)sender).IsEnabled = true;

            //if (result.IsSuccessed) {
            //    var rateModel = new DatPowerRateViewModel() { Data = new DatPowerRateDataViewModel() };
            //    var model = rateModel.Data;
            //    Converters.DatPower.Rate.RateConverter.Convert(result, model);
            //    model.Calc();
            //    Model.Load.DatRate = (decimal?)(model.ShowRates ? model.SpotEstimatedLinehaulRateFuel * model.RateMiles : model.SpotEstimatedLinehaulTotal);
            //} else {
                MessageBox.Show("DISABLED");
            //}
        }

        #region [ Drag & Drop ]

        private bool _checkFileDrag(DragEventArgs e) {
            return e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        #region [ Confirmation ]

        private void Confirmation_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                confirmationDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void Confirmation_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                confirmationDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Confirmation_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) {
                    MessageBoxEx.Error("Please drop one file");
                    return;
                }
                Model.Load.ConfirmationFile = Path.GetFileName(files[0]);
                Model.Load.ConfirmationDate = DateTime.UtcNow;
                filesControl.FileSave(files[0]);
                confirmationDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void ParseConfirmation_OnClick(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(this.Model.Load.ConfirmationFileView) == false) {
                this.IsEnabled = false;
                this.parseConfBusy.Visibility = Visibility.Visible;
                this.parseConfButton.Visibility = Visibility.Collapsed;
                this._dispatchService.FileLoad(new FileViewModel() {
                    EntityId = (Guid)this.Model.Load.ConfirmationEntityIdView,
                    Name = this.Model.Load.ConfirmationFileView
                }, async (r) => {
                    await Dispatcher.InvokeAsync(async () => {
                        if (r.Status == ResponseStatus.Success) {
                            var service = App.Services.GetRequiredService<HelperDataService>();
                            var board = (DispatchBoardType)((int)this.Model.Load.BoardType % 2);
                            ExtractLoadFromPdf2Response response = await service.ExtractLoadFromPdfAsync(r.Data.Name, r.Data.Data, board);
                            this.IsEnabled = true;
                            this.parseConfBusy.Visibility = Visibility.Collapsed;
                            this.parseConfButton.Visibility = Visibility.Visible;
                            if (response.IsSuccessed) {
                                LoadViewModel viewModel = LoadConverter.ToVm(response.Data);
                                var manager = App.Services.GetRequiredService<MainManager>();
                                var previewDlg = new EditLoadDialog(_dispatchService, manager.GetCallService, _mapper);
                                // generates defaults
                                var newViewModel = new LoadViewModel();
                                newViewModel.Assign(viewModel);
                                previewDlg.Model = new EditBookLoadViewModel(newViewModel, DispatchBoardViewModel.Instance.ActualCompanies, this.Model.Load.ActualCompany);
                                previewDlg.IsReadOnly = true;
                                previewDlg.ShowDialog();
                            } else {
                                MessageBoxEx.Error(response.ErrorMessage);
                            }
                        } else {
                            this.IsEnabled = true;
                            this.parseConfBusy.Visibility = Visibility.Collapsed;
                            this.parseConfButton.Visibility = Visibility.Visible;
                            MessageBoxEx.Error(r.MessageError);
                        }
                    });
                });
            }
        }

        #endregion [ Confirmation ]

        #region [ BOL ]

        private void BOL_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                bolDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void BOL_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                bolDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void BOL_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) {
                    MessageBoxEx.Error("Please drop one file");
                    return;
                }
                Model.Load.BOLFile = Path.GetFileName(files[0]);
                Model.Load.BOLDate = DateTime.UtcNow;
                filesControl.FileSave(files[0]);
                bolDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion [ BOL ]

        #region [ POD ]

        private void POD_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                podDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void POD_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                podDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void POD_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) {
                    MessageBoxEx.Error("Please drop one file");
                    return;
                }
                Model.Load.PODFile = Path.GetFileName(files[0]);
                Model.Load.PODDate = DateTime.UtcNow;
                filesControl.FileSave(files[0]);
                podDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion [ POD ]

        #region [ Photo ]

        private void Photo_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                photoDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void Photo_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                photoDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Photo_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) {
                    MessageBoxEx.Error("Please drop one file");
                    return;
                }
                Model.Load.PhotoFile = Path.GetFileName(files[0]);
                filesControl.FileSave(files[0]);
                photoDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion [ Photo ]

        #region [ Lumper ]

        private void Lumper_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                lumperDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void Lumper_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                lumperDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Lumper_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) {
                    MessageBoxEx.Error("Please drop one file");
                    return;
                }
                Model.Load.LumperFile = Path.GetFileName(files[0]);
                Model.Load.LumperDate = DateTime.UtcNow;
                filesControl.FileSave(files[0]);
                lumperDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion [ Lumper ]

        #region [ Paperwork ]

        private void Paperwork_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                paperworkDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void Paperwork_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                paperworkDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Paperwork_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) {
                    MessageBoxEx.Error("Please drop one file");
                    return;
                }
                Model.Load.PaperworkFile = Path.GetFileName(files[0]);
                Model.Load.PaperworkDate = DateTime.UtcNow;
                filesControl.FileSave(files[0]);
                paperworkDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion [ Paperwork ]

        #region [ File ]

        private void File_DragEnter(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                fileDragGrid.Visibility = Visibility.Visible;
            }
        }

        private void File_DragLeave(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                fileDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void File_Drop(object sender, DragEventArgs e) {
            if (_checkFileDrag(e)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files) {
                    filesControl.FileSave(file);
                }
                fileDragGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void PreviewImages_Click(object sender, RoutedEventArgs e) {
            _dispatchService.FilesLoad(this.Model.Load.EntityId, true, async viewModelWrapper => {
                await Dispatcher.InvokeAsync(() => {
                    var carusel = new RadCaruselWindow(this._dispatchService);
                    foreach (var f in viewModelWrapper.Data) {
                        f.DataImage = ImageHelper.ToImage(f.Data);
                    }
                    if (carusel.SetImageItemsSource(viewModelWrapper.Data)) {
                        carusel.ShowDialog();
                    } else {
                        MessageBoxEx.Warning("Pictures were not found.");
                    }
                });
            });
        }

        #endregion [ File ]

        #endregion [ Drag & Drop ]

        #region [ Scan OnClick ]

        private void ScanConfirmation_OnClick(object sender, RoutedEventArgs e) {
            var data = ScanAndSave();
            if (data != null) {
                var fileName = "confirmation.pdf";
                Model.Load.ConfirmationFile = fileName;
                Model.Load.ConfirmationDate = DateTime.UtcNow;
                filesControl.FileSave(fileName, data);
            }
        }

        private void ScanBOL_OnClick(object sender, RoutedEventArgs e) {
            var data = ScanAndSave();
            if (data != null) {
                var fileName = "bol.pdf";
                Model.Load.BOLFile = fileName;
                Model.Load.BOLDate = DateTime.UtcNow;
                filesControl.FileSave(fileName, data);
            }
        }

        private void ScanPOD_OnClick(object sender, RoutedEventArgs e) {
            var data = ScanAndSave();
            if (data != null) {
                var fileName = "pod.pdf";
                Model.Load.PODFile = fileName;
                Model.Load.PODDate = DateTime.UtcNow;
                filesControl.FileSave(fileName, data);
            }
        }

        private void ScanLumper_OnClick(object sender, RoutedEventArgs e) {
            var data = ScanAndSave();
            if (data != null) {
                var fileName = "lumper.pdf";
                Model.Load.LumperFile = fileName;
                Model.Load.LumperDate = DateTime.UtcNow;
                filesControl.FileSave(fileName, data);
            }
        }

        private byte[] ScanAndSave() {
            var dlg = new ScannerDialog();
            return dlg.ShowDialog() == true ? dlg.GeneratePdf() : null;
        }

        #endregion [ Scan OnClick ]

        private void EditImagePOD_OnClick(object sender, RoutedEventArgs args) {
            _errorTextBlock.Visibility = Visibility.Collapsed;
            _dispatchService.FileLoad(new FileViewModel() {
                EntityId = Model.Load.EntityId,
                Name = Model.Load.PODFile
            }, async w => {
                await Dispatcher.InvokeAsync(() => {
                    if (w.Status == ResponseStatus.Success) {
                        try {
                            var pngImages = PdfConverter.ToImages(w.Data.Data);
                            var path = Path.Combine(Path.GetTempPath(), "AutoCall Documents");
                            if (Directory.Exists(path) == false) {
                                Directory.CreateDirectory(path);
                            } else {
                                try {
                                    DirectoryInfo di = new DirectoryInfo(path);
                                    foreach (FileInfo file in di.GetFiles()) file.Delete();
                                    foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
                                } finally {
                                }
                            }
                            var flag = false;
                            foreach (var img in pngImages) {
                                string fileName = $"{Guid.NewGuid()}.png";
                                File.WriteAllBytes(Path.Combine(path, fileName), img);
                                if (flag == false) {
                                    Process.Start("Explorer", $"/select, \"{Path.Combine(path, fileName)}\"");
                                    flag = true;
                                }
                            }
                        } catch (Exception e) {
                            _errorTextBlock.Visibility = Visibility.Visible;
                            _errorTextBlock.Text = e.Message;
                        }
                    } else {
                        _errorTextBlock.Visibility = Visibility.Visible;
                        _errorTextBlock.Text = w.MessageError;
                    }
                });
            });
        }
    }
}
