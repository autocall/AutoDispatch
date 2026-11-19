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
    public partial class EnableTrackerDialog : DetailsBaseDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => null;
        protected override Button CloseButton => btnClose;
        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public EnableTrackerViewModel Model {
            get { return DataContext as EnableTrackerViewModel; }
            set { DataContext = value; }
        }

        public EnableTrackerDialog(
            DispatchService service,
            ICallService callService,
            IMapper mapper) : base(service, callService) {
            Model = new EnableTrackerViewModel();

            InitializeComponent();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            Model.TextName = Model.TextName;
            Model.TextPhone = Model.TextPhone;
            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
