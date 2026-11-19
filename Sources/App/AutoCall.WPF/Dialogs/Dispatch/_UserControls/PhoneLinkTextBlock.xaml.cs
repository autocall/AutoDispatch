using System.Windows;
using System.Windows.Controls;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class PhoneLinkTextBlock : TextBlock {

        public PhoneLinkTextBlock() {
            InitializeComponent();
        }

        private async void Hyperlink_OnClick(object sender, RoutedEventArgs e) {
            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            var phone = StringExtension.OnlyNumbers(((string)((FrameworkContentElement)sender).DataContext));
            await callService?.StartCallAsync(callService.ToPhoneNumber(phone));
        }
    }
}
