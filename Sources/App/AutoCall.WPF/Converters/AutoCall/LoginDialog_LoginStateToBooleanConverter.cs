using System;
using System.Windows.Data;
using AutoCall.Logic.Enums;

namespace AutoCall.WPF.Converters.AutoCall {
    /// <summary>
    ///     Represents converter "Login state" -> "Boolean" </summary>
    public class LoginDialog_LoginStateToBooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var state = (LoginState) value;
            switch (state) {
                case LoginState.LoggedIn:
                case LoginState.LoggedOut:
                case LoginState.LoginFailed:
                case LoginState.LogoutFailed:
                    return true;
                case LoginState.LoginStarted:
                case LoginState.LogoutStarted:
                    return false;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}