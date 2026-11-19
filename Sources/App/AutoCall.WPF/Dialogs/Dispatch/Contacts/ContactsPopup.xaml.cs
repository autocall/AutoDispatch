using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    /// <summary>
    ///     Popup work with GridViewPhoneColumn in paired </summary>
    public partial class ContactsPopup : Popup {

        public ContactsPopup() {
            InitializeComponent();
        }

        private void Border_MouseOnEnter(object sender, MouseEventArgs e) {
            PlacementContainer.PlacementTarget = (UIElement) sender;
        }

        private async void Border_MouseOnLeave(object sender, MouseEventArgs e) {
            PlacementContainer.PlacementTarget = null;
            await Task.Delay(500);

            if (PlacementContainer.PlacementTarget == null) {
                IsOpen = false;
            }
        }

        private  async void Phone_OnClick(object sender, RoutedEventArgs e) {
            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            var phone = StringExtension.OnlyNumbers(((FrameworkContentElement)sender).DataContext?.ToString());
            await callService?.StartCallAsync(callService.ToPhoneNumber(phone));
        }
    }
}
