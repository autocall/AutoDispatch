using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DatCompanyDetailsDialog_VisibilityConfirmationConverter : IValueConverter {

        public enum OrderPropertyName {
            NewConfirmation,
            EditConfirmation,
            Send,
            Resend,
        }

        public OrderPropertyName OrderProperty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var load = value as LoadViewModel;
            if (load != null) {
                if (OrderProperty == OrderPropertyName.NewConfirmation) {
                    if (load.ConfirmationFile == null) {
                        return Visibility.Visible;
                    }
                }
                if (OrderProperty == OrderPropertyName.EditConfirmation) {
                    if (load.ConfirmationFile != null && load.ConfirmedFile == null) {
                        return Visibility.Visible;
                    }
                }
                if (OrderProperty == OrderPropertyName.Send) {
                    if (load.ConfirmationFile != null) {
                        if (load.ConfirmationDate != null && load.EmailConfirmationSentDate == null) {
                            return Visibility.Visible;
                        }
                    }
                }
                if (OrderProperty == OrderPropertyName.Resend) {
                    if (load.ConfirmationFile != null) {
                        if (load.ConfirmationDate != null && load.EmailConfirmationSentDate != null && load.ConfirmedDate == null) {
                            return Visibility.Visible;
                        }
                    }
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
