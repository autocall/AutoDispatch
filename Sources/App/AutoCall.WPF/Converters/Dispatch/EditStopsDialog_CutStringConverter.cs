using System;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class EditStopsDialog_CutStringConverter : IValueConverter {
        public int Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null)
                return null;

            return value.ToString().Substring(0, 4);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}