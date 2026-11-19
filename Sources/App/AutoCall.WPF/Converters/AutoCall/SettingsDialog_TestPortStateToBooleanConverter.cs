using System;
using System.Windows.Data;
using AutoCall.Logic.Enums;

namespace AutoCall.WPF.Converters.AutoCall {
    /// <summary>
    ///     Represents converter "test port state" -> "Visibility" </summary>
    public class SettingsDialog_TestPortStateToBooleanConverter : IValueConverter {
        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var state = (TestPortState) value;
            switch (state) {
                case TestPortState.Default:
                case TestPortState.TestFailed:
                case TestPortState.TestSuccess:
                    if (Inverted)
                        return true;
                    else
                        return false;
                case TestPortState.TestStarted:
                    if (Inverted)
                        return false;
                    else
                        return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}