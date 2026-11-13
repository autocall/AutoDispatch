using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Превью изображений в FilesLoad/FileLoad — ширина 200 (V1 ImageHelper на System.Drawing; здесь ImageSharp, кросс-платформенно) </summary>
public static class ImageHelper {

    private const int PreviewWidth = 200;

    public static byte[] OptimizeImages(byte[] data) {
        try {
            using var image = Image.Load(data);
            var ratio = (double)PreviewWidth / image.Width;
            var width = (int)(image.Width * ratio);
            var height = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(width, height));
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        } catch (Exception) {
            return null;
        }
    }

    public static bool IsImage(string fileName) {
        return fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg");
    }
}
