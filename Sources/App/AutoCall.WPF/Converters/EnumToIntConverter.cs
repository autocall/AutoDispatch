using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class EnumToIntConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Enum.ToObject((Type)parameter, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (int)value;
        }
    }
}
