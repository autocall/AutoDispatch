using System;
using System.Globalization;
using System.Windows.Data;
using AutoCall.Common.Helpers;

namespace AutoCall.WPF.Converters.Dispatch {
    public class RadRangeDateTimePickerConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length >= 3 && values[0] != null && values[1] != null && values[2] != null) {
                return DispatchHelper.DateTimeToAppointmentFullDateTime((DateTime)values[0], (TimeSpan)values[1], (TimeSpan)values[2]);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            var str = value as string;
            if (String.IsNullOrEmpty(str) == false) {
                str = str.ToLower();
                if (str.Contains("app")) {
                    var arr = str.Replace("app", "|").Split('|');
                    if (DateTime.TryParseExact(arr[0].Trim(), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                        DateTime.TryParseExact($"{arr[0].Trim()} {arr[1].Trim()}", "M/d/yyyy h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)) {
                        return new object[3] { date, time.TimeOfDay, time.TimeOfDay };
                    }
                } else {
                    var arr = str.Replace(" - ", " ").Split(' ');
                    if (DateTime.TryParseExact(arr[0].Trim(), "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                        DateTime.TryParseExact($"{arr[0].Trim()} {arr[1].Trim()}", "M/d/yyyy h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time1) &&
                        DateTime.TryParseExact($"{arr[0].Trim()} {arr[2].Trim()}", "M/d/yyyy h:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time2)) {
                        return new object[3] { date, time1.TimeOfDay, time2.TimeOfDay };
                    }
                }
            } 
            return new object[3] { null, null, null };
        }
    }
}
