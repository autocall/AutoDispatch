using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Logic.Services;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.MaskedInput;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class AssignDatDriverDialog : DetailsBaseDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => null;
        protected override Button CloseButton => btnClose;
        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public AssignDatDriverViewModel Model {
            get { return DataContext as AssignDatDriverViewModel; }
            set { DataContext = value; }
        }

        public DatDriverViewModel Result { get; set; }

        public AssignDatDriverDialog(
            DispatchService service,
            ICallService callService,
            IMapper mapper,
            LoadViewModel load,
            IList<DatDriverViewModel> drivers) : base(service, callService) {
            Model = new AssignDatDriverViewModel(load, drivers);

            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.SelectedDatDriver != null) {
                // edit truck and trailer
                Result = Model.SelectedDatDriver;
            } else {
                if (String.IsNullOrEmpty(Model.TextDatDriverPhone) == true) {
                    MessageBoxEx.Alert(this, "Please enter a phone number");
                    return;
                }
                if (String.IsNullOrEmpty(Model.TextDatDriverName) == true) {
                    MessageBoxEx.Alert(this, "Please enter a name of the driver");
                    return;
                }
                this.BusyStart();
                Result = new DatDriverViewModel();
            }
            Result.Name = Model.TextDatDriverName;
            Result.ContactPhone = Model.TextDatDriverPhone;
            Result.TruckNumber = Model.TextDatDriverTruckNumber;
            Result.TrailerNumber = Model.TextDatDriverTrailerNumber;
            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void tbPhone_ValueChanged(object sender, RadRoutedEventArgs args) {
            var value = (string)((RadMaskedInputValueChangingEventArgs)args).NewValue;
            var driver = Model.DatDrivers.FirstOrDefault(e => e.ContactPhone == value);
            if (value != null) {
                Model.SelectedDatDriver = driver;
            }
        }
    }
}
