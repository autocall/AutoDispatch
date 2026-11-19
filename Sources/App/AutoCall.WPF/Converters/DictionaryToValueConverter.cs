using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class DictionaryToValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                var dic = value as IDictionary;
                return dic?[parameter];
            } catch (Exception) {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
