using AutoCall.Common;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
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
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;
using GridViewColumnCollection = Telerik.Windows.Controls.GridViewColumnCollection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AutoCall.WPF.UserControls.Boards {
    public abstract partial class DispatchBoard : DispatchBoardBase {

        public override Slider ScaleSlider => this.uiScaleSlider;
        public override ScaleTransform ScaleTransform => this.scaleTransform;

        #region [ protected ]

        protected override LoadingControl loading1 => this.loading;

        #endregion [ protected ]

        #region [ .ctor ]

        protected DispatchBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings)
            : base(moduleName, boardType, manager, settings) {

            InitializeComponent();
            this.gridPatterns.KeyboardCommandProvider = new CustomKeyboardCommandProvider(this.gridPatterns);
            this.gridLoads.KeyboardCommandProvider = new CustomKeyboardCommandProvider(this.gridLoads);

            DataContext = Model;

            // Assigns Items Source
            gridPatterns.ItemsSource = ((CollectionViewSource)Resources["PatternsViewSource"]).View;
            gridLoads.ItemsSource = ((CollectionViewSource)Resources["LoadsViewSource"]).View;

            tbPatternsSearch.AddHandler(KeyDownEvent, new KeyEventHandler(tbPatternFilter_OnKeyDown), true);
            tbLoadsSearch.AddHandler(KeyDownEvent, new KeyEventHandler(tbLoadFilter_OnKeyDown), true);

            var session = UserSession.Current;
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == false) {
                this.bDrivers.Visibility =
                    this.bEquipments.Visibility =
                    this.bWarehouse.Visibility =
                    this.bYards.Visibility =
                    this.sCatalogs.Visibility =
                        Visibility.Collapsed;
            }

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
            {
                var col = gridPatterns.CreateImageColumn("", "ActualCompany.LogoSmall");
                PropertyExtensions.SetRelativeDisplayIndex(col, 1);
                col.ColumnGroupName = "Load";
                col.IsResizable = false;
                col.Width = 30;
            }
            {
                var col = gridPatterns.CreateTextColumn("Company Name", "ActualCompany.LegalName");
                PropertyExtensions.SetRelativeDisplayIndex(col, 2);
                col.ColumnGroupName = "Load";
                col.Width = 100;
            }

            var session = UserSession.Current;
#if !DEBUG
            // аудит D19: кнопка разработчика — только в Debug-сборке; V1 — пользователь pilipenko.anton@ngs.ru
            btnTest.Visibility = Visibility.Collapsed;
#endif

            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == false) {
                // Warehouse Condition
                gridPatterns.Columns.Remove(p01); // LineHaul
                gridPatterns.Columns.Remove(p02); // Rate
                gridPatterns.Columns.Remove(p03); // Rate Change
                gridPatterns.Columns.Remove(gridPatterns_DatRate); // DAT Rate
                gridLoads.Columns.Remove(r04); // Line Haul
                gridLoads.Columns.Remove(r05); // Rate
                gridLoads.Columns.Remove(gridLoads_DatRate); // DAT Rate
            }

            pComment1Col.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Comment1);
            pComment2Col.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Comment2);
            pConsolidationCol.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Consolidation);

            rComment1Col.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Comment1);
            rComment2Col.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Comment2);
            rConsolidationCol.Header = base.GetCommentHeaderText(DispatchBoard_BoardToCommentNameConverter.CommentType.Consolidation);
        }

        public override void InitializeGrids() {
            this.AddColumns();

            //BusyStart("Loading..."); // binding with Model.Loading will be broken
            loading1.LoadingMessage = "Loading...";

            switch (this.DispatchBoardType) {
                case DispatchBoardType.Carrier:
                    base.CreateFilterBinding(this.itemsFilterPatterns, DispatchFilterTableType.Carrier_Patterns);
                    base.CreateFilterBinding(this.itemsFilterRecords, DispatchFilterTableType.Carrier_Records);
                    break;
                case DispatchBoardType.Broker:
                    base.CreateFilterBinding(this.itemsFilterPatterns, DispatchFilterTableType.Broker_Patterns);
                    base.CreateFilterBinding(this.itemsFilterRecords, DispatchFilterTableType.Broker_Records);
                    break;
            }

            Model.Loaded -= Model_Loaded;
            Model.Loaded += Model_Loaded;

            // *** Applies settings of columns AFTER after was their added ***
            base.InitializeGrids();

        }

        private void Model_Loaded() {
            // это обязательно?
            //if (this.Dispatcher.Thread.IsAlive) {
            //    ((ICollectionView)this.gridPatterns.ItemsSource).Refresh();
            //    ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
            //}
        }

        #endregion [ Initialize ]

        #region [ Settings ]

        public override void SetSettings(BoardSettings2 settings) {
            var tablePatterns = this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Patterns : SettingsTableNames.Broker_Patterns;
            var tableRecords = this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Records : SettingsTableNames.Broker_Records;

            GridViewColumnsHelper.LoadAndBindSettingsViewModel(_mapper, tablePatterns, settings.PatternColumnSettings2, gridPatterns, Model.GridsSettings);
            GridViewColumnsHelper.LoadAndBindSettingsViewModel(_mapper, tableRecords, settings.RecordColumnSettings2, gridLoads, Model.GridsSettings);

            GridViewColumnsHelper.ReorderGroupRelative(gridPatterns);
            GridViewColumnsHelper.ReorderGroupRelative(gridLoads);

            if (settings.PatternColumnSettings2?.SelectedFilterColumnsName != null)
                this.itemsFilterPatterns.SelectedFilterName = settings.PatternColumnSettings2.SelectedFilterColumnsName;
            if (settings.RecordColumnSettings2?.SelectedFilterColumnsName != null)
                this.itemsFilterRecords.SelectedFilterName = settings.RecordColumnSettings2.SelectedFilterColumnsName;
        }

        public override BoardSettings2 GetSettings() {
            var tablePatterns = this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Patterns : SettingsTableNames.Broker_Patterns;
            var tableRecords = this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Records : SettingsTableNames.Broker_Records;

            GridSettingsViewModel data1 = Model.GridsSettings.Grids[tablePatterns];
            GridSettingsViewModel data2 = Model.GridsSettings.Grids[tableRecords];

            data1.SelectedFilterColumnsName = this.itemsFilterPatterns.Model.SelectedFilter?.Name;
            data2.SelectedFilterColumnsName = this.itemsFilterRecords.Model.SelectedFilter?.Name;

            return new BoardSettings2() {
                PatternColumnSettings2 = _mapper.Map<GridViewSettings>(data1),
                RecordColumnSettings2 = _mapper.Map<GridViewSettings>(data2),
            };
        }

        #endregion [ Settings ]

        #region [ SubLoads Loaded ]

        private void childGridLoads_OnLoaded(object sender, RoutedEventArgs args) {
            var gridView = (RadGridView)sender;
            // Subscription doesn't worked in a markup
            ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).Filter -= SubLoadsViewSource_OnFilter;
            ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).Filter += SubLoadsViewSource_OnFilter;
            // Sets CollectionView
            gridView.ItemsSource = ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).View;
            ((ICollectionView)gridView.ItemsSource).Refresh();
            // NOTES: Selects last row on last subload
            this.SelectedItemsModel.SelectedLoads.Clear();
            this.SelectedItemsModel.SelectedPatterns.Clear();

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
            GridSettingsViewModel parentGridViewSettingsViewModel = Model.GridsSettings
                .Grids[this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Records : SettingsTableNames.Broker_Records];

            foreach (GridViewColumn parentColumn in parentColumns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex)) {
                GridViewColumnsHelper.SyncChildColumn(parentColumn, parentGridViewSettingsViewModel, childColumns);
            }
        }

        private void childGridPatterns_OnLoaded(object sender, RoutedEventArgs args) {
            var gridView = (RadGridView)sender;
            // Subscription doesn't worked in a markup
            ((CollectionViewSource)gridView.Resources["SubPatternsViewSource"]).Filter -= SubPatternsViewSource_OnFilter;
            ((CollectionViewSource)gridView.Resources["SubPatternsViewSource"]).Filter += SubPatternsViewSource_OnFilter;
            // Sets CollectionView
            gridView.ItemsSource = ((CollectionViewSource)gridView.Resources["SubPatternsViewSource"]).View;
            ((ICollectionView)gridView.ItemsSource).Refresh();
            // NOTES: Selects last row on last subload
            this.SelectedItemsModel.SelectedLoads.Clear();
            this.SelectedItemsModel.SelectedPatterns.Clear();

            GridViewColumnCollection parentColumns = gridPatterns.Columns;
            GridViewColumnCollection childColumns = gridView.Columns;

            // doesn't need to add columns if its exists
            if (gridView.Columns.Count > 0) {
                return;
            }

            gridView.RowStyle = new Style(typeof(GridViewRow), gridPatterns.RowStyle);
            gridView.RowStyle.Setters.Add(new Setter(GridViewRow.IsExpandableProperty, false));
            gridView.HierarchyChildTemplate = gridPatterns.HierarchyChildTemplate;

            // ViewModel of GridViewColumns
            GridSettingsViewModel parentGridViewSettingsViewModel = Model.GridsSettings.Grids[this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Patterns : SettingsTableNames.Broker_Patterns];

            foreach (GridViewColumn parentColumn in parentColumns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex)) {
                GridViewColumnsHelper.SyncChildColumn(parentColumn, parentGridViewSettingsViewModel, childColumns);
            }
        }

        #endregion [ SubLoads Loaded ]

        #region [ toolbar ]

        protected abstract void ExportPatterns_Clicked(object sender, RoutedEventArgs e);

        protected abstract void ExportLoads_Clicked(object sender, RoutedEventArgs e);

        private void Warehouses_Clicked(object sender, RoutedEventArgs args) {
            var exists = Application.Current.Windows.OfType<WarehousesDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new WarehousesDialog(_dispatchService, _settings,
                    DispatchBoardViewModel.Instance.ActualCompanies.FirstOrDefault(e => String.IsNullOrEmpty(e.BnsfLogin) == false));
                dialog.Show();
            }
        }

        private void Yards_Clicked(object sender, RoutedEventArgs args) {
            var exists = Application.Current.Windows.OfType<YardsDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new YardsDialog(_dispatchService, _settings);
                dialog.Show();
            }
        }

        private void Equipments_Clicked(object sender, RoutedEventArgs args) {
            var exists = Application.Current.Windows.OfType<EquipmentsDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new EquipmentsDialog(_dispatchService, _settings,
                    DispatchBoardViewModel.Instance.ActualCompanies.FirstOrDefault(e => String.IsNullOrEmpty(e.BnsfLogin) == false));
                dialog.Show();
            }
        }

        private void Drivers_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<DriversDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new DriversDialog(_dispatchService, _settings);
                dialog.Show();
            }
        }

        private void BrokersCompanies_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<CustomerCompaniesDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new CustomerCompaniesDialog(_dispatchService, _settings, Model.SelectedBrokerCompany);
                dialog.Show();
            }
        }

        private void ShipperReceiverCompanies_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<ShipperReceiverCompaniesDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new ShipperReceiverCompaniesDialog(_dispatchService, _settings, Model.SelectedBrokerCompany);
                dialog.Show();
            }
        }

        #endregion [ toolbar ]

        #region [ DoubleClick Events ]

        private bool _eventHandled = false;
        protected void gridPatterns_CellDoubleClick(object sender, RadRoutedEventArgs e) {
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
                case "Load.CurrentLocation.FormattedAddress":
                    if (this.DispatchCommands.ChangeLocationCommand.CanExecute(wrapper)) {
                        this.DispatchCommands.ChangeLocationCommand.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.LineHaul":
                    if (this.DispatchCommands.EditLineHaulCommand.CanExecute(wrapper)) {
                        this.DispatchCommands.EditLineHaulCommand.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.ConfirmationFileView":
                    if (this.DispatchCommands.OpenConfirmationCommand.CanExecute(wrapper)) {
                        this.DispatchCommands.OpenConfirmationCommand.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.BOLFileView":
                    if (this.DispatchCommands.OpenBOLCommand.CanExecute(wrapper)) {
                        this.DispatchCommands.OpenBOLCommand.Execute(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.PODFileView":
                    if (this.DispatchCommands.OpenPODCommand.CanExecute(wrapper)) {
                        this.DispatchCommands.OpenPODCommand.Execute(wrapper);
                        _eventHandled = true;
                    }
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

        protected void gridLoads_CellDoubleClick(object sender, RadRoutedEventArgs e) {
            var wrapper = (Wrapper1LoadViewModel)((FrameworkElement)e.Source).DataContext;
            var uniqueName = ((GridViewCellBase)e.Source).Column.UniqueName;
            switch (uniqueName) {
                case "Load.Comment1Text":
                    this.DispatchActions.EditComments1Action(wrapper);
                    _eventHandled = true;
                    break;
                case "Load.Comment2Text":
                    this.DispatchActions.EditComments2Action(wrapper);
                    _eventHandled = true;
                    break;
                case "Load.ConsolidationText":
                    this.DispatchActions.EditConsolidationAction(wrapper);
                    _eventHandled = true;
                    break;
                case "Load.Index":
                case "Load.Status":
                case "Load.CustomStatusId":
                case "Load.Pallets":
                    _eventHandled = true;
                    break;
                case "Load.ConfirmationFileView":
                    this.DispatchActions.OpenConfirmationAction(wrapper);
                    _eventHandled = true;
                    break;
                case "Load.BOLFileView":
                    this.DispatchActions.OpenBOLAction(wrapper);
                    _eventHandled = true;
                    break;
                case "Load.PODFileView":
                    this.DispatchActions.OpenPODAction(wrapper);
                    _eventHandled = true;
                    break;
                case "Load.ExpensesTotal":
                    if (wrapper.Load.Parent1 == null) {
                        this.DispatchActions.EditExpensesAction(wrapper);
                        _eventHandled = true;
                    }
                    break;
                case "Load.GainsTotal":
                    if (wrapper.Load.Parent1 == null) {
                        this.DispatchActions.EditGainsAction(wrapper);
                        _eventHandled = true;
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

        private void gridPatterns_RowDoubleClick(object sender, MouseButtonEventArgs e) {
            // NOTE: the event triggers twice. Checks by one
            if ((e.OriginalSource as FrameworkContentElement)?.DataContext != ((FrameworkElement)e.Source)?.DataContext &&
                (e.OriginalSource as FrameworkElement)?.DataContext != ((FrameworkElement)e.Source)?.DataContext) {
                return;
            }
            if (_eventHandled == false) {
                var wrapper = (Wrapper1LoadViewModel)((FrameworkElement)e.Source).DataContext;
                if (BoardType == BoardType.DispatchCarrier) {
                    if ((AutoDispatch.Logic.Helpers.DispatchHelper.GetTrailerWrapper(wrapper) is var w) && w.Trailer != null) {
                        this.DispatchActions.ViewTrailerAction(w);
                    } else if (wrapper.Warehouse != null) {
                        MessageBox.Show("this cargo is in the warehouse\nuse context menu to load into trailer or to move to accounting");
                    } else {
                        this.DispatchActions.EditLoadAction(wrapper);
                    }
                }
                if (BoardType == BoardType.DispatchBroker) {
                    if ((AutoDispatch.Logic.Helpers.DispatchHelper.GetDatCompanyWrapper(wrapper) is var w) && w.DatCompany != null) {
                        this.DispatchActions.ViewCarrierAction(w);
                    } else {
                        this.DispatchActions.EditLoadAction(wrapper);
                    }
                }
            }
            _eventHandled = false;
        }

        private void gridLoads_RowDoubleClick(object sender, MouseButtonEventArgs e) {
            // NOTE: the event triggers twice. Checks by one
            if ((e.OriginalSource as FrameworkContentElement)?.DataContext != ((FrameworkElement)e.Source).DataContext &&
                (e.OriginalSource as FrameworkElement)?.DataContext != ((FrameworkElement)e.Source).DataContext) {
                return;
            }
            if (_eventHandled == false) {
                var wrapper = (Wrapper1LoadViewModel)((FrameworkElement)e.Source).DataContext;
                this.DispatchActions.EditLoadAction(wrapper);
            }
            _eventHandled = false;
        }

        #endregion [ DoubleClick Events ]

        #region [ Filter ]

        protected override void Patterns_FilterColumns_SelectionChanged() {
            try {
                tbPatternsSearch.Text = String.Empty;
                ((ICollectionView)this.gridPatterns.ItemsSource).Refresh();
            } catch (Exception) {
            }
        }

        protected override void Records_FilterColumns_SelectionChanged() {
            try {
                tbLoadsSearch.Text = String.Empty;
                ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
            } catch (Exception) {
            }
        }

        // Patterns
        protected virtual void PatternsViewSource_OnFilter(object sender, FilterEventArgs e) {
            try {
                var search = tbPatternsSearch.Text;
                DispatchFilterTableType filterTableType;
                switch (this.DispatchBoardType) {
                    case DispatchBoardType.Carrier:
                        filterTableType = DispatchFilterTableType.Carrier_Patterns;
                        break;
                    case DispatchBoardType.Broker:
                        filterTableType = DispatchFilterTableType.Broker_Patterns;
                        break;
                    default:
                        throw new Exception();
                }
                FiltersColumnsViewModel filterColumns = Model.FiltersColumns[filterTableType];
                e.Accepted = base.ViewSource_OnFilter(search, filterColumns.SelectedFilter, (Wrapper1LoadViewModel)e.Item);
            } catch (NullReferenceException ex) {
                // Debugging Exception!!
                if (tbPatternsSearch == null) {
                    _webl.e("tbPatternsSearch == null");
                } else if (e == null) {
                    _webl.e("e == null");
                } else if (((Wrapper1LoadViewModel)e.Item)?.ActualCompany == null) {
                    _webl.e("wrapper.ActualCompany == null");
                }
                throw;
            }
        }

        // SubPatterns
        protected virtual void SubPatternsViewSource_OnFilter(object sender, FilterEventArgs e) {
            try {
                var search = tbPatternsSearch.Text;
                DispatchFilterTableType filterTableType;
                switch (this.DispatchBoardType) {
                    case DispatchBoardType.Carrier:
                        filterTableType = DispatchFilterTableType.Carrier_Patterns;
                        break;
                    case DispatchBoardType.Broker:
                        filterTableType = DispatchFilterTableType.Broker_Patterns;
                        break;
                    default:
                        throw new Exception();
                }
                FiltersColumnsViewModel filterColumns = Model.FiltersColumns[filterTableType];
                e.Accepted = base.ViewSource_OnFilter(search, filterColumns.SelectedFilter, (Wrapper1LoadViewModel)e.Item);
            } catch (NullReferenceException ex) {
                // Debugging Exception!!
                if (tbPatternsSearch == null) {
                    _webl.e("tbPatternsSearch == null");
                } else if (e == null) {
                    _webl.e("e == null");
                } else if (((Wrapper1LoadViewModel)e.Item)?.ActualCompany == null) {
                    _webl.e("wrapper.ActualCompany == null");
                }
                throw;
            }
        }

        // Loads
        protected virtual void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            var search = tbLoadsSearch.Text;
            DispatchFilterTableType filterTableType;
            switch (this.DispatchBoardType) {
                case DispatchBoardType.Carrier:
                    filterTableType = DispatchFilterTableType.Carrier_Records;
                    break;
                case DispatchBoardType.Broker:
                    filterTableType = DispatchFilterTableType.Broker_Records;
                    break;
                default:
                    throw new Exception();
            }
            FiltersColumnsViewModel filterColumns = Model.FiltersColumns[filterTableType];
            e.Accepted = base.ViewSource_OnFilter(search, filterColumns.SelectedFilter, (Wrapper1LoadViewModel)e.Item);
        }

        // SubLoads
        protected virtual void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            var search = tbLoadsSearch.Text;
            DispatchFilterTableType filterTableType;
            switch (this.DispatchBoardType) {
                case DispatchBoardType.Carrier:
                    filterTableType = DispatchFilterTableType.Carrier_Records;
                    break;
                case DispatchBoardType.Broker:
                    filterTableType = DispatchFilterTableType.Broker_Records;
                    break;
                default:
                    throw new Exception();
            }
            FiltersColumnsViewModel filterColumns = Model.FiltersColumns[filterTableType];
            e.Accepted = base.ViewSource_OnFilter(search, filterColumns.SelectedFilter, (Wrapper1LoadViewModel)e.Item);
        }

        private Task task = null;

        private void gridPatterns_ClearFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            tbPatternsSearch.Text = String.Empty;
            ((ICollectionView)this.gridPatterns.ItemsSource).Refresh();
        }

        private async void tbLoadFilter_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                ((RadWatermarkTextBox)sender).Text = null;
                Keyboard.ClearFocus();
            } else {
                var tableName = this.BoardType == BoardType.DispatchBroker ?
                    DispatchFilterTableType.Broker_Records : DispatchFilterTableType.Carrier_Records;
                Model.FiltersColumns[tableName].SelectedFilter = null;
                #region [ single request + delay ]
                var localTask = task = Task.Delay(1000);
                await task;
                if (localTask != task) {
                    return;
                }
                #endregion [ single request + delay ]
                ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
            }
        }

        private void gridLoads_ClearFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            tbLoadsSearch.Text = String.Empty;
            ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
        }

        private async void tbPatternFilter_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                ((RadWatermarkTextBox)sender).Text = null;
                Keyboard.ClearFocus();
            } else {
                var tableName = this.BoardType == BoardType.DispatchBroker ?
                    DispatchFilterTableType.Broker_Patterns : DispatchFilterTableType.Carrier_Patterns;
                Model.FiltersColumns[tableName].SelectedFilter = null;
                #region [ single request + delay ]
                var localTask = task = Task.Delay(1000);
                await task;
                if (localTask != task) {
                    return;
                }
                #endregion [ single request + delay ]
                ((ICollectionView)this.gridPatterns.ItemsSource).Refresh();
            }
        }

        public override void SelectedBrokerCompany(ActualCompanyViewModel actualCompanyViewModel) {
            base.SelectedBrokerCompany(actualCompanyViewModel);
            ((ICollectionView)this.gridPatterns.ItemsSource).Refresh();
            ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
        }

        public override void SelectedCarrierCompany(ActualCompanyViewModel actualCompanyViewModel) {
            base.SelectedCarrierCompany(actualCompanyViewModel);
            ((ICollectionView)this.gridPatterns.ItemsSource).Refresh();
            ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
        }

        #endregion [ Filter ]

        #region [ Reordering columns ]

        private void gridPatterns_ColumnReordering(object sender, ColumnReorderingEventArgs args) {
            var tableName = this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Patterns : SettingsTableNames.Broker_Patterns;

            GridViewColumnsHelper.ReorderGroup((RadGridView)sender, args);
            GridViewColumnsHelper.ReorderGroupRelative((RadGridView)sender);
            args.Cancel = true;
        }

        private void gridLoads_ColumnReordering(object sender, ColumnReorderingEventArgs args) {
            var tableName = this.BoardType == BoardType.DispatchCarrier ?
                SettingsTableNames.Carrier_Records : SettingsTableNames.Broker_Records;

            GridViewColumnsHelper.ReorderGroup((RadGridView)sender, args);
            GridViewColumnsHelper.ReorderGroupRelative((RadGridView)sender);
            args.Cancel = true;
        }

        #endregion [ Reordering columns ]

        #region [ edit cell ]

        protected override void CellEditEnded(Wrapper1LoadViewModel wrapper, GridViewCellEditEndedEventArgs args) {
            base.CellEditEnded(wrapper, args);
            // Typed Index
            if (args.Cell.Column.UniqueName == "Load.Index") {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            // Selected status
            if (args.Cell.Column.UniqueName == "Load.Status") {
                if (wrapper.Load.Status != (LoadStatus)args.OldData) {
                    _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
                }
            }
            // Selected custom status
            if (args.Cell.Column.UniqueName == "Load.CustomStatusId") {
                if (wrapper.Load.CustomStatusId != (Guid?)args.OldData) {
                    _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
                }
            }
            // Selected Date
            if (args.Cell.Column.UniqueName == "Load.QuikPay") {
                if (wrapper.Load.QuikPay != (DateTime?)args.OldData) {
                    _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
                }
            }
            // Edited Pallets
            if (args.Cell.Column.UniqueName == "Load.Pallets") {
                if (wrapper.Load.Pallets != (short?)args.OldData) {
                    _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
                }
            }
        }

        #endregion [ edit cell ]

        private void Test_OnClick(object sender, RoutedEventArgs args) {
            //var saveList = new List<LoadViewModel>();
            //var loads = Model.Loads.Where(e => e.ParentId == null && e.DatRate == null).ToList();
            //for (int i = 0; i < loads.Count(); i++) {
            //    var load = loads[i];
            //    // DAT Rate
            //    if (load.ParentId == null) {
            //        var origin = new Place() {
            //            Item = new LatLon() {
            //                latitude = load.OriginLocation.Latitude,
            //                longitude = load.OriginLocation.Longitude,
            //            }
            //        };
            //        var destination = new Place() {
            //            Item = new LatLon() {
            //                latitude = load.DestinationLocation.Latitude,
            //                longitude = load.DestinationLocation.Longitude,
            //            }
            //        };
            //        var equipment = RateEquipmentConverter.Convert(load.Equipment);
            //        if (equipment != null) {
            //            var result = _manager.DatPowerManager.GetRates(
            //                origin,
            //                destination,
            //                equipment.Value,
            //                DatPowerBoardType.PostTruck);
            //            var rateModel = new DatPowerRateViewModel() { Data = new DatPowerRateDataViewModel() };
            //            if (result.Successed) {
            //                RateConverter.Convert(result, rateModel.Data);
            //                var model = rateModel.Data;
            //                model.Calc();
            //                load.DatRate = (decimal?)(model.ShowRates ?
            //                    model.SpotEstimatedLinehaulRateFuel * model.RateMiles : model.SpotEstimatedLinehaulTotal);
            //                saveList.Add(load);
            //            }
            //        }
            //    }
            //    Debug.WriteLine(100 * i / loads.Count());
            //}
            //Debug.WriteLine("SAVING...");
            //_dispatchService.SaveLoads(saveList, this.SaveLoadsCompleted);
        }

        private void SaveLoadsCompleted(ViewModelWrapper<List<LoadViewModel>> obj) {
            Debug.WriteLine("DONE");
        }

        private void r06_Error(object sender, ValidationErrorEventArgs e) {
            e.Error.ErrorContent = "123";
        }
    }
}
