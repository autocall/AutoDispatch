using AutoDispatch.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBoard_CustomStatusIdToNameConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            var session = UserSession.Current;
            if (session.CustomStatuses.ContainsKey((Guid)value)) {
                return session.CustomStatuses[(Guid)value];
            } else {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
