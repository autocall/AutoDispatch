using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class FilterCollectionMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var list = values[0] as IList;
            var value = values[1];
            var property = parameter as string;
            var result = new List<object>();
            if (list != null && property != null) {
                foreach (object o in list) {
                    object cur = o;
                    foreach (var path in property.Split('.')) {
                        cur = cur.GetType().GetProperty(path).GetValue(cur, null);
                    }
                    if (cur.Equals(value)) {
                        result.Add(o);
                    }
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
