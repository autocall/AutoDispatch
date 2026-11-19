using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBoard_FormattedAddressToCityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var address = value as string;
            if (String.IsNullOrEmpty(address)) return value;
            //4545 Ogden Ave, Chicago, IL 60623, USA
            var regex = new Regex(@"[ ]?(?<city>[\w ]+, [\w]{2}) \d*, USA$");
            var match = regex.Match(address);
            if (match.Success) {
                return match.Groups["city"].Value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
