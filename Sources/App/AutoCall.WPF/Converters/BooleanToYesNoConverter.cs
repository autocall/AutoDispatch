using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Convert Boolean -> "Yes" or "No" </summary>
    [ValueConversion(sourceType: typeof (bool), targetType: typeof (string))]
    public class BooleanToYesNoConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}