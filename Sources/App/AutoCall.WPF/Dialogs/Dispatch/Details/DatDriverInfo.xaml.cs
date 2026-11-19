using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class DatDriverInfo : UserControl {
        private DispatchService _service;

        private DatDriverViewModel ViewModel => (DatDriverViewModel)DataContext;

        public DatDriverInfo() {
            InitializeComponent();
            _service = App.Services.GetRequiredService<DispatchService>();
        }

        private void EditCarrierViewPickupComment_Click(object sender, RoutedEventArgs e) {
            var comment = ViewModel.CarrierViewPickupComment;
            var msgBox = new InputMessageBox("Edit Comment", "CarrierView Pickup Comment", comment);
            if (msgBox.ShowDialog() == true) {
                var clone = (DatDriverViewModel)ViewModel.Clone();
                clone.CarrierViewEdit = true;
                clone.CarrierViewPickupComment = msgBox.tbText.Text;
                loading.Visibility = Visibility.Visible;
                _service.SaveDatDriver(clone, SaveCompleted);
            }
        }

        private void EditCarrierViewDeliveryComment_Click(object sender, RoutedEventArgs e) {
            var comment = ViewModel.CarrierViewDeliveryComment;
            var msgBox = new InputMessageBox("Edit Comment", "CarrierView Delivery Comment", comment);
            if (msgBox.ShowDialog() == true) {
                var clone = (DatDriverViewModel)ViewModel.Clone();
                clone.CarrierViewEdit = true;
                clone.CarrierViewDeliveryComment = msgBox.tbText.Text;
                loading.Visibility = Visibility.Visible;
                _service.SaveDatDriver(clone, SaveCompleted);
            }
        }

        public async Task SaveCompleted(ViewModelWrapper<DatDriverViewModel> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveCompleted(wrapper));
                return;
            }
            loading.Visibility = Visibility.Hidden;
            if (wrapper.Status == ResponseStatus.Success) {
                ViewModel.Assign(wrapper.Data);
            } else {
                tbError.Visibility = Visibility.Visible;
                tbError.Text = wrapper.MessageError;
            }
        }
    }
}
