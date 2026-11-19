using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class ExpensePaidDateToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var dt = value as DateTime?;
            if(dt != null) {
                return $"Paid in {dt:d}";
            } else {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
