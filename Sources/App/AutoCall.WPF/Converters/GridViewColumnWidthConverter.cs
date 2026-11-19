using System;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Width settings" <-> "Datagrid column width" </summary>
    public class GridViewColumnWidthConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int width = (int) value;
            return new GridViewLength(width, GridViewLengthUnitType.Pixel);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            GridViewLength width = (GridViewLength) value;
            return width.Value;
        }
    }
}