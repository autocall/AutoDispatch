using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    /// <summary>
    ///     Represents converter "Object" -> "Object" </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public class MenuConverter : IValueConverter {

        public object Carrier_PatternsMenu { get; set; }
        public object Carrier_RecordsMenu { get; set; }
        public object Broker_PatternsMenu { get; set; }
        public object Broker_RecordsMenu { get; set; }

        public object Sub_Carrier_PatternsMenu { get; set; }
        public object Sub_Carrier_RecordsMenu { get; set; }
        public object Sub_Broker_PatternsMenu { get; set; }
        public object Sub_Broker_RecordsMenu { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return DependencyProperty.UnsetValue;
            var w = (Wrapper1LoadViewModel)value;
            if (w == null) return DependencyProperty.UnsetValue;
            DispatchFilterLoadType filterType = DispatchBoardBase.Static_ViewSource_OnFilter(w);
            switch (filterType) {

                case DispatchFilterLoadType.Carrier_Patterns:
                    return Carrier_PatternsMenu;
                case DispatchFilterLoadType.Carrier_Records:
                    return Carrier_RecordsMenu;

                case DispatchFilterLoadType.Broker_Patterns:
                    return Broker_PatternsMenu;
                case DispatchFilterLoadType.Broker_Records:
                    return Broker_RecordsMenu;

                case DispatchFilterLoadType.Sub_Carrier_Patterns:
                    return Sub_Carrier_PatternsMenu;
                case DispatchFilterLoadType.Sub_Carrier_Records:
                    return Sub_Carrier_RecordsMenu;

                case DispatchFilterLoadType.Sub_Broker_Patterns:
                    return Sub_Broker_PatternsMenu;
                case DispatchFilterLoadType.Sub_Broker_Records:
                    return Sub_Broker_RecordsMenu;

                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}