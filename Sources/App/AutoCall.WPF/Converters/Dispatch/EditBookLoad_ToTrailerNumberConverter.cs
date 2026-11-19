using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class EditBookLoad_ToTrailerNumberConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var load = (LoadViewModel)values[0];
            var trailers = (List<TrailerViewModel>)values[1];
            if (load == null) return null;
            if (load.TrailerId == null) return null;
            var number = trailers.FirstOrDefault(e => e.EntityId == load.TrailerId)?.Number;
            if (number != null) {
                return number + ", ";
            } else {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
