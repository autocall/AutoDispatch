using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class LocationDetailsMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var city = (string)values[0];
            var parent = (LoadViewModel)values[1];

            return parent.OriginCity == city || parent.DestinationCity == city ? String.Empty : "(stop)";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
