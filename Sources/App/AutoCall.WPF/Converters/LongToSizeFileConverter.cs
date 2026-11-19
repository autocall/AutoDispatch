using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class LongToSizeFileConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = (long)value;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length) {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
