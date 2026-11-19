using System;
using System.Windows;
using System.Windows.Data;
using AutoCall.Logic.Enums;

namespace AutoCall.WPF.Converters.AutoCall {
    /// <summary>
    ///     Represents converter "Login state" -> "Visibility" </summary>
    public class LoginDialog_LoginStateToVisibilityConverter : IValueConverter {
        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var state = (LoginState) value;
            switch (state) {
                case LoginState.LoggedIn:
                case LoginState.LoggedOut:
                case LoginState.LoginFailed:
                case LoginState.LogoutFailed:
                    if (Inverted)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                case LoginState.LoginStarted:
                case LoginState.LogoutStarted:
                    if (Inverted)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}