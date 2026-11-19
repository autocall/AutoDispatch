using System.Windows;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    public partial class RadPhoneButton : RadButton {

        public RadPhoneButton() {
            InitializeComponent();
        }

        private async void Phone_OnMouseUp(object sender, RoutedEventArgs routedEventArgs) {
            var phone = ((FrameworkElement) sender).DataContext as string;
            var viewModel = ((FrameworkElement)sender).DataContext as ContactBasePhoneViewModel;


            if (viewModel != null) {
                phone = viewModel.Phone + viewModel.Ext;
            }

            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            phone = StringExtension.OnlyNumbers(phone);
            await callService?.StartCallAsync(callService.ToPhoneNumber(phone));
        }
    }
}
