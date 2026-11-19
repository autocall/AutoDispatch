//using System;
//using System.Collections.ObjectModel;
//using System.Globalization;
//using System.Linq;
//using System.Windows.Data;
//using AutoDispatch.Logic.ViewModels.Items;

//namespace AutoCall.WPF.Converters.Dispatch {
//    public class TruckIdTrucksLoadsToAttachLoadsConverter : IMultiValueConverter {
//        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
//            var truckId = values[0] as Guid?;
//            var trucks = values[1] as ObservableCollection<TruckViewModel>;
//            var loads = values[2] as ObservableCollection<LoadViewModel>;
//            var driverSearch = values[3] as string;
//            if (truckId != null && trucks != null && loads != null) {
//                var result = loads
//                    .Where(l => l.TrailerId != null && trucks.Any(t => t.EntityId == truckId && t.TrailerId == l.TrailerId) && l.Filter(driverSearch))
//                    .OrderBy(l => l.BindDate) // TODO check order
//                    .ToList();
//                return result;
//            } else {
//                return null;
//            }
//        }

//        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
//            throw new NotImplementedException();
//        }
//    }
//}
