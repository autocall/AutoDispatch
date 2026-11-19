using System;
using System.Globalization;
using System.Windows.Data;
using AutoCall.Logic.Enums;

namespace AutoCall.WPF.Converters.ToUri {
    /// <summary>
    ///     Converter for CallService icon </summary>
    [ValueConversion(typeof (CallServiceType), typeof (Uri))]
    public class CallServiceTypeToUriConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((CallServiceType) value) {
                // Skype, Bria, Talk, Vodia (Chrome), Desktop — типы V1, в перечне софтфонов V3 нет (AI/wiki/plan.md «Софтфоны в App»)
                //case CallServiceType.Zoiper:
                //    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/zoiper.png");
                case CallServiceType.MicroSIP:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/autophone.png");
                case CallServiceType.AbtoPhone:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/abtophone.png");
                case CallServiceType.Unknown:
                default:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/icon_question.png");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}