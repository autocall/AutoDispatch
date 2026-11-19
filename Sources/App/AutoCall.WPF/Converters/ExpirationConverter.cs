using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoCall.WPF.Converters {
    public enum ExpirationConverterType {
        Background,
        Foreground,
        FontWeight,
    }

    public class ExpirationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return DependencyProperty.UnsetValue;

            int expired = 0; // 0 - transparent, 1 - yellow, 2 - red

            if (((DateTime)value - DateTime.Now).TotalDays < 30) {
                expired = 2;
            } else
            if (((DateTime)value - DateTime.Now).TotalDays < 60) {
                expired = 1;
            } 

            if (expired == 0) {
                return DependencyProperty.UnsetValue;
            }
            if (expired == 1) {
                switch ((ExpirationConverterType)parameter) {
                    case ExpirationConverterType.Background:
                        return new SolidColorBrush(Colors.Yellow);
                    case ExpirationConverterType.Foreground:
                        return new SolidColorBrush(Colors.Black);
                    case ExpirationConverterType.FontWeight:
                        return FontWeights.Bold;
                }
            }
            if (expired == 2) {
                switch ((ExpirationConverterType)parameter) {
                    case ExpirationConverterType.Background:
                        return new SolidColorBrush(Colors.Red);
                    case ExpirationConverterType.Foreground:
                        return new SolidColorBrush(Colors.White);
                    case ExpirationConverterType.FontWeight:
                        return FontWeights.Bold;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (parameter == null)
                return DependencyProperty.UnsetValue;

            return parameter;
        }
    }
}