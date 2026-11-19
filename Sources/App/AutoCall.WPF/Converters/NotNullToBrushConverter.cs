using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCall.WPF.Converters {
    /// <summary>
    ///     Convert Boolean -> Brush </summary>
    [ValueConversion(sourceType: typeof (object), targetType: typeof (SolidColorBrush))]
    public class NotNullToBrushConverter : IValueConverter {
        public SolidColorBrush TrueBrush { get; set; }
        public SolidColorBrush FalseBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value != null) ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}