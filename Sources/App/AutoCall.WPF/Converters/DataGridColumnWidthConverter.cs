using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Width settings" <-> "Datagrid column width" </summary>
    public class DataGridColumnWidthConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int width = (int) value;
            return new DataGridLength(width, DataGridLengthUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            DataGridLength width = (DataGridLength) value;
            return width.Value;
        }
    }
}