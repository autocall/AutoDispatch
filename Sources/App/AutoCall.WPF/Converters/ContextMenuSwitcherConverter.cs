using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    [ValueConversion(typeof(object), typeof(object))]
    public class ContextMenuSwitcherConverter : IValueConverter {

        public object BrokerMenu { get; set; }
        public object CarrierMenu { get; set; }
        public object SubLoadMenu { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var load = (LoadViewModel)value;
            if (load.Parent1 != null || load.Parent2 != null) {
                return SubLoadMenu;
            }
            if (load.BoardType == DispatchBoardType.Broker || load.BoardType == DispatchBoardType.BrokerAccount) {
                return BrokerMenu;
            }
            if (load.BoardType == DispatchBoardType.Carrier || load.BoardType == DispatchBoardType.CarrierAccount) {
                return CarrierMenu;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null)
                return DependencyProperty.UnsetValue;

            return parameter;
        }
    }
}