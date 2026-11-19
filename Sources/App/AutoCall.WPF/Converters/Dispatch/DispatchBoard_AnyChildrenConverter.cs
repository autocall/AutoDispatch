using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBoard_AnyChildrenConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            //return value != null && ((Wrapper1LoadViewModel)value).Load.ContainerType != Common.Enums.ContainerType.None;
            return value != null && ((Wrapper1LoadViewModel)value).Children.Any();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
