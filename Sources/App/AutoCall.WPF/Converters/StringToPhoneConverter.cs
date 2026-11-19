using AutoCall.Common.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "string" -> "phone number" </summary>
    public class StringToPhoneConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var phone = (string)value;
            if (phone == null) return null;
            phone = phone.StartsWith("+1") ? phone.Substring(2) : phone;
            var digits = StringExtension.OnlyNumbers(phone);
            if (digits.Length == 10) {
                return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6)}";
            }
            if (digits.Length > 10) {
                return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)} ({digits.Substring(10)})";
            }
            return phone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}