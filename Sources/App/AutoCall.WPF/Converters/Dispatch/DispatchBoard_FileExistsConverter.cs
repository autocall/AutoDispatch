using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCall.WPF.Converters.Dispatch {
    public enum FileExistsConverterParameter {
        Background,
        Foreground,
        FontWeight,
    }
    public class DispatchBoard_DefectConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((FileExistsConverterParameter)parameter) {
                case FileExistsConverterParameter.Background:
                    return !(bool)value ? Brushes.Transparent : Brushes.Red;
                case FileExistsConverterParameter.Foreground:
                    return !(bool)value ? Brushes.Black : Brushes.White;
                case FileExistsConverterParameter.FontWeight:
                    return !(bool)value ? FontWeights.Normal : FontWeights.Bold;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
