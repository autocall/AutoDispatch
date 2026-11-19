using AutoCall.Common.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class IntToDescriptionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Enum)Enum.ToObject((Type)parameter, value)).GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
