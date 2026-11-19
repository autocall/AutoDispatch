using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class EditLineHaulDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private readonly DispatchService _service;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public LoadViewModel Model {
            get { return DataContext as LoadViewModel; }
            set { DataContext = value; }
        }

        public EditLineHaulDialog(DispatchService service) {
            _service = service;

            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            BusyStart();

            Model.ConfirmationDate = null;
            Model.ConfirmationFile = null;
            Model.ConfirmedDate = null;
            Model.ConfirmedFile = null;
            Model.EmailConfirmationSent = null;
            Model.EmailConfirmationSentDate = null;
            _service.SaveLoad(Model, SaveLoad_Completed);
        }

        private void Currency_OnClicked(object sender, RoutedEventArgs e) {
            Model.Currency = (CurrencyType)(((byte)Model.Currency + 1) % Enum.GetValues(typeof(CurrencyType)).Length);
        }

        private async Task SaveLoad_Completed(ViewModelWrapper<LoadViewModel> wrapper) {
            await SaveCompleted(wrapper);
        }
    }
}
