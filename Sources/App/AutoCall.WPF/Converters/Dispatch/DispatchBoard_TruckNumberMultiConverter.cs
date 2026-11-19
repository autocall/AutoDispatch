using AutoDispatch.Logic.ViewModels;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBoard_TruckNumberMultiConverter : IMultiValueConverter {

        public string Path { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values[0] is Guid && values[1] is IEnumerable) {
                var models = ((IEnumerable)values[1]).Cast<BaseDispatchViewModel>();
                var model = models.FirstOrDefault(t => t.EntityId == (Guid) values[0]);
                if (model != null) {
                    return model.GetType().GetProperty(Path).GetValue(model);
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
