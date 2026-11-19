using System;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Record age" -> "String in format "HH:mm" </summary>
    public class RecordAgeToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            TimeSpan age = (TimeSpan) value;
            if(age.TotalHours > 1) {
                return $"{(int)age.TotalHours:00}:{age.Minutes:00}:{age.Seconds:00}";
            } else {
                return $"{age.Minutes:00}:{age.Seconds:00}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}