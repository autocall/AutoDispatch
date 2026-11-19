using System;
using System.Globalization;
using System.Windows.Data;
using AutoDispatch.Logic.Enums;

namespace AutoCall.WPF.Converters.ToUri {
    [ValueConversion(typeof(EngineState), typeof(Uri))]
    public class EngineStateToUriConverter : IValueConverter {
           public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((EngineState)value) {
                case EngineState.Connected:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/circle-green.png");
                case EngineState.Disconected:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/circle-red.png");
                case EngineState.Buzy:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/circle_buzy.gif");
                case EngineState.Unknown:
                default:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/circle-red.png");
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
