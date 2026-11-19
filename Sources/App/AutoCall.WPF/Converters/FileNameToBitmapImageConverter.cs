using System;
using System.Globalization;
using System.Windows.Data;
using AutoDispatch.Logic.Heplers;

namespace AutoCall.WPF.Converters {
    public class FileNameToBitmapImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var str = value as string;
            if (String.IsNullOrEmpty(str) == false) {
                return FileIconLoader.GetFileIcon(str, true);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
