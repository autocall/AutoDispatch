using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.WPF.UserControls;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class PhoneControl : AutoGreyableImage {

        private readonly Logger<DatCompanyDetailsDialog> _l = new Logger<DatCompanyDetailsDialog>();

        public PhoneControl() {
            InitializeComponent();
        }

        private void Phone_OnMouseUp(object sender, MouseButtonEventArgs e) {
            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            var phone = new String(((string)((FrameworkElement)sender).DataContext)?.Where(Char.IsDigit).ToArray());
            _l.i($"Call phone: {phone}");
            callService?.StartCall(callService.ToPhoneNumber(phone));
        }
    }
}
