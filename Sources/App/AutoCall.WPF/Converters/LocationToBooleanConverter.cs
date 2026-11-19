using System;
using System.Linq;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter exists ',' -> "Boolean" </summary>
    public class LocationToBooleanConverter : IValueConverter {

        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var location = value as string;
            var result = location != null && (location.Contains(",") || (location.Length > 0 && location.All(Char.IsDigit)));
            if(Inverted == false) {
                return result;
            } else {
                return !result;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}