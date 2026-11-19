using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AutoDispatch.Dtos.Enums;

namespace AutoCall.WPF.Converters.Dispatch {
    public class BookLoadDialog_BorderTypeToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || Enum.IsDefined(value.GetType(), parameter) == false)
                return DependencyProperty.UnsetValue;

            switch ((DispatchBoardType)value) {
                case DispatchBoardType.Broker:
                case DispatchBoardType.BrokerAccount:
                    return DispatchBoardType.Broker.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
                case DispatchBoardType.Carrier:
                case DispatchBoardType.CarrierAccount:
                    return DispatchBoardType.Carrier.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
