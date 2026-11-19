using System;
using System.Windows.Data;
using System.Windows.Media;
using AutoCall.Logic.Enums;

namespace AutoCall.WPF.Converters.AutoCall {
    /// <summary>
    ///     Represents converter "test port state" -> "Brush" </summary>
    public class SettingsDialog_TestPortStateToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var state = (TestPortState) value;
            switch (state) {
                case TestPortState.TestFailed:
                    return new SolidColorBrush(Colors.Red);
                case TestPortState.TestSuccess:
                    return new SolidColorBrush(Colors.Green);
                default:
                    return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}