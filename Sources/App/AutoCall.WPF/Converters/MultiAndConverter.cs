using System;
using System.Globalization;
using System.Windows.Data;

namespace AutoCall.WPF.Converters {
    public class MultiConditionConverter : IMultiValueConverter {
        public enum ConditionType {
            And,
            Or
        }

        public ConditionType Condition { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var result = (bool) values[0];
            for (int i = 1; i < values.Length; i++) {
                switch (Condition) {
                    case ConditionType.Or:
                        result |= (bool) values[i];
                        break;
                    case ConditionType.And:
                        result &= (bool) values[i];
                        break;
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}