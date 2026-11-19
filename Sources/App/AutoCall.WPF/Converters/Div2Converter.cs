using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class Div2Converter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (double)value / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
