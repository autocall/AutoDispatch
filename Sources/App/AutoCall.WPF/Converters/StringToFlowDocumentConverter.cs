using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace AutoCall.WPF.Converters {
    public class StringToFlowDocumentConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var flowDocument = new FlowDocument();
            if (value == null) {
                return flowDocument;
            }
            byte[] byteArray = Encoding.ASCII.GetBytes((string) value);
            MemoryStream stream = new MemoryStream(byteArray);

            TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            textRange.Load(stream, DataFormats.Rtf);
            flowDocument.PagePadding = new Thickness(0);
            return flowDocument;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
