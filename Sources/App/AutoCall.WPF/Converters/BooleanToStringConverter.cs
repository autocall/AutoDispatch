using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Convert Boolean -> String </summary>
    [ValueConversion(sourceType: typeof (bool), targetType: typeof (string))]
    public class BooleanToStringConverter : IValueConverter {
        public String TrueString { get; set; }
        public String FalseString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool) value ? TrueString : FalseString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}