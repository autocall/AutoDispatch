using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AutoDispatch.Logic.Enums;

namespace AutoCall.WPF.Converters.Dispatch {
    public enum PalletTypeConverterParameter {
        Background,
        Foreground,
    }

    public class EditLoadDialog_PalletTypeToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            switch ((PalletTypeConverterParameter)parameter) {
                case PalletTypeConverterParameter.Background:
                    switch ((PalletType)value) {
                        case PalletType.Standard:
                            return DependencyProperty.UnsetValue;
                        case PalletType.Oversize:
                            return new SolidColorBrush(Colors.DarkOrange);
                        case PalletType.FloorLoaded:
                            return new SolidColorBrush(Colors.Red);
                        default:
                            return DependencyProperty.UnsetValue;
                    }
                case PalletTypeConverterParameter.Foreground:
                    switch ((PalletType)value) {
                        case PalletType.Standard:
                            return DependencyProperty.UnsetValue;
                        case PalletType.Oversize:
                            return new SolidColorBrush(Colors.Purple);
                        case PalletType.FloorLoaded:
                            return new SolidColorBrush(Colors.Red);
                        default:
                            return DependencyProperty.UnsetValue;
                    }
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}