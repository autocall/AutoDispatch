using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.UI;

namespace AutoDispatch.Logic.Heplers {
    public static class PdfConverter {
        public static IEnumerable<byte[]> ToImages(byte[] pdfData) {
            ThumbnailFactory thumbnailFactory = new ThumbnailFactory();
            PdfFormatProvider formatProvider = new PdfFormatProvider();
            RadFixedDocument document = formatProvider.Import(new MemoryStream(pdfData));
            foreach (RadFixedPage page in document.Pages) {
                var imageSource = (RenderTargetBitmap)thumbnailFactory.CreateThumbnail(page, page.Size);
                byte[] pixels = GetPixels(imageSource);
                // ARGB-> RGB, A=>255
                for (int j = 0; j < pixels.Length; j += 4) {
                    pixels[j + 0] = RemoveAlpha(pixels[j + 3], pixels[j + 0], 255);
                    pixels[j + 1] = RemoveAlpha(pixels[j + 3], pixels[j + 1], 255);
                    pixels[j + 2] = RemoveAlpha(pixels[j + 3], pixels[j + 2], 255);
                    pixels[j + 3] = 255;
                }
                var writeableBitmap = new WriteableBitmap(imageSource);
                PutPixels(writeableBitmap, pixels);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                BitmapFrame frame = BitmapFrame.Create(writeableBitmap);
                encoder.Frames.Add(frame);
                using (var memoryStream = new MemoryStream()) {
                    encoder.Save(memoryStream);
                    yield return memoryStream.ToArray();
                }
            }
        }

        private static byte RemoveAlpha(byte alphaByte, byte foreground, byte background) {
            if (alphaByte == 255)
                return foreground;

            double alpha = alphaByte / 255.0;
            double diff = 1.0 - alpha;
            double result = foreground * alpha + background * diff;
            return (byte)(result <= 255 ? result : 255);
        }
        private static byte[] GetPixels(BitmapSource source) {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            byte[] result = new byte[width * height * 4];
            source.CopyPixels(result, width * 4, 0);
            return result;
        }
        private static void PutPixels(WriteableBitmap bitmap, byte[] pixels) {
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, bitmap.PixelWidth * 4, 0, 0);
        }
    }
}
