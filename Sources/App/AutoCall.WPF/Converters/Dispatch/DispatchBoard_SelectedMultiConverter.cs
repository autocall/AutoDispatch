using AutoDispatch.Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBoard_SelectedMultiConverter :IMultiValueConverter{
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is BaseObjectViewModel && values[1] is IList<BaseObjectViewModel>) {
                var result = ((IList<BaseObjectViewModel>) values[1]).Contains(values[0]);
                if (result == true) {
                    Debug.WriteLine($" - select {((BaseObjectViewModel)values[0]).EntityId}"); // TODO DEBUG
                }

                return result;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
