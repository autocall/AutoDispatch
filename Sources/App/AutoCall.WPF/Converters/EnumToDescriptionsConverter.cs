using System;
using System.Globalization;
using System.Windows.Data;
using AutoCall.Common.Extensions;

namespace AutoCall.WPF.Converters {
    [ValueConversion(typeof (Enum), typeof (string))]
    public class EnumToDescriptionsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((Enum) value).GetDescriptions("/");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}