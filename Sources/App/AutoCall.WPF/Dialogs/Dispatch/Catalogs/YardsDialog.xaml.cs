using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class YardsDialog : BaseDispatchDialog {

        #region [ Base ]

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        #endregion [ Base ]

        #region [ _fields ]

        private DispatchService _service;
        private SettingsService _settings;
        private IMapper _mapper;
        private DispatchActions dispatchActions;

        #endregion [ _fields ]

        #region [ View Model ]

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public YardsViewModel Model {
            get { return DataContext as YardsViewModel; }
            set { DataContext = value; }
        }

        #endregion [ View Model ]

        #region [ .ctor ]

        public YardsDialog(DispatchService service, SettingsService settings) {
            _service = service;
            _settings = settings;
            dispatchActions = new DispatchActions(null);
            Model = new YardsViewModel();
            _mapper = App.Services.GetRequiredService<IMapper>();

            InitializeComponent();
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        private void Initialize() {
            loadingYards.Visibility = Visibility.Visible;
            Model.Yards?.Clear();
            _service.LoadYards(LoadYards_Completed);
        }

        #endregion [ Initialize ]

        #region [ Event Handlers ]

        private void YardsDialog_OnLoaded(object sender, RoutedEventArgs e) {
            Initialize();
            try {
                var data = _settings.GetEquipmentsDialogGridViewYards();
                if (data != null) {
                    gridYards.SetSettings(data);
                }
            } finally {
            }
        }

        private void YardsDialog_OnClosed(object sender, EventArgs e) {
            loadingYards.Visibility = Visibility.Visible;
            _settings.SetEquipmentsDialogGridViewYards(gridYards.GetSettings());
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        #endregion [ Event Handlers ]

        #region [ Yards ]

        private async Task LoadYards_Completed(ViewModelWrapper<List<YardViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadYards_Completed(wrapper));
                return;
            }
            loadingYards.Visibility = Visibility.Collapsed;
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                foreach (YardViewModel companyViewModel in wrapper.Data.OrderByDescending(c => c.CreatedDate)) {
                    Model.Yards.Add(companyViewModel);
                }
            }
        }

        private void CreateYard_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditYardDialog(_service, null);
            if (dialog.ShowDialog() == true) {
                Initialize();
            }
        }

        private void EditYard() {
            if (gridYards.SelectedItem != null) {
                var dialog = new EditYardDialog(_service, (YardViewModel)((YardViewModel)gridYards.SelectedItem).Clone());
                if (dialog.ShowDialog() == true) {
                    Initialize();
                }
            }
        }

        private void EditYard_Clicked(object sender, RoutedEventArgs e) {
            EditYard();
        }

        private void gridYards_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditYard();
        }

        private void DeleteYard_Clicked(object sender, RoutedEventArgs e) {
            if (gridYards.SelectedItem != null) {
                if (MessageBox.Show("Are you sure you want to delete selected Yard?", "Delete Yard",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                _service.DeleteYard(((YardViewModel)gridYards.SelectedItem).EntityId, DeleteYardCompleted);
            }
        }

        private async Task DeleteYardCompleted(ViewModelWrapper<YardViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await DeleteYardCompleted(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Initialize();
            }
        }

        #endregion [ Yards ]
    }
}