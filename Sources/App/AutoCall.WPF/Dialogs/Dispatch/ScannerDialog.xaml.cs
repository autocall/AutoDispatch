using AutoCall.Common.Helpers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using Telerik.Windows.Documents.Model;
using TwainDotNet;
using TwainDotNet.Win32;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class ScannerDialog : Window {

        private Twain _twain;

        public ScannerViewModel Model {
            get { return DataContext as ScannerViewModel; }
            set { DataContext = value; }
        }

        public ScannerDialog(List<byte[]> dataList) { }

        private SettingsService _settings => App.Services.GetRequiredService<SettingsService>();

        public ScannerDialog() {
            InitializeComponent();

            Model = new ScannerViewModel() {
                ADF = _settings.GetScanADF(),
                BW = _settings.GetScanBW()
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _twain = new Twain(new WpfWindowMessageHook(this));
            _twain.TransferImage += _twain_TransferImage;
            _twain.ScanningComplete += _twain_ScanningComplete;

            IList<string> sourceList = _twain.SourceNames;
            Model.ScannerItemSource = sourceList;
            if (sourceList.Any()) {
            var defaultScanner = _settings.GetDefaultScanner();
                Model.SelectedScanner = sourceList.Contains(defaultScanner) ? defaultScanner : sourceList.First();
            }
        }

        private void _twain_ScanningComplete(object sender, ScanningCompleteEventArgs e) {
            Model.Processing = false;

            Debug.WriteLine("_twain_ScanningComplete");
        }

        private void _twain_TransferImage(object sender, TransferImageEventArgs args) {
            if (args.Image != null) {
                IntPtr hbitmap = new System.Drawing.Bitmap(args.Image).GetHbitmap();
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    hbitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                Gdi32Native.DeleteObject(hbitmap);

                Model.Images.Add(bitmap);

                Model.Images_PropertyChanged();

                if (Model.SelectedImage == null) {
                    Model.SelectedImage = bitmap;
                }

                Debug.WriteLine("_twain_TransferImage");
            }
        }

        private async void scanButton_Click(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(Model.SelectedScanner)) {
                MessageBoxEx.Error("Please select 'Source'");
                return;
            }

            Model.Images.Clear();

            Model.Processing = true;

            await Task.Delay(100);

            var settings = new ScanSettings {
                UseDocumentFeeder = Model.ADF,
                ShowTwainUI = false,
                ShowProgressIndicatorUI = false,
                UseDuplex = false,
                Resolution = Model.BW ? ResolutionSettings.Fax : ResolutionSettings.ColourPhotocopier,
                Area = null,
                ShouldTransferAllPages = true,
                Rotation = new RotationSettings {
                    AutomaticRotate = false,
                    AutomaticBorderDetection = false
                }
            };

            try {
                _twain.SelectSource(Model.SelectedScanner);
                _twain.StartScanning(settings);
            } catch (TwainException ex) {
                MessageBoxEx.Error(ex.Message);
                Model.ErrorMessage = ex.Message;
                Dispatcher.Invoke(() => Model.Processing = false);
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;

            _settings.SetScanADF(this.Model.ADF);
            _settings.SetScanBW(this.Model.BW);
            _settings.SetDefaultScanner(this.Model.SelectedScanner);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        public byte[] ImageToBinary(BitmapSource bitmap) {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 30;
            using (MemoryStream stream = new MemoryStream()) {
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();
                return bit;
            }
        }

        public byte[] GeneratePdf() {
            if (Model.Images.Any() == false) return null;
            var width = (int)Model.Images.First().Width;
            var height = (int)Model.Images.First().Height;

            var binaries = Model.Images.Select(e => this.ImageToBinary(e)).ToList();

            RadDocument document = new RadDocument();
            document.LayoutMode = DocumentLayoutMode.Paged;
            document.Measure(RadDocument.MAX_DOCUMENT_SIZE);
            document.Arrange(new RectangleF(PointF.Empty, document.DesiredSize));
            document.SectionDefaultPageMargin = new Telerik.Windows.Documents.Layout.Padding(0, 0, 0, 0);
            var imageWidth = width;
            var imageHeight = height;
            if (imageWidth > document.DesiredSize.Width) {
                imageWidth = (int)document.DesiredSize.Width;
                imageHeight = height * imageWidth / width;
            }

            document.Sections.Clear();

            foreach (var binary in binaries) {
                Section section = new Section();
                Paragraph paragraph = new Paragraph();
                ImageInline imageInline = new ImageInline(new MemoryStream(binary), new Size(imageWidth, imageHeight), "jpg");
                paragraph.Inlines.Add(imageInline);
                section.Blocks.Add(paragraph);
                document.Sections.Add(section);
            }


            PdfFormatProvider provider = new PdfFormatProvider();
            provider.ExportSettings.ImagesCompressionMode = PdfImagesCompressionMode.Jpeg;
            provider.ExportSettings.ImagesDeflaterCompressionLevel = 9;
            provider.ExportSettings.ContentsDeflaterCompressionLevel = 9;
            using (Stream output = new MemoryStream()) {
                provider.Export(document, output);
                output.Position = 0;
                return BinaryHelper.ReadFully(output);
            }
        }
    }
}
