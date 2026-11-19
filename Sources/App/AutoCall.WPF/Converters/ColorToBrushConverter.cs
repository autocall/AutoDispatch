using System;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Represents converter "Color" -> "Solid brush" </summary>
    public class ColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Color color = (Color) value;
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var brush = (SolidColorBrush) value;
            return brush.Color;
        }
    }
}