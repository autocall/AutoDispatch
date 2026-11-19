using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class NotEmptyStringToBoolConverter : IValueConverter {

        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (Inverted) {
                return String.IsNullOrEmpty((string)value);
            } else {
                return !String.IsNullOrEmpty((string)value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
