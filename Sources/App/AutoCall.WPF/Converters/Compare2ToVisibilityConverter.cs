using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class Compare2ToVisibilityConverter : IValueConverter {

        public bool Inverted { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(Inverted == false) {
                return Object.Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed;
            } else {
                return Object.Equals(value, parameter) ? Visibility.Collapsed: Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
