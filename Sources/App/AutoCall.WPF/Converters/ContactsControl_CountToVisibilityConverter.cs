using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class IntToVisibilityConverter : IValueConverter {

        public int Value { get; set; }
        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!Inverted) {
                return (int)value == Value ? Visibility.Visible : Visibility.Collapsed;
            } else {
                return (int)value == Value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
