using SkiaSharp;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Превью изображений в FilesLoad/FileLoad — ширина 200 (V1 ImageHelper на System.Drawing; здесь SkiaSharp, как в Safer —
///     ImageSharp 4.x требует лицензионный ключ при Release-сборке, AI/wiki/plan.md) </summary>
public static class ImageHelper {

    private const int PreviewWidth = 200;

    public static byte[] OptimizeImages(byte[] data) {
        try {
            using var bitmap = SKBitmap.Decode(data);
            if (bitmap == null) {
                return null;
            }
            var ratio = (double)PreviewWidth / bitmap.Width;
            var width = (int)(bitmap.Width * ratio);
            var height = (int)(bitmap.Height * ratio);
            using var resized = bitmap.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            using var image = SKImage.FromBitmap(resized);
            using var png = image.Encode(SKEncodedImageFormat.Png, 100);
            return png.ToArray();
        } catch (Exception) {
            return null;
        }
    }

    public static bool IsImage(string fileName) {
        return fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg");
    }
}
