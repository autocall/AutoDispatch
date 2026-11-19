using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AutoDispatch.Logic.Heplers {
    public struct SHFILEINFO {
        public IntPtr hIcon;

        public IntPtr iIcon;

        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    public static class FileIconLoader {
        private const uint SHGFI_ICON = 256u;

        private const uint SHGFI_LARGEICON = 0u;

        private const uint SHGFI_SMALLICON = 1u;

        private const uint SHGFI_USEFILEATTRIBUTES = 16u;

        private const uint FILE_ATTRIBUTE_NORMAL = 128u;

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public static BitmapImage GetFileIcon(string fileName, bool largeIcon) {
            string extension = Path.GetExtension(fileName);
            fileName = "*" + extension;
            SHFILEINFO sHFILEINFO = default(SHFILEINFO);
            if (largeIcon) {
                FileIconLoader.SHGetFileInfo(fileName, 128u, ref sHFILEINFO, (uint)Marshal.SizeOf(sHFILEINFO), 272u);
            } else {
                FileIconLoader.SHGetFileInfo(fileName, 128u, ref sHFILEINFO, (uint)Marshal.SizeOf(sHFILEINFO), 273u);
            }
            BitmapImage result;
            try {
                Icon icon = Icon.FromHandle(sHFILEINFO.hIcon);
                Bitmap bitmap = icon.ToBitmap();
                using (MemoryStream memoryStream = new MemoryStream()) {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0L;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    result = bitmapImage;
                }
            } catch {
                result = null;
            }
            return result;
        }
    }
}
