using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Logic.Services;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class EditDatDriverDialog : DetailsBaseDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => null;
        protected override Button CloseButton => btnClose;
        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DatDriverViewModel Model {
            get { return DataContext as DatDriverViewModel; }
            set { DataContext = value; }
        }

        public EditDatDriverDialog(
            DispatchService service,
            DatDriverViewModel datDriver) : base(service, null) {

            InitializeComponent();

            Model = (DatDriverViewModel)datDriver.Clone();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(Model.Name) == true) {
                MessageBoxEx.Alert(this, "Please enter a name of the driver");
                return;
            }
            base._dispatchService.SaveDatDriver(Model, (w) => this.SaveCompleted(w));
            base.BusyStart();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
