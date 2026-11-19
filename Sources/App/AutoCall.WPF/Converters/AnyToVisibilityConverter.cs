using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class AnyToVisibilityConverter : IValueConverter {

        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (Inverted == false) {
                if (value is IList) {
                    return ((IList)value).Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                if (value is string) {
                    return ((string)value).Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            } else {
                if (value is IList) {
                    return ((IList)value).Count > 0 ? Visibility.Collapsed : Visibility.Visible;
                }
                if (value is string) {
                    return ((string)value).Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
