using AutoCall.WPF.Extensions;

namespace AutoCall.WPF.Helpers {
    public static class FileSizeHelper {
        public static bool Check20MBSize(byte[] binary) {
            if (binary.Length <= 20 * 1024 * 1024) {
                return true;
            } else {
                MessageBoxEx.Error("Max file size 20 MB");
                return false;
            }
        }
    }
}
