using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Logic.Managers;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class SearchTrailerDialog : Window {

        #region [ Protected Fields ]

        /// <summary>
        ///     Flag = true if initialize of trailers was completed </summary>
        protected bool _trailersInitCompleted;

        #endregion [ Protected Fields ]

        private DispatchService _service;
        private MainManager _manager;

        public SearchTrailerViewModel Model {
            get { return DataContext as SearchTrailerViewModel; }
            set { DataContext = value; }
        }

        public SearchTrailerDialog(DispatchService service, MainManager manager) {
            _service = service;
            _manager = manager;
            _service.LoadTrailers(LoadTrailers_Completed);

            InitializeComponent();

            Model = new SearchTrailerViewModel();
            gridTrailers.ItemsSource = ((CollectionViewSource)Resources["WrappersViewSource"]).View;
        }

        private async Task LoadTrailers_Completed(ViewModelWrapper<List<TrailerViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadTrailers_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                _trailersInitCompleted = true;
                Model.Trailers.AddRange(wrapper.Data);
                CheckByLoaded();
            } else {
                MessageBoxEx.Error(this, wrapper.MessageError);
            }
        }

        private void CheckByLoaded() {
            if (_trailersInitCompleted) {
                loading.Visibility = Visibility.Collapsed;
                ((ICollectionView)gridTrailers.ItemsSource).Refresh();
            }
        }

        private void WrappersViewSource_OnFilter(object sender, FilterEventArgs e) {
            var wrapper = (TrailerViewModel)e.Item;
            e.Accepted = String.IsNullOrEmpty(Model.SearchText) ||
                (wrapper.Filter(Model.SearchText));
        }

        private void tbFilter_OnTextChanged(object sender, TextChangedEventArgs e) {
            ((ICollectionView)this.gridTrailers.ItemsSource).Refresh();
        }

        private void grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (Model.SelectedWrapper != null) {
                btnOk_Clicked(null, null);
            }
        }

        private void CreateTrailer_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditTrailerDialog(_service, null);
            if (dialog.ShowDialog() == true) {
                gridTrailers.Focus();
                Model.Trailers.Insert(0, dialog.Model);
                ((ICollectionView)this.gridTrailers.ItemsSource).Refresh();
                Model.SelectedWrapper = dialog.Model;
            }
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
