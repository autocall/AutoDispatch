using AutoCall.Common.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCall.WPF.Converters.Dispatch {
    public enum TemperatureToColorConverterParameter {
        Background,
        Foreground,
        FontWeight,
    }
    public class TemperatureToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var load = (LoadViewModel)value;
            if (load != null && load.Temperature != null && load.Equipment == AutoCallEquipmentType.Reefer) {
                if (load.Temperature > 39) {
                    switch ((TemperatureToColorConverterParameter)parameter) {
                        case TemperatureToColorConverterParameter.Background:
                            return DependencyProperty.UnsetValue;
                        case TemperatureToColorConverterParameter.Foreground:
                            return Brushes.Red;
                        case TemperatureToColorConverterParameter.FontWeight:
                            return FontWeights.Bold;
                    }
                }
                if (load.Temperature <= 32) {
                    switch ((TemperatureToColorConverterParameter)parameter) {
                        case TemperatureToColorConverterParameter.Background:
                            return DependencyProperty.UnsetValue;
                        case TemperatureToColorConverterParameter.Foreground:
                            return Brushes.Blue;
                        case TemperatureToColorConverterParameter.FontWeight:
                            return FontWeights.Bold;
                    }
                }
                // 32 - 39
                switch ((TemperatureToColorConverterParameter)parameter) {
                    case TemperatureToColorConverterParameter.Background:
                        return DependencyProperty.UnsetValue;
                    case TemperatureToColorConverterParameter.Foreground:
                        return Brushes.Green;
                    case TemperatureToColorConverterParameter.FontWeight:
                        return FontWeights.Bold;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
