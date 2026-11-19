using System;
using System.Windows.Data;
using System.Windows.Media;
using AutoDispatch.Logic.Enums;

namespace AutoCall.WPF.Converters.Dispatch {
    /// <summary>
    ///     Represents converter "Engine state" -> "Background color brush" </summary>
    public class App_EngineStateToBackgroundConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var state = (EngineState) value;
            switch (state) {
                case EngineState.Connected:
                    return new SolidColorBrush(Colors.SkyBlue);
                case EngineState.Disconected:
                    return new SolidColorBrush(Colors.LightGray);
                case EngineState.Buzy:
                    return new SolidColorBrush(Colors.Khaki);
            }

            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}