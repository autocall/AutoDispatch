using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Object" -> "Object" </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class CustomCompareConverter : IValueConverter {

        public object TrueValue { get; set; }
        public object FalseValue { get; set; }
        public object TrueResult { get; set; }
        public object FalseResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.Equals(TrueValue)) {
                return TrueResult;
            }
            if (value.Equals(FalseValue)) {
                return FalseResult;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null)
                return DependencyProperty.UnsetValue;

            return parameter;
        }
    }
}