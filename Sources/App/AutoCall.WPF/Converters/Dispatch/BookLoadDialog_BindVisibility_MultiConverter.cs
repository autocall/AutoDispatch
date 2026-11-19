using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Converters.Dispatch {
    public class BookLoadDialog_BindVisibility_MultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var load = values[0] as LoadViewModel;
            var parent = values[1] as BaseObjectViewModel;

            if (load != null && parent != null) {
                // Check Parent
                if (load.TrailerId == null && load.DatDriverId == null && load.DatCompanyId == null && load.WarehouseId == null) {
                    // Check board
                    if (load.BoardType == DispatchBoardType.Carrier && (parent is DriverViewModel || parent is TruckViewModel || parent is TrailerViewModel || parent is WarehouseViewModel)) {
                        return Visibility.Visible;
                    }
                    // Check board
                    if (load.BoardType == DispatchBoardType.Broker && (parent is DatDriverViewModel || parent is DatCompanyViewModel)) {
                        return Visibility.Visible;
                    }
                }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
