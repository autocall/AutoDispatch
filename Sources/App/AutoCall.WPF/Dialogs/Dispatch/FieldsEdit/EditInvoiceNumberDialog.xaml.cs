using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class EditInvoiceNumberDialog : BaseDispatchDialog {

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

        public EditInvoiceNumberDialog(DispatchService service) {
            _service = service;

            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            BusyStart();

            _service.SaveInvoice(Model, SaveInvoice_Completed);
        }

        private async Task SaveInvoice_Completed(ViewModelWrapper<LoadViewModel> wrapper) {
            await SaveCompleted(wrapper);
        }
    }
}
