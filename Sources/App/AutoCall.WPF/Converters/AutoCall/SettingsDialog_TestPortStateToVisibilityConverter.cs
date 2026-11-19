using System;
using System.Windows;
using System.Windows.Data;
using AutoCall.Logic.Enums;

namespace AutoCall.WPF.Converters.AutoCall {
    /// <summary>
    ///     Represents converter "test port state" -> "Visibility" </summary>
    public class SettingsDialog_TestPortStateToVisibilityConverter : IValueConverter {
        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var state = (TestPortState) value;
            switch (state) {
                case TestPortState.Default:
                case TestPortState.TestFailed:
                case TestPortState.TestSuccess:
                    if (Inverted)
                        return Visibility.Visible;
                    else
                        return Visibility.Hidden;
                case TestPortState.TestStarted:
                    if (Inverted)
                        return Visibility.Hidden;
                    else
                        return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}