using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class AnyToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is IList) {
                return ((IList)value).Count > 0;
            }
            if (value is string) {
                return ((string)value).Length > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
