using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Object" -> "Object" </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class CustomNotNullConverter : IValueConverter {

        public object TrueResult { get; set; }
        public object FalseResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null) {
                return TrueResult;
            } else {
                return FalseResult;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null)
                return DependencyProperty.UnsetValue;

            return parameter;
        }
    }
}