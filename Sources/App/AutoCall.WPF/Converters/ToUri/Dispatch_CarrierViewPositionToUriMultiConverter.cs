using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.ToUri {
    /// <summary>
    ///     Converter for DataSource icon </summary>
    public class CarrierViewPositionStatusToUriMultiConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length != 3 || values[0] is bool == false || values[1] is string == false || values[2] is List<string> == false) 
                return new Uri($"pack://application:,,,/AutoDispatch;component/Resources/CarrierView/Start_disabled.svg");
            var CarrierViewLate = (bool)values[0];
            var CarrierViewStatus = (string)values[1];
            var CarrierViewStatuses = (List<string>)values[2];
            var CarrierViewDisabled = CarrierViewStatuses != null && CarrierViewStatuses.Contains("past");
            var iconKind = CarrierViewDisabled ? "_disabled" : (CarrierViewLate ? "_late" : String.Empty);

            var status = CarrierViewStatus;
            switch (status) {
                case "stopped":
                    return new Uri($"pack://application:,,,/AutoDispatch;component/Resources/CarrierView/Stop{iconKind}.svg");
                case "paused":
                    return new Uri($"pack://application:,,,/AutoDispatch;component/Resources/CarrierView/Pause{iconKind}.svg");
                case "drive":
                    return new Uri($"pack://application:,,,/AutoDispatch;component/Resources/CarrierView/Arrow0{iconKind}.svg");
                default:
                    return new Uri($"pack://application:,,,/AutoDispatch;component/Resources/CarrierView/Start_disabled.svg");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
