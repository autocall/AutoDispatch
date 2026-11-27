using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AutoCall.WPF.Helpers {
    public class ImageHelper {
        /// <summary>
        ///     Без учёта регистра, как сервер (аудит C5): .PNG/.JPG — тоже картинки; иначе Preview Images их отбрасывал (app-testing 9.7) </summary>
        public static bool IsImage(string fileName) {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
        }

        public static BitmapImage ToImage(byte[] array) {
            try {
                using (var ms = new MemoryStream(array)) {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // here
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            } catch (Exception) {
                return null;
            }
        }
    }
}
