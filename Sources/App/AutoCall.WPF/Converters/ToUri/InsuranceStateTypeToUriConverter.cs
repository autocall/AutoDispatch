using System;
using System.Globalization;
using System.Windows.Data;
using AutoDispatch.Logic.Enums;

namespace AutoCall.WPF.Converters.ToUri {
    /// <summary>
    ///     Converter for InsuranceStateType icon </summary>
    [ValueConversion(typeof (InsuranceStateType), typeof (Uri))]
    public class InsuranceStateTypeToUriConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((InsuranceStateType)value) {
                case InsuranceStateType.Gray:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/shield_grey.png");
                case InsuranceStateType.Yellow:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/shield_yellow.png");
                case InsuranceStateType.Red:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/shield_red.png");
                case InsuranceStateType.Green:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/shield_green.png");
                default:
                    return new Uri("pack://application:,,,/AutoDispatch;component/Resources/shield_grey.png");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}