using System;
using System.Globalization;
using System.Windows.Data;
using AutoCall.Common.Extensions;

namespace AutoCall.WPF.Converters {
    [ValueConversion(typeof (Enum), typeof (string))]
    public class EnumToDescriptionConverter : IValueConverter {
        public bool AllowToString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (AllowToString == false) {
                return ((Enum) value).GetDescription();
            } else {
                return ((Enum)value).GetDescription() ?? value?.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}