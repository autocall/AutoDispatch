using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditYardDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private DispatchService _service;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public YardViewModel Model {
            get { return DataContext as YardViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     .ctor </summary>
        public EditYardDialog(DispatchService service, YardViewModel warehouse) {
            _service = service;

            InitializeComponent();

            if (warehouse != null) {
                Model = warehouse;
                Title = "Edit Yard";
            } else {
                Model = new YardViewModel();
                Title = "Add Yard";
            }
        }

        private void btnLocation_Clicked(object sender, RoutedEventArgs e) {
            var dlg = new EditLocationDialog(false);
            dlg.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                Location = Model.Location,
            });
            dlg.Initialize();
            if (dlg.ShowDialog() == true) {
                Model.Location = dlg.Model.Location.Location;
                Model.Address = dlg.Model.Location.Address;
                Model.City = dlg.Model.Location.City;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs args) {
            if (Model.Location == null) {
                MessageBoxEx.Error(this, "Please set 'Location'");
                return;
            }
            if (DispatchBoardViewModel.Instance.Yards.Any(e => e.Name.Trim().ToLower() == Model.Name.Trim().ToLower() && e.EntityId != Model.EntityId)) {
                MessageBox.Show($"Yard Number '{Model.Name}' already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _service.SaveYard(Model, SaveCompleted);

            base.BusyStart();
        }

        private async Task SaveCompleted(ViewModelWrapper<YardViewModel> wrapper) {
            await base.SaveCompleted(wrapper);
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}