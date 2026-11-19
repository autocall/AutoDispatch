using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Object" -> "Boolean" </summary>
    [ValueConversion(typeof (object), typeof (bool))]
    public class CompareConverter : IValueConverter {
        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || Enum.IsDefined(value.GetType(), parameter) == false)
                return DependencyProperty.UnsetValue;
            if (Inverted == false) {
                return parameter.Equals(value);
            } else {
                return !parameter.Equals(value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null)
                return DependencyProperty.UnsetValue;

            return parameter;
        }
    }
}