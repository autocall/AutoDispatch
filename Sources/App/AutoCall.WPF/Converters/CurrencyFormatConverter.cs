using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class CurrencyFormatConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is decimal decimalValue) {
                var symbol = culture.NumberFormat.CurrencySymbol;
                var formatted = Math.Abs(decimalValue).ToString("N2", culture);
                return (decimalValue < 0 ? "-" : "") + symbol + formatted;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string str) {
                str = str.Replace(culture.NumberFormat.CurrencySymbol, "").Trim();
                if (decimal.TryParse(str, NumberStyles.Any, culture, out decimal result))
                    return result;
            }

            return 0m;
        }
    }
}
