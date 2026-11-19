using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Worker state" -> "Enabling" </summary>
    public class MultiBoolConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var result = values.Aggregate((v1, v2) => v1 is bool && v2 is bool && (bool)v1 && (bool)v2);
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}