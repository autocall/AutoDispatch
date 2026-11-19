using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class SendSmsDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        public SendSmsViewModel Model {
            get { return DataContext as SendSmsViewModel; }
            set { DataContext = value; }
        }

        public SendSmsDialog(StopType stopType, LoadViewModel load, ActualCompanyViewModel actualCompany) {
            InitializeComponent();

            this.Model = new SendSmsViewModel() {
                ActualCompanyId = actualCompany.EntityId,
                Message = DispatchHelper.GetMessage(stopType, load, actualCompany),
                Drivers = DispatchBoardViewModel.Instance.Drivers.Where(e => String.IsNullOrEmpty(e.Phone) == false).ToList()
            };
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Model.ResultPhone) || Model.ResultPhone.Length < 10) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Please enter a valid phone number";
                return;
            }
            base.BusyStart();
            var service = App.Services.GetRequiredService<DispatchService>();
            service.SendSms(Model.ActualCompanyId, Model.ResultPhone, Model.Message, SendSms_Completed);
        }

        private async Task SendSms_Completed(ViewModelWrapper<string> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SendSms_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                MessageBoxEx.Info("Sucessed!");
            }

            await base.SaveCompleted(wrapper);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
