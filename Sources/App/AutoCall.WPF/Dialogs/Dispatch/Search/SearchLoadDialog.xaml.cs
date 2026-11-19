using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using AutoCall.Common;
using AutoCall.Common.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Behaviors;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows.Controls;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class SearchLoadDialog : BaseDispatchDialog {
        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public SearchCarrierLoadViewModel Model {
            get { return DataContext as SearchCarrierLoadViewModel; }
            set { DataContext = value; }
        }

        public SearchLoadDialog(
            DispatchService _dispatchService, MainManager manager, SettingsService settings, 
                LoadViewModel brokerLoad, List<Wrapper1LoadViewModel> carrierLoads) {
            InitializeComponent();

            this.Model = new SearchCarrierLoadViewModel() {
                BrokerLoad = brokerLoad,
                CarrierLoads = carrierLoads
            };
            gridLoads.ItemsSource = ((CollectionViewSource)Resources["LoadsViewSource"]).View;
            loading.Visibility = Visibility.Collapsed;
        }

        private void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (Wrapper1LoadViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchText) || wrapper.Load.Filter(Model.SearchText);
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
        }

        private void gridLoads_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Model.SelectedItem != null) {
                DialogResult = true;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (Model.SelectedItem != null) {
                DialogResult = true;
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
