using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using AutoDispatch.Logic.Heplers;
using Newtonsoft.Json.Linq;

namespace AutoCall.WPF.Converters {
    public class EditBaseLoadControl_ParseMatchConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var result = new List<Inline>();

            var mainMatch = PlaceAutocompleteJsonParser.GetMainMatch((JObject)value);
            var mainText = PlaceAutocompleteJsonParser.GetMainText((JObject)value);
            var secondaryText = PlaceAutocompleteJsonParser.GetSecondaryText((JObject)value);

            if (mainText != null) {
                if (mainMatch != null) {
                    result.Add(new Run(mainMatch) { FontWeight = FontWeights.Bold });
                    result.Add(new Run(mainText.Replace(mainMatch, String.Empty)));
                } else {
                    result.Add(new Run(mainText));
                }
            }
            if (secondaryText != null) {
                result.Add(new Run($" {secondaryText}") { Foreground = Brushes.Gray });
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
