using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "string|int" -> "bool" </summary>
    public class StringIntToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return false;

            if (value is string) {
                string strValue = (string) value;
                if (string.IsNullOrEmpty(strValue))
                    return false;
                else
                    return true;
            }

            if (value is int) {
                int intValue = (int) value;
                if (intValue < 1)
                    return false;
                else
                    return true;
            }

            throw new Exception(string.Format("Can not convert '{0} to 'bool''", value.GetType().Name));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}