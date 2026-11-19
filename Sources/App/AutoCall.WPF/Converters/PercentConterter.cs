using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class PercentConterter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double) {
                return System.Convert.ToInt32((double)value * 100);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int) {
                return System.Convert.ToDouble(value) / 100;
            }
            return null;
        }
    }
}
