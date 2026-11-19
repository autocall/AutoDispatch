using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Services;
using AutoCall.WPF.Helpers;
using AutoCall.WPF.UserControls;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class DetailsTrailerLayoutDialog : DetailsBaseDialog, IDispatchControl {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => null;
        protected override Button CloseButton => btnClose;

        private ReportService _reportService;
        private IMapper _mapper;

        public BoardType BoardType => BoardType.DispatchCarrier;
        public DispatchBoardType DispatchBoardType => DispatchBoardType.Carrier;

        public ActualCompanyViewModel SelectedBrokerCompany => throw new NotImplementedException();
        public ActualCompanyViewModel SelectedCarrierCompany => Model.ActualCompany;

        public DispatchActions DispatchActions { get; }
        public DispatchCommands DispatchCommands { get; }
        // abbr
        public DispatchCommands dc => DispatchCommands;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DetailesTrailerViewModel Model {
            get { return DataContext as DetailesTrailerViewModel; }
            set { DataContext = value; }
        }

        public DetailsTrailerLayoutDialog(
            DispatchService dispatchService,
            ICallService callService,
            IMapper mapper,
            DriverViewModel driver,
            TruckViewModel truck,
            TrailerViewModel trailer,
            IList<LoadViewModel> loads,
            ActualCompanyViewModel actualCompany) : base(dispatchService, callService) {
            _reportService = App.Services.GetRequiredService<ReportService>();
            _mapper = mapper;
            DispatchActions = new DispatchActions(this);
            DispatchCommands = new DispatchCommands(DispatchActions, this);

            InitializeComponent();

            Model = new DetailesTrailerViewModel(driver, truck, trailer, loads, actualCompany);

            var roles = App.Services.GetRequiredService<LoginService>().GetRoles();
            btnEditLayout.Visibility = roles.CheckPermission(RolesOperator.EditLayoutPallets) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override void BusyStart(string message = null) {
            loading.Visibility = Visibility.Visible;
            base.BusyStart(message);
        }

        public override void BusyFinish() {
            loading.Visibility = Visibility.Collapsed;
            base.BusyFinish();
        }

        private void DriverDetailesLoadDialog_OnLoaded(object sender, RoutedEventArgs e) {

        }

        private void btnMap_Clicked(object sender, RoutedEventArgs e) {
            var dialog = new EditStopsDialog(_dispatchService, _callService, _mapper, Model.Trailer, Model.Loads.Select(l => l.Load).ToList());
            dialog.Initialize();
            dialog.ShowDialog();
        }

        private async Task Edit() {
            var clones = Model.Loads.Select(l => (LoadViewModel)l.Load.Clone()).ToList();
            var dialog = new TrailerLayoutEditDialog(_dispatchService, _callService, _mapper,
                (TrailerViewModel)Model.Trailer.Clone(), clones);
            if (dialog.ShowDialog() == true) {
                await Model.SetLoads(dialog.Result);
            }
        }

        private async void btnEditLayout_Clicked(object sender, RoutedEventArgs e) {
            await Edit();
        }

        private void editComments_OnClicked(object sender, RoutedEventArgs e) {
            var editor = new RichTextEditor();
            if (((FrameworkElement)sender).DataContext is LoadViewModel) {
                var load = (LoadViewModel)((FrameworkElement)sender).DataContext;
                load = (LoadViewModel)load.Clone();
                editor.ViewModel = load.Comments;
                editor.Title = "Edit Comments";

                if (editor.ShowDialog() == true) {
                    load.Comments = editor.ViewModel;
                    this._dispatchService.SaveLoad(load, SaveCompleted);
                    BusyStart();
                }
            } else {
                throw new Exception("DataContext unrecognized. Type: " + ((FrameworkElement)sender).DataContext.GetType());
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        public async Task SetLoads(List<LoadViewModel> loads) {
            await Model.SetLoads(loads);
        }

        public async Task SaveCompleted<T, F>(ViewModelWrapper<T> wrapper1, ViewModelWrapper<F> wrapper2) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await this.SaveCompleted(wrapper1, wrapper2));
                return;
            }
            await base.SaveCompleted(wrapper1, wrapper2);
        }

        public async Task SaveCompleted<T>(ViewModelWrapper<T> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await this.SaveCompleted(wrapper));
                return;
            }

            if (wrapper is ViewModelWrapper<LoadViewModel>) {
                if (wrapper.Status == ResponseStatus.Success) {
                    await Model.SetLoads(new List<LoadViewModel>() { wrapper.Data as LoadViewModel });
                    Model.Trailer.GeneratePallets(Model.Loads.Select(e => e.Load).ToList());
                    OnCanExecuteChanged();
                }
                await base.SaveCompleted(wrapper, false);
                // Checks by empty
                if (Model.Loads.Any() == false) {
                    Close();
                }
            } else if (wrapper is ViewModelWrapper<FileViewModel>) {
                await base.SaveCompleted(wrapper, false);
            } else {
                await base.SaveCompleted(wrapper);
            }
        }

        public void OnCanExecuteChanged() {
            foreach (Command command in this.GetType().GetProperties().Where(e => e.PropertyType == typeof(Command)).Select(e => e.GetValue(this))) {
                command?.OnCanExecuteChanged();
            }
        }
    }
}
