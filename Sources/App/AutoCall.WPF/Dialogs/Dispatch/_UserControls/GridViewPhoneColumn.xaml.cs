using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Managers;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {

    public partial class GridViewPhoneColumn : GridViewDynamicHyperlinkColumn {
        public GridViewPhoneColumn() {
            InitializeComponent();
        }

        private static ContactsPopup _contactsPopup;

        private ContactsPopup ContactsPopup {
            get {
                if (_contactsPopup == null) {
                    _contactsPopup = new ContactsPopup();
                }
                return _contactsPopup;
            }
        }

        public override FrameworkElement CreateCellElement(GridViewCell cell, object dataItem) {
            cell.SetBinding(FrameworkElement.TagProperty, DataMemberBinding);
            return base.CreateCellElement(cell, dataItem);
        }

        private async void Cell_MouseOnEnter(object sender, MouseEventArgs e) {
            if ((((FrameworkElement)sender).Tag as ContactsViewModel)?.IsEmpty == false) {
                PlacementContainer.PlacementTarget = (UIElement)sender;
                await Task.Delay(200);

                if (Equals(sender, PlacementContainer.PlacementTarget)) {
                    ContactsPopup.PlacementTarget = PlacementContainer.PlacementTarget;
                    ContactsPopup.DataContext = ((FrameworkElement)sender).Tag;
                    // reopen
                    ContactsPopup.IsOpen = false;
                    ContactsPopup.IsOpen = true;
                }
            }
        }

        private async void Cell_MouseOnLeave(object sender, MouseEventArgs e) {
            PlacementContainer.PlacementTarget = null;
            await Task.Delay(500);

            if (PlacementContainer.PlacementTarget == null) {
                ContactsPopup.IsOpen = false;
            }
        }

        private async void Phone_Click(object sender, RoutedEventArgs e) {
            var callService = App.Services.GetRequiredService<MainManager>().GetCallService;
            var phone = StringExtension.OnlyNumbers(((GridViewCell)sender).Value?.ToString());
            var numbers  = callService.ToPhoneNumber(phone);
            if (String.IsNullOrEmpty(numbers) == false) {
                await callService?.StartCallAsync(numbers);
            }
        }
    }
}
