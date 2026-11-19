using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBoard_TrailerNumberMultiConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is Guid && values[1] is IList<TruckViewModel> && values[2] is IList<TrailerViewModel>) {
                var truckId = (Guid)values[0];
                var trucks = (IList<TruckViewModel>)values[1];
                var trailers = (IList<TrailerViewModel>)values[2];
                var truck = trucks.FirstOrDefault(t => t.EntityId == truckId);
                if (truck != null) {
                    var trailer = trailers.FirstOrDefault(t => t.EntityId == truck.TrailerId);
                    if (trailer != null) {
                        return trailer.Number;
                    }
                } 
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
