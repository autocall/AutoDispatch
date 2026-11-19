using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AutoCall.WPF.Helpers {
    public class ImageHelper {
        public static bool IsImage(string fileName) {
            return fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg");
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
