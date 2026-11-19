using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Logic.Services;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using AutoCall.WPF.UserControls.Boards;
using AutoCall.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels;
using System.Threading.Tasks;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class DetailsCarrierDialog : DetailsBaseDialog, IDispatchControl {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => null;
        protected override Button CloseButton => btnClose;

        private ReportService _reportService;
        private IMapper _mapper;
        private HelperDataService _helper;

        public BoardType BoardType => BoardType.DispatchBroker;
        public DispatchBoardType DispatchBoardType => DispatchBoardType.Broker;

        public ActualCompanyViewModel SelectedBrokerCompany => throw new NotImplementedException();
        public ActualCompanyViewModel SelectedCarrierCompany => Model.ActualCompany;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public DetailesCarrierViewModel Model {
            get { return DataContext as DetailesCarrierViewModel; }
            set { DataContext = value; }
        }

        public DispatchActions DispatchActions { get; }
        public DispatchCommands DispatchCommands { get; }
        // abbr
        public DispatchCommands dc => DispatchCommands;

        /// <summary>
        ///     .ctor </summary>
        /// <param name="company">Current DAT Company</param>
        /// <param name="drivers">All DAT Drivers</param>
        /// <param name="loads">All Loads on the board</param>
        public DetailsCarrierDialog(
            DispatchService service,
            ICallService callService,
            IMapper mapper,
            DatCompanyViewModel company,
            IList<DatDriverViewModel> drivers,
            IList<LoadViewModel> loads,
            IList<ActualCompanyViewModel> actualCompanies,
            ActualCompanyViewModel actualCompany) : base(service, callService) {

            DispatchActions = new DispatchActions(this);
            DispatchCommands = new DispatchCommands(DispatchActions, this);

            _mapper = mapper;
            _helper = App.Services.GetRequiredService<HelperDataService>();
            _reportService = App.Services.GetRequiredService<ReportService>();

            InitializeComponent();

            Title = $"Company '{company.Name}' {company.McNumber}";

            Model = new DetailesCarrierViewModel(company, drivers, loads, actualCompanies, actualCompany);

            //var result = _helper.ReverseLocation(new ReverseLocationRequest() { Location = mapper.Map<LocationDto>(drivers.First().CurrentLocation) });

        }

        public override void BusyStart(string message = null) {
            loading.Visibility = Visibility.Visible;
            base.BusyStart(message);
        }

        public override void BusyFinish() {
            loading.Visibility = Visibility.Collapsed;
            base.BusyFinish();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            throw new System.NotImplementedException();
        }

        private void loadItemsControl_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            // TODO IMPLEMENT MULTILOAD FORM
            var dialog = new EditLoadDialog(_dispatchService, _callService, _mapper,
                ((Wrapper1LoadViewModel)((FrameworkElement)sender).DataContext).Load, Model.ActualCompanies, Model.ActualCompany);
            dialog.Show();
        }

        private void DatDriver_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount != 2) {
                return;
            }
            var model = (DatDriverViewModel)((FrameworkElement)sender).DataContext;
            var dialog = new EditDatDriverDialog(_dispatchService, model);
            if (dialog.ShowDialog() == true) {
                model.Assign(dialog.Model);
            }
        }

        private void driver_OnMouseDown(object sender, MouseButtonEventArgs e) {
            Model.SelectedLoads.Clear();
            Model.SelectedLoad = null;
        }

        private void CompanyName_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new EditCarrierDialog(_dispatchService, this.Model.DatCompany,
               this.Model.ActualCompany, Model.ActualCompanies);
            dialog.ShowDialog();
        }

        private void DatCompanyDetailsDialog_OnClosed(object sender, EventArgs e) {
        }

        private void btnLocation_OnMouseUp(object sender, MouseButtonEventArgs e) {
            var driver = ((FrameworkElement)sender).DataContext as DatDriverViewModel;
            if (driver != null) {
                EditLocationDialog dialog = new EditLocationDialog();
                dialog.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                    Location = driver.CurrentLocation
                });
                dialog.Initialize();
                if (dialog.ShowDialog() == true) {
                    this.BusyStart();
                    _dispatchService.LocationDatDriverChanged(
                        driver.EntityId,
                        dialog.Model.Location.Location,
                        LocationDatDriverChangedCompleted);
                }
            }
        }

        private async Task LocationDatDriverChangedCompleted(ViewModelWrapper<DatDriverViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LocationDatDriverChangedCompleted(wrapper));
                return;
            }
            await SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                var load = Model.DatCompany.Loads.FirstOrDefault(l => l.DatDriver != null && l.DatDriver.EntityId == wrapper.Data.EntityId);
                if (load == null) throw new Exception("Load Was Not Found");
                var datDriver = load.DatDriver;
                datDriver.Assign(wrapper.Data);
                Model.Reload();
            }
        }

        // TODO: REMOVE TO ACCOUNT BOARD
        private void CompleteLoadClicked(object sender, RoutedEventArgs e) {
            var load = (LoadViewModel)((FrameworkElement)sender).DataContext;
            if (MessageBox.Show(String.Format("Do you want to close load?\nLoad# : {0}", load.IndexString), "Close Load",
                    MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) {
                return;
            }
            load.Status = LoadStatus.Completed;

            this._dispatchService.SaveLoad(load, SaveLoad_Completed);
            BusyStart();
        }

        private void EditContacts_OnClicked(object sender, RoutedEventArgs e) {
            var dialog = new EditContactsDialog();
            dialog.ViewModel = (ContactsViewModel)Model.DatCompany.Contacts.Clone();
            if (dialog.ShowDialog() == true) {
                Model.DatCompany.Contacts = dialog.ViewModel;
                _dispatchService.SaveDatCompany(Model.DatCompany, SaveDatCompany_Completed);
            }
        }

        private void NextContacts_OnClicked(object sender, RoutedEventArgs e) {
            Model.DatCompany.Contacts.Next();
            _dispatchService.SaveDatCompany(Model.DatCompany, SaveDatCompany_Completed);
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
                    this._dispatchService.SaveLoad(load, SaveLoad_Completed);
                    BusyStart();
                }
            } else if (((FrameworkElement)sender).DataContext is DatCompanyViewModel) {
                var company = (DatCompanyViewModel)((FrameworkElement)sender).DataContext;
                editor.ViewModel = company.Comments;
                if (editor.ShowDialog() == true) {
                    company.Comments = editor.ViewModel;
                    this._dispatchService.SaveDatCompany(company, SaveDatCompany_Completed);
                    BusyStart();
                }
            } else if (((FrameworkElement)sender).DataContext is DatDriverViewModel) {
                var driver = (DatDriverViewModel)((FrameworkElement)sender).DataContext;
                editor.ViewModel = driver.Comments;
                if (editor.ShowDialog() == true) {
                    driver.Comments = editor.ViewModel;
                    this._dispatchService.SaveDatDriver(driver, SaveDatDriver_Completed);
                    BusyStart();
                }
            } else {
                throw new Exception("DataContext unrecognized. Type: " + ((FrameworkElement)sender).DataContext.GetType());
            }
        }

        /// <summary>
        ///     Clears selection </summary>
        private void Window_OnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource is FrameworkElement && ((FrameworkElement)e.OriginalSource).DataContext is LoadViewModel == false) {
                Model.SelectedLoads.Clear();
            }
        }

        public async Task SetLoads(List<LoadViewModel> loads) {
            await Model.SetLoads(loads);
        }

        public void OnCanExecuteChanged() {
            foreach (Command command in this.GetType().GetProperties().Where(e => e.PropertyType == typeof(Command)).Select(e => e.GetValue(this))) {
                command?.OnCanExecuteChanged();
            }
        }

        #region [ SaveCompleted ]

        public async Task SaveCompleted<T, F>(ViewModelWrapper<T> wrapper1, ViewModelWrapper<F> wrapper2) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await this.SaveCompleted(wrapper1, wrapper2));
                return;
            }
            if (wrapper1 is ViewModelWrapper<DatDriverViewModel> && wrapper2 is ViewModelWrapper<List<LoadViewModel>>) {
                await this.SaveDatDriverAndBindLoad_Completed(wrapper1 as ViewModelWrapper<DatDriverViewModel>, wrapper2 as ViewModelWrapper<List<LoadViewModel>>);
            } else
            if (wrapper1 is ViewModelWrapper<LoadViewModel> && wrapper2 is ViewModelWrapper<FileViewModel>) {
                await this.SaveLoad_Completed(wrapper1 as ViewModelWrapper<LoadViewModel>);
            } else {
                await base.SaveCompleted(wrapper1, wrapper2);
            }
        }

        public async Task SaveCompleted<T>(ViewModelWrapper<T> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await this.SaveCompleted(wrapper));
                return;
            }
            if (wrapper is ViewModelWrapper<DatDriverViewModel>) {
                await this.SaveDatDriver_Completed(wrapper as ViewModelWrapper<DatDriverViewModel>);
            } else if (wrapper is ViewModelWrapper<DatCompanyViewModel>) {
                await this.SaveDatCompany_Completed(wrapper as ViewModelWrapper<DatCompanyViewModel>);
            } else if (wrapper is ViewModelWrapper<LoadViewModel>) {
                await this.SaveLoad_Completed(wrapper as ViewModelWrapper<LoadViewModel>);
            } else {
                await base.SaveCompleted(wrapper);
            }
            OnCanExecuteChanged();
        }

        private async Task SaveDatDriverAndBindLoad_Completed(
            ViewModelWrapper<DatDriverViewModel> wrapper1,
            ViewModelWrapper<List<LoadViewModel>> wrapper2) {

            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveDatDriverAndBindLoad_Completed(wrapper1, wrapper2));
                return;
            }

            await base.SaveCompleted(wrapper1, false);
            if (wrapper1.Status == ResponseStatus.Success) {
                Model.SetDriver(wrapper1.Data);
            }

            await base.SaveCompleted(wrapper2, false);
            if (wrapper2.Status == ResponseStatus.Success) {
                await Model.SetLoads(wrapper2.Data);
            }
        }

        private async Task SaveLoad_Completed(ViewModelWrapper<LoadViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveLoad_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await Model.SetLoads(new List<LoadViewModel>() { wrapper.Data });
            }
            await base.SaveCompleted(wrapper, false);
            // Checks by empty
            if (Model.DatCompany.Loads.Any() == false) {
                if (DialogResult != true) {
                    DialogResult = true;
                };
            }
        }

        private async Task SaveDatCompany_Completed(ViewModelWrapper<DatCompanyViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveDatCompany_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                // assign + reload
                Model.DatCompany.Assign(wrapper.Data);
                Model.Reload();
            }
            await base.SaveCompleted(wrapper, false);
        }

        private async Task SaveDatDriver_Completed(ViewModelWrapper<DatDriverViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await SaveDatDriver_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                Model.SetDriver(wrapper.Data);
            }
            await base.SaveCompleted(wrapper, false);
        }

        private async Task BindLoads_Completed(ViewModelWrapper<List<LoadViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await BindLoads_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await Model.SetLoads(wrapper.Data);
            }
        }

        #endregion [ SaveCompleted ]
    }
}
