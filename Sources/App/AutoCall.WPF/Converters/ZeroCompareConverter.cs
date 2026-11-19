using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Number" -> "string" </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class ZeroCompareConverter : IValueConverter {

        public string TrueResult { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value.Equals(0) || value.Equals(0.0) || value.Equals(0f)) {
                return TrueResult;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}