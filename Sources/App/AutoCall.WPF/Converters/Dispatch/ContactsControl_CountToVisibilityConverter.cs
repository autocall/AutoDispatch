using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class ContactsControl_CountToVisibilityConverter : IValueConverter {

        public int Count { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var items = value as IList;
            return items != null && items.Count > Count ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
