using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class IsNotNullOrEmptyStringToVisibilityConverter : IValueConverter {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !String.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
