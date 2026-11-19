using AutoCall.Common.Attributes;
using AutoCall.Common.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class EnumToKeyValuesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Enum.GetValues((Type)value).Cast<Enum>().Where(e => DisplayAttribute.HasDisplay(e))
                .OrderBy(e => DisplayAttribute.GetOrder(e)).ToDictionary(e => e, e => e.GetDescription());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
