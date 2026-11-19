using System;
using System.Globalization;
using System.Windows.Data;
using Telerik.Windows.Documents.FormatProviders.Txt;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Model;

namespace AutoCall.WPF.Converters.Dispatch {
    public class RtfToTextConverter : IValueConverter {



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var providerXaml = new XamlFormatProvider();
            var providerTxt = new TxtFormatProvider();
            
            RadDocument docRtf = providerXaml.Import((string)value);
            string result = providerTxt.Export(docRtf);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
