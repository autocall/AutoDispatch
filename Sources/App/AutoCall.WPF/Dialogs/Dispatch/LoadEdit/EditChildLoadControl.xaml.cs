using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoDispatch.Dtos.Enums;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class EditChildLoadControl : EditBaseLoadControl {
        public EditChildLoadControl() {
            InitializeComponent();
        }

        private void EditBaseLoadControl_Loaded(object sender, RoutedEventArgs e) {
        }

        private async void RadAutoCompleteBox_SearchAddressTextChanged(object sender, EventArgs e) {
            await RadAutoCompleteBox_SearchAddressTextChanged(sender, e,
               ((FrameworkElement)sender).Tag.Equals(CacheLoadIndexType.Origin) ? this.originBusy : this.descriptionBusy);
        }

        protected async void RadAutoCompleteBox_SearchAddressSelectionChanged(object sender, SelectionChangedEventArgs e) {
            await RadAutoCompleteBox_SearchAddressSelectionChanged(sender, e,
                ((FrameworkElement)sender).Tag.Equals(CacheLoadIndexType.Origin) ? this.originBusy : this.descriptionBusy);
        }

        private void btnSendSmsPickup_OnClick(object sender, RoutedEventArgs args) {
            var dialog = new SendSmsDialog(StopType.Pickup, Model.Load, Model.Root.ActualCompanies.First(e => e.EntityId == Model.Load.ActualCompanyId));
            dialog.ShowDialog();
        }

        private void btnSendSmsDelivery_OnClick(object sender, RoutedEventArgs args) {
            var dialog = new SendSmsDialog(StopType.Delivery, Model.Load, Model.Root.ActualCompanies.First(e => e.EntityId == Model.Load.ActualCompanyId));
            dialog.ShowDialog();
        }
    }
}