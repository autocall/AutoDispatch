using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.ToUri {
    public class CarrierViewStatusToUriConverter : IValueConverter {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((string)value) {
                case "active":
                    return DependencyProperty.UnsetValue;
                case "active_on_time":
                    return DependencyProperty.UnsetValue;
                case "active_late":
                    return DependencyProperty.UnsetValue;
                case "past":
                    return DependencyProperty.UnsetValue;
                case "tracking_successful":
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/CarrierView/tracking_successful.svg");
                case "tracking_interrupted":
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/CarrierView/tracking_interrupted.svg");
                default:
                    return DependencyProperty.UnsetValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
