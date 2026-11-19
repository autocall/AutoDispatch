//using AutoCall.Common.Helpers;
//using AutoCall.Logic.Helpers;
//using AutoCall.WPF.Dialogs.Dispatch;
//using AutoCall.WPF.Extensions;
//using AutoDispatch.Logic.ViewModels.Pages;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Windows;
//using Telerik.Windows.Documents.FormatProviders.Pdf;
//using Telerik.Windows.Documents.Model;
//using WIA;

//namespace AutoCall.WPF.Helpers {
//    public static class ScanHelper {
//        public static byte[] JpgScan() {
//            try {
//                var scanDialog = new WIA.CommonDialog();

//                ImageFile image = scanDialog.ShowAcquireImage(
//                        WiaDeviceType.ScannerDeviceType,
//                        WiaImageIntent.ColorIntent,
//                        WiaImageBias.MinimizeSize,
//                        WIA.FormatID.wiaFormatJPEG,
//                        false,
//                        true,
//                        false);

//                var binary = image.FileData.get_BinaryData() as byte[];
//                // bmp to jpg
//                byte[] jpegBytes = JpgHelper.ConvertBytestoJpegBytes(binary, image.Width, image.Height);
//                return jpegBytes;
//            } catch (COMException ex) {
//                if (ex.ErrorCode == -2145320939) {
//                    MessageBoxEx.Warning("Scanner not found");
//                    return null;
//                } else {
//                    throw;
//                }
//            }
//        }

//        public static byte[] PdfScan() {
////#if DEBUG
////            return File.ReadAllBytes(@"D:\Develop\AutoCall\Sources\Web\Web\Documents\fw9.pdf");
////#endif
//            var images = AdfScan();
//            if (images == null || images.Any() == false) return null;

//            var imgBytes = ToBytes(images);
//            ImageEditorDialog dlg = new ImageEditorDialog(imgBytes);
//            if (dlg.ShowDialog() == false) {
//                return null;
//            }

//            #region [ convert images to pdf ]

//            RadDocument document = new RadDocument();
//            document.LayoutMode = DocumentLayoutMode.Paged;
//            document.Measure(RadDocument.MAX_DOCUMENT_SIZE);
//            document.Arrange(new RectangleF(PointF.Empty, document.DesiredSize));
//            document.SectionDefaultPageMargin = new Telerik.Windows.Documents.Layout.Padding(0, 0, 0, 0);
//            var imageWidth = images[0].Width;
//            var imageHeight = images[0].Height;
//            if (imageWidth > document.DesiredSize.Width) {
//                imageWidth = (int)document.DesiredSize.Width;
//                imageHeight = images[0].Height * imageWidth / images[0].Width;
//            }

//            document.Sections.Clear();

//            foreach (var image in images) {
//                Section section = new Section();
//                Paragraph paragraph = new Paragraph();
//                var jpgBinary = JpgHelper.ConvertBytestoJpegBytes(image.FileData.get_BinaryData() as byte[], image.Width, image.Height);
//                ImageInline imageInline = new ImageInline(new MemoryStream(jpgBinary), new Size(imageWidth, imageHeight), "jpg");
//                paragraph.Inlines.Add(imageInline);
//                section.Blocks.Add(paragraph);
//                document.Sections.Add(section);
//            }


//            PdfFormatProvider provider = new PdfFormatProvider();
//            provider.ExportSettings.ImagesCompressionMode = PdfImagesCompressionMode.Jpeg;
//            provider.ExportSettings.ImagesDeflaterCompressionLevel = 9;
//            provider.ExportSettings.ContentsDeflaterCompressionLevel = 9;
//            using (Stream output = new MemoryStream()) {
//                provider.Export(document, output);
//                output.Position = 0;
//                return BinaryHelper.ReadFully(output);
//            }

//            #endregion [ convert images to pdf ]
//        }

//        private static List<byte[]> ToBytes(List<ImageFile> images) {
//            return images.Select(e => (byte[])e.FileData.get_BinaryData()).ToList();
//        }

//        private static List<ImageFile> AdfScan() {
//            var scanDialog = new WIA.CommonDialog();
//            Device d = scanDialog.ShowSelectDevice(WiaDeviceType.UnspecifiedDeviceType, true, false);
//            string deviceId;
//            if (d != null) {
//                deviceId = d.DeviceID;
//            } else {
//                //no scanner chosen
//                return null;
//            }

//            List<ImageFile> result = new List<ImageFile>();

//            bool hasMorePages = true;
//            int x = 0;
//            while (hasMorePages) {
//                //Create DeviceManager
//                DeviceManager manager = new DeviceManager();
//                Device WiaDev = null;
//                foreach (DeviceInfo info in manager.DeviceInfos) {
//                    if (info.DeviceID == deviceId) {
//                        WIA.Properties infoprop = null;
//                        infoprop = info.Properties;

//                        //connect to scanner
//                        WiaDev = info.Connect();

//                        break;
//                    }
//                }

//                var Items = scanDialog.ShowSelectItems(WiaDev, WiaImageIntent.GrayscaleIntent, WiaImageBias.MinimizeSize, false, true);
//                //Start Scan

//                WIA.Item Item = Items[1] as WIA.Item;

//                try {
//                    var img = (ImageFile)scanDialog.ShowTransfer(Item, wiaFormatJPEG, false);
//                    result.Add(img);

//                } catch (COMException ex) {
//                    if (ex.ErrorCode == -2145320939) {
//                        MessageBoxEx.Warning("Scanner not found");
//                        return null;
//                    } else {
//                        throw;
//                    }
//                } finally {
//                    Item = null;
//                    //determine if there are any more pages waiting
//                    Property documentHandlingSelect = null;
//                    Property documentHandlingStatus = null;
//                    foreach (Property prop in WiaDev.Properties) {
//                        if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_SELECT)
//                            documentHandlingSelect = prop;
//                        if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_STATUS)
//                            documentHandlingStatus = prop;
//                    }

//                    hasMorePages = false; //assume there are no more pages
//                    if (documentHandlingSelect != null)
//                    //may not exist on flatbed scanner but required for feeder
//                    {
//                        //check for document feeder
//                        if ((Convert.ToUInt32(documentHandlingSelect.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_SELECT.FEEDER) != 0) {
//                            hasMorePages = ((Convert.ToUInt32(documentHandlingStatus.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_STATUS.FEED_READY) != 0);
//                        }
//                    }
//                    x++;
//                }
//            }
//            return result;
//        }

//        const string wiaFormatBMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
//        const string wiaFormatPNG = "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}";
//        const string wiaFormatGIF = "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}";
//        const string wiaFormatJPEG = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";
//        const string wiaFormatTIFF = "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}";

//        class WIA_DPS_DOCUMENT_HANDLING_SELECT {
//            public const uint FEEDER = 0x00000001;
//            public const uint FLATBED = 0x00000002;
//        }

//        class WIA_DPS_DOCUMENT_HANDLING_STATUS {
//            public const uint FEED_READY = 0x00000001;
//        }

//        class WIA_PROPERTIES {
//            public const uint WIA_RESERVED_FOR_NEW_PROPS = 1024;
//            public const uint WIA_DIP_FIRST = 2;
//            public const uint WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
//            public const uint WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
//            //
//            // Scanner only device properties (DPS)
//            //
//            public const uint WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
//            public const uint WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13;
//            public const uint WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14;
//        }

//        class WIA_ERRORS {
//            public const uint BASE_VAL_WIA_ERROR = 0x80210000;
//            public const uint WIA_ERROR_PAPER_EMPTY = BASE_VAL_WIA_ERROR + 3;
//        }
//    }
//}
