using AutoCall.Common.Collections;
using AutoDispatch.Logic.Converters;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Common.Helpers;
using AutoCall.Common.Loggers;
using AutoDispatch.Data;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Contracts.Settings.Sections;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.Logic.Settings.Sections;
using AutoCall.WPF.Converters.Dispatch;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using Application = System.Windows.Application;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;
using GridViewColumnCollection = Telerik.Windows.Controls.GridViewColumnCollection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using ResponseStatus = AutoDispatch.Dtos.Enums.ResponseStatus;

namespace AutoCall.WPF.UserControls.Boards {

    public abstract partial class DispatchAccountBoard : DispatchBoardBase {
        public override Slider ScaleSlider => this.uiScaleSlider;
        public override ScaleTransform ScaleTransform => this.scaleTransform;

        #region [ _fields ]

        private readonly CompositeFilterDescriptor compositeFilter;

        #endregion [ _fields ]

        #region [ Properties ]

        protected override LoadingControl loading1 => this.loading;

        #endregion [ Properties ]

        #region [ .ctor ]

        protected DispatchAccountBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings)
            : base(moduleName, boardType, manager, settings) {

            InitializeComponent();
            this.gridLoads.KeyboardCommandProvider = new CustomKeyboardCommandProvider(this.gridLoads);

            DataContext = Model;

            // Assigns Items Source
            this.dataPager.Source = ((CollectionViewSource)Resources["LoadsViewSource"]).View;
            // The Filter by board type
            gridLoads.FilterDescriptors.Add(new FilterDescriptor("Load.BoardType", FilterOperator.IsEqualTo, this.DispatchBoardType));
            // The Filter for history
            compositeFilter = new CompositeFilterDescriptor() { LogicalOperator = FilterCompositionLogicalOperator.Or };
            compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Load.Status", FilterOperator.IsEqualTo, LoadStatus.Unpaid));
            compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Load.Status", FilterOperator.IsEqualTo, LoadStatus.Completed));
            compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Load.Status", FilterOperator.IsEqualTo, LoadStatus.Canceled2));
            compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Load.IsDeleted", FilterOperator.IsEqualTo, true));

            tbLoadsSearch.AddHandler(KeyDownEvent, new KeyEventHandler(tbLoadFilter_OnKeyDown), true);
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        protected virtual void AddColumns() {
            {
                var col = gridLoads.CreateImageColumn("", "ActualCompany.LogoSmall");
                PropertyExtensions.SetRelativeDisplayIndex(col, 1);
                col.ColumnGroupName = "Load";
                col.IsResizable = false;
                col.Width = 30;
            }
            {
                var col = gridLoads.CreateTextColumn("Company Name", "ActualCompany.LegalName");
                PropertyExtensions.SetRelativeDisplayIndex(col, 2);
                col.ColumnGroupName = "Load";
                col.Width = 100;
            }

            pComment1Col.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Comment1);
            pComment2Col.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Comment2);
            pConsolidationCol.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Consolidation);
        }

        public override void InitializeGrids() {
            this.AddColumns();

            //BusyStart("Loading..."); // binding with Model.Loading will be broken
            loading1.LoadingMessage = "Loading...";

            switch (this.DispatchBoardType) {
                case DispatchBoardType.CarrierAccount:
                    base.CreateFilterBinding(this.itemsFilterRecords, DispatchFilterTableType.Accounting_Carriers);
                    break;
                case DispatchBoardType.BrokerAccount:
                    base.CreateFilterBinding(this.itemsFilterRecords, DispatchFilterTableType.Accounting_Brokers);
                    break;
            }

            Model.Loaded -= Model_Loaded;
            Model.Loaded += Model_Loaded;

            // *** Applies settings of columns AFTER after was their added ***
            base.InitializeGrids();
        }

        private void Model_Loaded() {
            // это обязательно?
            //((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
        }

        private void RadContextMenu_Opening(object sender, RadRoutedEventArgs e) {
            if ((RoleName)_session.Roles != RoleName.Admin) {
                if (((Wrapper1LoadViewModel)((FrameworkElement)sender).DataContext).Load.Parent1 != null) {
                    e.Handled = true;
                }
            }
        }

        #endregion [ Initialize ]

        #region [ Engine Events ]

        protected override async Task _engine_EventReceived(EventResponse response) {
            await base._engine_EventReceived(response);
            if (this.Dispatcher.CheckAccess() == true) {
                StackLogger.Push($"_engine_EventReceived");
                gridLoads.CalculateAggregates();
            }
        }

        #endregion [ Engine Events ]

        #region [ Settings ]

        public override void SetSettings(BoardSettings2 settings) {
            var tableName = this.BoardType == BoardType.DispatchBrokerAccount ?
                SettingsTableNames.Accounting_Brokers : SettingsTableNames.Accounting_Carriers;
            GridViewColumnsHelper.LoadAndBindSettingsViewModel(_mapper, tableName, settings.RecordColumnSettings2, gridLoads, Model.GridsSettings);
            GridViewColumnsHelper.ReorderGroupRelative(gridLoads);
            if (settings.RecordColumnSettings2?.SelectedFilterColumnsName != null)
                this.itemsFilterRecords.SelectedFilterName = settings.RecordColumnSettings2.SelectedFilterColumnsName;
        }

        public override BoardSettings2 GetSettings() {
            var tableName = this.BoardType == BoardType.DispatchBrokerAccount ?
                SettingsTableNames.Accounting_Brokers : SettingsTableNames.Accounting_Carriers;
            GridSettingsViewModel data2 = Model.GridsSettings.Grids[tableName];
            data2.SelectedFilterColumnsName = this.itemsFilterRecords.Model.SelectedFilter?.Name;
            return new BoardSettings2() {
                RecordColumnSettings2 = _mapper.Map<GridViewSettings>(data2),
            };
        }

        #endregion [ Settings ]

        #region [ Factoring Company ]

        /// <summary>
        ///     Cache </summary>
        private List<SetupPacketFactoringCompanyDto> FactoringCompanies = null;

        /*private async void cbFactoringCompany_Loaded(object sender, RoutedEventArgs args) {
            var comboBox = (RadComboBox)sender;
            StyleManager.SetTheme(comboBox, new VistaTheme());
            if (FactoringCompanies == null) {
                GetFactoringCompaniesResponse response = await App.Services.GetRequiredService<DatCompanyDataService>()
                    .GetFactoringCompanies(new GetFactoringCompaniesRequest());
                if (response.IsSuccessed) {
                    FactoringCompanies = response.Data;
                } else {
                    ((GridViewEditorPresenter)comboBox.Parent).IsContentValid = false;
                    ((GridViewEditorPresenter)comboBox.Parent).ErrorMessage = response.ErrorMessage;
                }
            }
            comboBox.IsEnabled = FactoringCompanies != null;
            comboBox.ItemsSource = FactoringCompanies;
        }*/

        #endregion [ Factoring Company ]

        #region [ SubLoads Loaded ]

        // 0.2sec
        private void ChildGridLoads_OnLoaded(object sender, RoutedEventArgs args) {
            var gridView = (RadGridView)sender;
            // Subscription doesn't worked in a markup
            ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).Filter -= SubLoadsViewSource_OnFilter;
            ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).Filter += SubLoadsViewSource_OnFilter;
            // Sets CollectionView
            gridView.ItemsSource = ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).View;
            ((ICollectionView)gridView.ItemsSource).Refresh();
            // NOTES: Selects last row on last subload
            SelectedItemsModel.SelectedLoads.Clear();
            SelectedItemsModel.SelectedPatterns.Clear();

            GridViewColumnCollection parentColumns = gridLoads.Columns;
            GridViewColumnCollection childColumns = gridView.Columns;

            // doesn't need to add columns if its exists
            if (gridView.Columns.Count > 0) {
                return;
            }

            gridView.RowStyle = new Style(typeof(GridViewRow), gridLoads.RowStyle);
            gridView.RowStyle.Setters.Add(new Setter(GridViewRow.IsExpandableProperty, false));
            gridView.HierarchyChildTemplate = gridLoads.HierarchyChildTemplate;

            // ViewModel of GridViewColumns
            GridSettingsViewModel parentGridViewSettingsViewModel =
                Model.GridsSettings.Grids[this.BoardType == BoardType.DispatchBrokerAccount ?
                SettingsTableNames.Accounting_Brokers : SettingsTableNames.Accounting_Carriers];

            foreach (GridViewColumn parentColumn in parentColumns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex)) {
                GridViewColumnsHelper.SyncChildColumn(parentColumn, parentGridViewSettingsViewModel, childColumns);
            }
        }

        #endregion [ SubLoads Loaded ]

        #region [ toolbar ]

        private void Export_Clicked(object sender, RoutedEventArgs args) {
            base.ExportAction(gridLoads, "autocall_accounting");

            Console.WriteLine($"{this.BoardType} {gridLoads.Name}");
            foreach (var column in gridLoads.Columns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex)) {
                Console.WriteLine($"{column.UniqueName} index:{column.DisplayIndex} width:{column.Width}");
            }
        }

        protected virtual void Factoring_Company_OnClick(object sender, RoutedEventArgs args) { }

        #endregion [ toolbar ]

        #region [ DoubleClick Events ]

        private bool _eventHandled = false;
        private void gridLoads_CellDoubleClick(object sender, RadRoutedEventArgs e) {
            var wrapper = (Wrapper1LoadViewModel)((FrameworkElement)e.Source).DataContext;
            var uniqueName = ((GridViewCellBase)e.Source).Column.UniqueName;
            switch (uniqueName) {
                case "Load.Comment1Text":
                    if (this.DispatchCommands.EditComments1Command.CanExecute(wrapper)) {
                        this.DispatchCommands.EditComments1Command.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.Comment2Text":
                    if (this.DispatchCommands.EditComments2Command.CanExecute(wrapper)) {
                        this.DispatchCommands.EditComments2Command.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.ConsolidationText":
                    if (this.DispatchCommands.EditConsolidationCommand.CanExecute(wrapper)) {
                        this.DispatchCommands.EditConsolidationCommand.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.InvoiceNumber":
                case "Load.CheckNumber":
                case "Load.BOLFileView":
                case "Load.PODFileView":
                    _eventHandled = true;
                    break;
                case "Load.ExpensesTotal":
                    if (wrapper.Load.Parent1 == null) {
                        if (this.DispatchCommands.EditExpensesCommand.CanExecute(wrapper)) {
                            this.DispatchCommands.EditExpensesCommand.Execute(wrapper);
                            _eventHandled = true;
                        }
                    }
                    break;
                case "Load.GainsTotal":
                    if (wrapper.Load.Parent1 == null) {
                        if (this.DispatchCommands.EditGainsCommand.CanExecute(wrapper)) {
                            this.DispatchCommands.EditGainsCommand.Execute(wrapper);
                            _eventHandled = true;
                        }
                    }
                    break;
                default:
                    // $"Load.ExpensesValue[{i}]", "Load.ExpensesComment[{i}]"
                    if (uniqueName.StartsWith("Load.ExpensesView") ||
                        uniqueName.StartsWith("Load.GainsView") ||
                        uniqueName.StartsWith("LinkedWrapper.Load.ExpensesView") ||
                        uniqueName.StartsWith("LinkedWrapper.Load.GainsView") ||
                        uniqueName.StartsWith("Load.QuikPay")) {
                        _eventHandled = true;
                    }
                    break;
            }
        }

        private void gridLoads_RowDoubleClick(object sender, MouseButtonEventArgs e) {
            // NOTE: the event triggers twice. Checks by one
            if ((e.OriginalSource as FrameworkContentElement)?.DataContext != ((FrameworkElement)e.Source).DataContext &&
                (e.OriginalSource as FrameworkElement)?.DataContext != ((FrameworkElement)e.Source).DataContext) {
                return;
            }
            if (_eventHandled == false) {
                var wrapper = (Wrapper1LoadViewModel)((FrameworkElement)e.Source).DataContext;
                this.DispatchActions.EditLoadAction(wrapper, true);
            }
            _eventHandled = false;
        }

        #endregion [ DoubleClick Events ]

        #region [ Filter ]

        protected override void Patterns_FilterColumns_SelectionChanged() { }
        protected override void Records_FilterColumns_SelectionChanged() {
            tbLoadsSearch.Text = String.Empty;
            ((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
        }

        // Loads
        protected virtual void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            var search = tbLoadsSearch.Text;
            DispatchFilterTableType filterTableType;
            switch (this.DispatchBoardType) {
                case DispatchBoardType.CarrierAccount:
                    filterTableType = DispatchFilterTableType.Accounting_Carriers;
                    break;
                case DispatchBoardType.BrokerAccount:
                    filterTableType = DispatchFilterTableType.Accounting_Brokers;
                    break;
                default:
                    throw new Exception();
            }
            FiltersColumnsViewModel filterColumns = Model.FiltersColumns[filterTableType];
            e.Accepted = base.ViewSource_OnFilter(search, filterColumns.SelectedFilter, (Wrapper1LoadViewModel)e.Item);
        }

        // SubLoads
        // NOTES: I spent 2 hours, but the usual subscription does not work.
        public static FilterEventHandler Static_SubLoadsViewSource_OnFilter;
        protected virtual void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            var search = tbLoadsSearch.Text;
            DispatchFilterTableType filterTableType;
            switch (this.DispatchBoardType) {
                case DispatchBoardType.CarrierAccount:
                    filterTableType = DispatchFilterTableType.Accounting_Carriers;
                    break;
                case DispatchBoardType.BrokerAccount:
                    filterTableType = DispatchFilterTableType.Accounting_Brokers;
                    break;
                default:
                    throw new Exception();
            }
            FiltersColumnsViewModel filterColumns = Model.FiltersColumns[filterTableType];
            e.Accepted = base.ViewSource_OnFilter(search, filterColumns.SelectedFilter, (Wrapper1LoadViewModel)e.Item);
        }

        private Task task = null;

        private void gridLoads_ClearFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            tbLoadsSearch.Text = String.Empty;
            ((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
        }

        protected async void tbLoadFilter_OnKeyDown(object sender, KeyEventArgs e) {
            var tb = (RadWatermarkTextBox)sender;
            if (e.Key == Key.Escape) {
                tb.Text = null;
                Keyboard.ClearFocus();
            } else {
                var tableName = this.BoardType == BoardType.DispatchBrokerAccount ?
                    DispatchFilterTableType.Accounting_Brokers : DispatchFilterTableType.Accounting_Carriers;
                Model.FiltersColumns[tableName].SelectedFilter = null;
                #region [ single request + delay ]
                var localTask = task = Task.Delay(1000);
                await task;
                if (localTask != task) {
                    return;
                }
                #endregion [ single request + delay ]
                ((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
            }
        }

        private DateTime historyLoading;

        private async void HistoryFilter_OnSelectionChanged(object sender, SelectionChangedEventArgs args) {
            var keyValue = new System.Tuple<string, int>("100", 100);

            historyLoading = DateTime.UtcNow;
            this.loading1.LoadingMessage = "Loading...";
            this.loading1.progressBar.IsIndeterminate = true;
            this.loading1.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(0.2));

            if (((RadComboBox)sender).SelectedValue != null) {
                // to load loads and trailers
                _dispatchService.LoadHistory(this.DispatchBoardType,
                    ((DateTime)((RadComboBox)sender).SelectedValue),
                    ((DateTime)((RadComboBox)sender).SelectedValue).AddMonths(3),
                    LoadHistory_Completed);

                gridLoads.FilterDescriptors.Add(compositeFilter);
                gridLoads.IsReadOnly = true;
            } else {
                // синхронно
                gridLoads.FilterDescriptors.Remove(compositeFilter);
                gridLoads.IsReadOnly = false;

                this.loading1.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadHistory_Completed(ViewModelWrapper<List<byte[]>> wrapper1, ViewModelWrapper<List<TrailerViewModel>> wrapper2) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadHistory_Completed");
                await Dispatcher.InvokeAsync(async () => await LoadHistory_Completed(wrapper1, wrapper2));
                return;
            }
            Debug.WriteLine($" --> History Loaded: {(int)(DateTime.UtcNow - historyLoading).TotalSeconds}sec");
            historyLoading = DateTime.UtcNow;

            if (wrapper1.Status == ResponseStatus.Success) {
                if (wrapper1.Status == ResponseStatus.Success) {
                    // Removes history loads from previous request
                    var previousLoads = DispatchBoardViewModel.Instance.Loads.Where(e =>
                        (e.Status == LoadStatus.Unpaid || e.Status == LoadStatus.Canceled2 || e.Status == LoadStatus.Completed)
                         && e.BoardType == DispatchBoardType).ToList();
                    DispatchBoardViewModel.Instance.Loads.RemoveRange(previousLoads);
                    GC.Collect();
                    // sets trailers
                    await Model.SetTrailersAsync(wrapper2.Data, false);
                    // sets loads and refresh
                    await Model.SetLoadsAsync(LoadConverter.ToVm, wrapper1.Data, true, true);
                    GC.Collect();
                } else {
                    MessageBoxEx.Error(wrapper1.MessageError);
                }
            } else {
                MessageBoxEx.Error(wrapper1.MessageError);
            }
            this.loading1.Visibility = Visibility.Hidden;
            Debug.WriteLine($" --> History Displayed: {(int)(DateTime.UtcNow - historyLoading).TotalSeconds}sec");
        }

        public override void SelectedBrokerCompany(ActualCompanyViewModel actualCompanyViewModel) {
            base.SelectedBrokerCompany(actualCompanyViewModel);
            ((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
        }

        public override void SelectedCarrierCompany(ActualCompanyViewModel actualCompanyViewModel) {
            base.SelectedCarrierCompany(actualCompanyViewModel);
            ((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
        }

        private void PageSize_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            dataPager.UpdateAllBindings();
        }

        #endregion [ Filter ]

        #region [ edit cell ]

        protected override async void grid_CellEditEnded(object sender, GridViewCellEditEndedEventArgs args) {
            if (args.EditAction == GridViewEditAction.Cancel) {
                return;
            }

            var wrapper = (Wrapper1LoadViewModel)args.Cell.DataContext;
            var clone = (LoadViewModel)wrapper.Load.Clone();

            if (this.DispatchActions.CheckParent(clone) == false) {
                ((RadGridView)sender).CancelEdit();
                return;
            }

            if (args.EditingElement is RadDatePicker) {
                _dispatchService.SaveLoad(clone, SaveCompleted);
                return;
            }

            // Inputted invoice
            if (args.Cell.Column.UniqueName == "Load.InvoiceNumber") {
                if (clone.BoardType != DispatchBoardType.BrokerAccount) {
                    return;
                }
                if ((string)args.OldData == clone.InvoiceNumber && invoiceFileName == null) {
                    return;
                }
                if (clone.InvoiceNumber?.Length != 0) {
                    clone.InvoiceDate = DateTime.Now;
                    if (invoiceFileName != null) {
                        clone.InvoiceFile = Path.GetFileName(invoiceFileName);
                        var fileViewModel = new FileViewModel() {
                            EntityId = clone.EntityId,
                            CreatedDate = DateTime.Now,
                            Name = Path.GetFileName(invoiceFileName),
                            Data = File.ReadAllBytes(invoiceFileName),
                        };
                        invoiceFileName = null;
                        clone.Status = SetLoadStatus(clone);
                        if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                            _dispatchService.SaveLoadAndAttachFile(clone, fileViewModel, SaveCompleted);
                        }
                    } else {
                        clone.Status = SetLoadStatus(clone);
                        _dispatchService.SaveLoad(clone, SaveCompleted);
                    }
                } else {
                    clone.InvoiceFile = null;
                    clone.InvoiceNumber = null;
                    clone.InvoiceDate = null;
                    _dispatchService.SaveLoad(clone, SaveCompleted);
                }
            }

            // Inputted check
            if (args.Cell.Column.UniqueName == "Load.CheckNumber") {
                if ((string)args.OldData == clone.CheckNumber && checkFileName == null) {
                    return;
                }
                if (clone.CheckNumber?.Length != 0) {
                    clone.CheckDate = DateTime.Now;
                    if (checkFileName != null) {
                        clone.CheckFile = Path.GetFileName(checkFileName);
                        var fileViewModel = new FileViewModel() {
                            EntityId = clone.EntityId,
                            CreatedDate = DateTime.Now,
                            Name = Path.GetFileName(checkFileName),
                            Data = File.ReadAllBytes(checkFileName),
                        };
                        checkFileName = null;
                        clone.Status = SetLoadStatus(clone);
                        if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                            _dispatchService.SaveLoadAndAttachFile(clone, fileViewModel, SaveCompleted);
                        }
                    } else {
                        clone.Status = SetLoadStatus(clone);
                        _dispatchService.SaveLoad(clone, SaveCompleted);
                    }
                } else {
                    clone.CheckFile = null;
                    clone.CheckNumber = null;
                    clone.CheckDate = null;
                    _dispatchService.SaveLoad(clone, SaveCompleted);
                }
            }

            // Inputted POD
            if (args.Cell.Column.UniqueName == "Load.PODFileView") {
                if (clone.BoardType != DispatchBoardType.CarrierAccount) {
                    return;
                }
                if ((string)args.OldData == clone.PODFile && PODFileName == null) {
                    return;
                }
                clone.PODDate = DateTime.Now;
                if (PODFileName != null) {
                    clone.PODFile = Path.GetFileName(PODFileName);
                    var fileViewModel = new FileViewModel() {
                        EntityId = clone.EntityId,
                        CreatedDate = DateTime.Now,
                        Name = Path.GetFileName(PODFileName),
                        Data = File.ReadAllBytes(PODFileName),
                    };
                    PODFileName = null;
                    clone.Status = SetLoadStatus(clone);
                    if (FileSizeHelper.Check20MBSize(fileViewModel.Data)) {
                        _dispatchService.SaveLoadAndAttachFile(clone, fileViewModel, SaveCompleted);
                    }
                } else {
                    clone.Status = SetLoadStatus(clone);
                    _dispatchService.SaveLoad(clone, SaveCompleted);
                }
            }

            // Inputted payout
            if (args.Cell.Column.UniqueName == "Load.Payout") {
                if ((float?)args.OldData == clone.Payout/* && fileName == null*/) {
                    return;
                }
                clone.Status = SetLoadStatus(clone);
                _dispatchService.SaveLoad(clone, SaveCompleted);
            }

            // Factoring Company
            if (args.Cell.Column.UniqueName == "SetupPacket.MailingCompanyLegalName") {
                if (wrapper.SetupPacket.FactoringCompanyId != null) {
                    var comp = FactoringCompanies.First(e => e.EntityId == wrapper.SetupPacket.FactoringCompanyId);
                    wrapper.SetupPacket.UseFactoringCompany = true;
                    wrapper.SetupPacket.MailingCompanyLegalName = comp.Name;
                    wrapper.SetupPacket.MailingAddress = comp.Address;
                    wrapper.SetupPacket.MailingCity = comp.City;
                    wrapper.SetupPacket.MailingStateId = comp.StateId;
                    wrapper.SetupPacket.MailingZipCode = comp.ZipCode;
                    wrapper.SetupPacket.MailingCountryId = comp.CountryId;
                }
                _dispatchService.SaveSetupPacket(wrapper.SetupPacket, SaveCompleted);
            }
            // Selected custom status
            if (args.Cell.Column.UniqueName == "Load.CustomStatusId") {
                if (wrapper.Load.CustomStatusId != (Guid?)args.OldData) {
                    _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
                }
            }

            base.grid_CellEditEnded(sender, args);


            await Task.Delay(TimeSpan.FromSeconds(1));
            ((CollectionViewSource)Resources["LoadsViewSource"]).View.Refresh();
        }

        private LoadStatus SetLoadStatus(LoadViewModel load) {
            if (load.Status == LoadStatus.Bond || load.Status == LoadStatus.Issues || load.Status == LoadStatus.Canceled1) {
                return load.Status;
            }
            if (load.BoardType == DispatchBoardType.BrokerAccount) {
                if (load.Paid) {
                    return LoadStatus.Completed;
                } else {
                    return load.Status;
                }
            }
            if (load.BoardType == DispatchBoardType.CarrierAccount) {
                if (load.Paid) {
                    return LoadStatus.Completed;
                } else {
                    return load.Status;
                }
            }

            throw new Exception();
        }

        // TODO: Implement observable dictionary
        //private void CheckHistoryFilters(List<LoadViewModel> loads) {
        //    var load = loads.FirstOrDefault(l => l.Status == LoadStatus.Completed);
        //    if (load != null) {
        //        CheckHistoryFilters(load);
        //    }
        //}

        //private void CheckHistoryFilters(LoadViewModel load) {
        //    if (load.Status == LoadStatus.Completed) {
        //        if (load.BoardType == DispatchBoardType.Broker || load.BoardType == DispatchBoardType.BrokerAccount) {
        //            _dispatchService.LoadTotalHistoryLoads(load.BoardType, LoadBrokerTotalHistoryLoads_Completed);
        //        }
        //        if (load.BoardType == DispatchBoardType.Carrier || load.BoardType == DispatchBoardType.CarrierAccount) {
        //            _dispatchService.LoadTotalHistoryLoads(load.BoardType, LoadCarrierTotalHistoryLoads_Completed);
        //        }
        //    }
        //}

        private string checkFileName = null, invoiceFileName = null, PODFileName = null;

        private void OpenHyperlinkFile_OnClick(object sender, RoutedEventArgs e) {

        }

        private void browse_OnClick(object sender, RoutedEventArgs e) {
            var cell = UIElementHelper.FindVisualParent<GridViewCell>((UIElement)sender);
            if (cell == null) return;
            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true) {
                if (cell.Column.UniqueName == "Load.CheckNumber") {
                    checkFileName = dialog.FileName;
                } else
                if (cell.Column.UniqueName == "Load.InvoiceNumber") {
                    invoiceFileName = dialog.FileName;
                } else
                if (cell.Column.UniqueName == "Load.PODFileView") {
                    PODFileName = dialog.FileName;
                }
            } else {
                if (cell.Column.UniqueName == "Load.CheckNumber") {
                    checkFileName = null;
                } else
                if (cell.Column.UniqueName == "Load.InvoiceNumber") {
                    invoiceFileName = null;
                } else
                if (cell.Column.UniqueName == "Load.PODFileView") {
                    PODFileName = null;
                }
            }
        }

        #endregion [ edit cell ]

        #region [ Reordering columns ]

        private void gridLoads_ColumnReordering(object sender, ColumnReorderingEventArgs args) {
            var tableName = this.BoardType == BoardType.DispatchBrokerAccount ?
                SettingsTableNames.Accounting_Brokers : SettingsTableNames.Accounting_Carriers;

            GridViewColumnsHelper.ReorderGroup((RadGridView)sender, args);
            GridViewColumnsHelper.ReorderGroupRelative((RadGridView)sender);
            args.Cancel = true;
        }

        #endregion [ Reordering columns ]
    }
}
