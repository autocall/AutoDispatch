using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Convert Boolean -> Custom object </summary>
    [ValueConversion(sourceType: typeof (bool), targetType: typeof (object))]
    public class BooleanToObjectConverter : IValueConverter {
        public object True { get; set; }
        public object False { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((bool) value) ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}