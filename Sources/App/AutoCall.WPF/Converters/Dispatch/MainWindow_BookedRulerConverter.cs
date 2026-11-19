using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCall.WPF.Converters.Dispatch {
    public class MainWindow_BookedRulerConverter : IValueConverter {
        public enum PropertyEnum {
            carriageX,
            carriageXLeft,
            carriageXRight,
            totalXRight,
            Background,
            Foreground,
        }

        public PropertyEnum Property { get; set; }
        // 200: 0 - 20
        public int Width { get; set; }

        public object Convert(object obj, Type targetType, object parameter, CultureInfo culture) {
            if (obj == null) {
                return DependencyProperty.UnsetValue;
            }
            var value = Math.Min((int)obj, 20);
            switch (Property) {
                case PropertyEnum.carriageX:
                    return value * 10;
                case PropertyEnum.carriageXLeft:
                    return value * 10 - 4;
                case PropertyEnum.carriageXRight:
                    return value * 10 + 4;
                case PropertyEnum.totalXRight:
                    return Width - value * 10;
                case PropertyEnum.Background:
                    if (value >= 15) {
                        return new SolidColorBrush(Colors.LimeGreen);
                    } else if (value >= 10) {
                        return new SolidColorBrush(Colors.DodgerBlue);
                    } else if (value >= 5) {
                        return new SolidColorBrush(Colors.Yellow);
                    } else {
                        return new SolidColorBrush(Colors.Red);
                    }
                case PropertyEnum.Foreground:
                    if (value >= 15) {
                        return new SolidColorBrush(Colors.Black);
                    } else if (value >= 10) {
                        return new SolidColorBrush(Colors.White);
                    } else if (value >= 5) {
                        return new SolidColorBrush(Colors.Black);
                    } else {
                        return new SolidColorBrush(Colors.White);
                    }
                default:
                    throw new Exception();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
