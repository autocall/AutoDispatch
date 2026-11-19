using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.ToUri {
    public class CarrierViewStatusToVisibilityConverter : CarrierViewStatusToUriConverter {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var prp = base.Convert(value, targetType, parameter, culture);
            return prp != DependencyProperty.UnsetValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
