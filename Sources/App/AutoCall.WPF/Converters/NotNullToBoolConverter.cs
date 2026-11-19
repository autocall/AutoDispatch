using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Converter. Isset object </summary>
    [ValueConversion(sourceType: typeof (object), targetType: typeof (bool))]
    public class NotNullToBoolConverter : IValueConverter {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (Invert == false)
                return value != null;
            else
                return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}