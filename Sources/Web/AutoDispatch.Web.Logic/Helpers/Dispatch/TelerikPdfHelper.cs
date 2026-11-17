using System.Collections.Concurrent;
using SkiaSharp;
using Telerik.Documents.Core.Fonts;
using Telerik.Windows.Documents.Core.Fonts;
using Telerik.Windows.Documents.Extensibility;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.FormatProviders.Rtf;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     RTF→PDF на Telerik Document Processing (.NET Standard, кросс-платформенный) вместо WPF RadDocument из V1.
///     В .NET Standard Telerik не читает системные шрифты и не кодирует PNG — шрифты отдаются из Resources/Fonts
///     (иначе подмена на стандартные PDF-шрифты), картинки перекодируются в JPEG через SkiaSharp </summary>
public static class TelerikPdfHelper {

    static TelerikPdfHelper() {
        FixedExtensibilityManager.FontsProvider = new FontsProvider();
        FixedExtensibilityManager.JpegImageConverter = new JpegImageConverter();
    }

    private static readonly object ExportLock = new();

    /// <summary>
    ///     Под замком: Telerik регистрирует шрифты в статическом FontsRepository (обычный Dictionary) — параллельные экспорты
    ///     рушат его («concurrent update ... corrupted its state», воспроизведено в Linux-контейнере 2026-11-16) </summary>
    public static byte[] RtfToPdf(string rtf) {
        lock (ExportLock) {
            var document = new RtfFormatProvider().Import(rtf);
            return new PdfFormatProvider().Export(document);
        }
    }

    /// <summary>
    ///     Шрифты, которые реально запрашивает Telerik на шаблонах setup packet (000102040506071314*, 05_02): текст — Calibri,
    ///     маркеры списков — Arial «•», Courier New «o», Wingdings «▪», Symbol «•» (стандартный PDF-шрифт, файл не нужен). Шрифты Microsoft в репозиторий не кладём (лицензия) —
    ///     метрически совместимые OFL: Calibri → Carlito, Arial → Liberation Sans, Courier New → Liberation Mono.
    ///     У Wingdings свободной замены нет: файл ищется в Resources/Fonts, затем в FilesPath/_fonts на хосте; без него маркер третьего
    ///     уровня списка пустой. Остальные запросы — null, Telerik подставляет стандартный PDF-шрифт </summary>
    private class FontsProvider : FontsProviderBase {

        private static readonly Dictionary<string, string> Files = new() {
            { "Calibri", "Carlito-Regular.ttf" }, { "Calibri Bold", "Carlito-Bold.ttf" }, { "Calibri Italic", "Carlito-Italic.ttf" }, { "Calibri Bold Italic", "Carlito-BoldItalic.ttf" },
            { "Arial", "LiberationSans-Regular.ttf" }, { "Courier New", "LiberationMono-Regular.ttf" },
            { "Brush Script MT", "DancingScript-Bold.ttf" }, { "Wingdings", "wingding.ttf" },
        };
        private static readonly ConcurrentDictionary<string, byte[]> Cache = new();

        public override byte[] GetFontData(FontProperties properties) {
            var key = properties.FontFamilyName
                + (properties.FontWeight == FontWeights.Bold ? " Bold" : "")
                + (properties.FontStyle == FontStyles.Italic ? " Italic" : "");
            return Files.TryGetValue(key, out var file) ? Cache.GetOrAdd(file, Read) : null;
        }

        private static byte[] Read(string file) {
            foreach (var dir in new[] { AppSettings.FontsPath, AppSettings.HostFontsPath }) {
                var path = Path.Combine(dir, file);
                if (File.Exists(path)) {
                    return File.ReadAllBytes(path);
                }
            }
            return null;
        }
    }

    private class JpegImageConverter : JpegImageConverterBase {

        public override bool TryConvertToJpegImageData(byte[] imageData, ImageQuality imageQuality, out byte[] jpegImageData) {
            jpegImageData = null;
            try {
                using var bitmap = SKBitmap.Decode(imageData);
                if (bitmap == null) {
                    return false;
                }
                using var opaque = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
                using (var canvas = new SKCanvas(opaque)) {
                    canvas.Clear(SKColors.White); // прозрачность PNG в JPEG иначе станет чёрной
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
                using var image = SKImage.FromBitmap(opaque);
                using var jpeg = image.Encode(SKEncodedImageFormat.Jpeg, 90);
                jpegImageData = jpeg.ToArray();
                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}
