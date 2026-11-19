using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AutoDispatch.Logic.Enums;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DispatchBrokerBoard_InsuranceStateConverter : IValueConverter {

        public enum BrushElementType {
            Background,
            Foreground,
            SelectedForeground,
            FontWeight,
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var elem = (BrushElementType)Enum.Parse(typeof(BrushElementType), (string)parameter);
            var state = (InsuranceStateType)value;

            switch (elem) {
                case BrushElementType.Background:
                    switch (state) {
                        case InsuranceStateType.Green:
                            return new SolidColorBrush(Colors.Transparent);
                        case InsuranceStateType.Yellow:
                            return new SolidColorBrush(Colors.Yellow);
                        case InsuranceStateType.Red:
                            return new SolidColorBrush(Colors.Red);
                        case InsuranceStateType.Gray:
                            return new SolidColorBrush(Colors.Gray);
                    }
                    return new SolidColorBrush(Colors.LightGray);
                case BrushElementType.Foreground:
                    switch (state) {
                        case InsuranceStateType.Green:
                            return new SolidColorBrush(Colors.Black);
                        case InsuranceStateType.Yellow:
                            return new SolidColorBrush(Colors.Black);
                        case InsuranceStateType.Red:
                            return new SolidColorBrush(Colors.White);
                        case InsuranceStateType.Gray:
                            return new SolidColorBrush(Colors.White);
                    }
                    return new SolidColorBrush(Colors.Black);
                case BrushElementType.FontWeight:
                    switch (state) {
                        case InsuranceStateType.Green:
                            return FontWeights.Normal;
                        case InsuranceStateType.Yellow:
                        case InsuranceStateType.Red:
                        case InsuranceStateType.Gray:
                            return FontWeights.Bold;
                    }
                    return FontWeights.Normal;
                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
