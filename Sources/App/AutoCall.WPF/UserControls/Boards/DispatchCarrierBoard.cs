using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Dialogs;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;

namespace AutoCall.WPF.UserControls.Boards {
    public class DispatchCarrierBoard : DispatchBoard {

        #region [ Properties ]

        public sealed override DispatchBoardType DispatchBoardType => DispatchBoardType.Carrier;

        #endregion [ Properties ]

        #region [ .ctor ]

        public DispatchCarrierBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings) :
            base(moduleName, boardType, manager, settings) {

            Grids[DispatchFilterTableType.Carrier_Patterns] = gridPatterns;
            Grids[DispatchFilterTableType.Carrier_Records] = gridLoads;

//#if DEBUG
//            Model.Loaded += async () => {
//                while (gridPatterns.Items.Count == 0 || gridPatterns.Items.Cast<Wrapper1LoadViewModel>().ToArray()[0].Load == null) {
//                    await Task.Delay(TimeSpan.FromSeconds(1));
//                }
//                Wrapper1LoadViewModel item = gridPatterns.Items.Cast<Wrapper1LoadViewModel>().ToArray()[0];
//                //var item = Model.Wrappers.First();
//                var actions = new DispatchActions(this);
//                actions.EditLoadAction(item);
//            };
//#endif
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        protected override void AddColumns() {
            base.AddColumns();

            var session = UserSession.Current;
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == true) {
                {
                    var col = gridLoads.CreateSumColumn("Total Expenses", "Load.ExpensesTotal", format: "C2");
                    col.ColumnGroupName = "Load";
                    col.SetColumnStyle2("Load.Parent1");
                    col.Width = 55;
                }
                {
                    var col = gridLoads.CreateSumColumn("Total Accessorial", "Load.GainsTotal", format: "C2");
                    col.ColumnGroupName = "Load";
                    col.SetColumnStyle2("Load.Parent1");
                    col.Width = 55;
                }
            }
            {
                var col = gridPatterns.CreateTextColumn("Driver", "Driver.DisplayName");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTextColumn("Truck", "Truck.Number");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var aggregate = new AggregateFunction<Wrapper1LoadViewModel, int>() {
                    AggregationExpression = wrappers => wrappers.Select(e => e.Trailer).Where(e => e != null).Distinct().Count(),
                };
                gridPatterns_trailerCol.AggregateFunctions.Add(aggregate);
            }
            {
                var col = gridPatterns.CreateTextColumn("Assigned Trailer", "HistoryAssignedTrailer.Number");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
                var aggregate = new AggregateFunction<Wrapper1LoadViewModel, int>() {
                    AggregationExpression = wrappers => wrappers.Select(e => e.HistoryAssignedTrailer).Where(e => e != null).Distinct().Count(),
                };
                col.AggregateFunctions.Add(aggregate);
            }
            {
                var col = gridPatterns.CreateTextColumn("Current Trailer Date", nameof(Wrapper1LoadViewModel.LoadBindDate));
                col.ColumnGroupName = "Carrier";
                col.Width = 85;
            }
            {
                var col = gridPatterns.CreateTextColumn("Assigned Trailer Date", nameof(Wrapper1LoadViewModel.LoadAssignedTrailerDate));
                col.ColumnGroupName = "Carrier";
                col.Width = 85;
            }
            {
                var col = gridPatterns.CreateTextColumn("Destination Warehouse", "DestinationWarehouse.Name");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Destination Warehouse", "DestinationWarehouse.Name");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTextColumn("Warehouse", "Warehouse.Name");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTextColumn("Yard", "Yard.Name");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == true) {
                {
                    var col = gridPatterns.CreateSumColumn("Total Expenses", "Load.ExpensesTotal", format: "C2");
                    col.ColumnGroupName = "Load";
                    col.SetColumnStyle2("Load.Parent1");
                    col.Width = 55;
                }
                {
                    var col = gridPatterns.CreateSumColumn("Total Accessorial", "Load.GainsTotal", format: "C2");
                    col.ColumnGroupName = "Load";
                    col.SetColumnStyle2("Load.Parent1");
                    col.Width = 55;
                }
            }
            {
                var aggregate = new AggregateFunction<Wrapper1LoadViewModel, int>() {
                    AggregationExpression = wrappers => wrappers.Select(e => (e.Load.Parent1 ?? e).Load).Distinct().Count(),
                };
                gridPatterns_IndexCol.AggregateFunctions.Add(aggregate);
            }
            gridPatterns.Columns.Remove(carrierRatingCol);

            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == true) {
                CreateGainAndExpenseColumns(gridPatterns);
                CreateGainAndExpenseColumns(gridLoads);
            } else {
                gridPatterns.Columns.Remove(p01);
                gridPatterns.Columns.Remove(p02);
                gridPatterns.Columns.Remove(p03);
                gridLoads.Columns.Remove(r04);
                gridLoads.Columns.Remove(r05);
                gridLoads.Columns.Remove(gridLoads_confCol);
            }
            if (_session.HasBnsfRailway) {
                CreateBNSFColumns();
            }

            // LinkedWrapper
            gridPatterns.Columns.Remove(lc0);
            gridPatterns.Columns.Remove(lc1);
            gridPatterns.Columns.Remove(lc2);
            gridPatterns.Columns.Remove(lc3);
            gridPatterns.Columns.Remove(lc4);
            gridPatterns.Columns.Remove(lc5);
            gridPatterns.Columns.Remove(lc6);
            gridPatterns.Columns.Remove(lc7);
            gridPatterns.Columns.Remove(lc8);
            gridPatterns.Columns.Remove(lc9);
            gridPatterns.Columns.Remove(lc10);
            gridPatterns.Columns.Remove(lc11);
            gridPatterns.Columns.Remove(lc12);
            gridPatterns.Columns.Remove(lc13);
            gridPatterns.Columns.Remove(lc14);
            gridPatterns.Columns.Remove(lc15);
            gridPatterns.Columns.Remove(lc16);

        }

        #region [ BNSF Columns ]

        private void CreateBNSFColumns() {
            Dictionary<string, string> bnsfColumns;
            if (((RoleName)_session.Roles).CheckPermission(RoleName.Warehouse) == true) {
                bnsfColumns = new Dictionary<string, string>() {
                    { nameof(TrailerViewModel.BnsfOwner), "Owner" },
                    { nameof(TrailerViewModel.BnsfShipper), "Shipper" },
                    { nameof(TrailerViewModel.BnsfUnit), "Unit" },
                    { nameof(TrailerViewModel.BnsfLoadEmptyStatus), "Load Empty Status" },
                    { nameof(TrailerViewModel.BnsfCurrentLocation), "Current Location" },
                    { nameof(TrailerViewModel.BnsfEventCode), "Event Code" },
                    { nameof(TrailerViewModel.BnsfEventDate), "Event Date" },
                    { nameof(TrailerViewModel.BnsfETA_Deramp), "ETA Deramp" },
                    { nameof(TrailerViewModel.BnsfOrigin), "Origin" },
                    { nameof(TrailerViewModel.BnsfDestination), "Destination" },
                    { nameof(TrailerViewModel.BnsfBeyondGoalHours), "Beyond Goal Hours" },
                    { nameof(TrailerViewModel.BnsfTimeDeramp), "Time Deramp" },
                    { nameof(TrailerViewModel.BnsfTimeNotify), "Time Notify" },
                    { nameof(TrailerViewModel.BnsfFinalTrainArrival), "Final Train Arrival" },
                    { nameof(TrailerViewModel.BnsfGoalDate), "Goal Date" },
                    { nameof(TrailerViewModel.BnsfNextevent), "Nexte Eent" },
                    { nameof(TrailerViewModel.BnsfPickUpNumber), "Pick-Up Number" },
                    { nameof(TrailerViewModel.BnsfStorageDay), "Storage Day" },
                    { nameof(TrailerViewModel.BnsfTrain), "Train" },
                    { nameof(TrailerViewModel.BnsfTrucker), "Trucker" },
                    { nameof(TrailerViewModel.BnsfWaybillDate), "Waybill Date" },
                    { nameof(TrailerViewModel.BnsfRateAuthority), "Rate Authority" },
                    { nameof(TrailerViewModel.BnsfAmountDue), "Amount Due" },
                    { nameof(TrailerViewModel.BnsfDueDate), "Due Date" },
                    { nameof(TrailerViewModel.BnsfWaybillNumber), "Waybill Number" },
                    { nameof(TrailerViewModel.BnsfBillNumber), "Bill Number" },
                    { nameof(TrailerViewModel.BnsfAdjustments), "Adjustments" },
                };
            } else {
                bnsfColumns = new Dictionary<string, string>() {
                    { nameof(TrailerViewModel.BnsfUnit), "Unit" },
                    { nameof(TrailerViewModel.BnsfCurrentLocation), "Current Location" },
                    { nameof(TrailerViewModel.BnsfEventCode), "Event Code" },
                    { nameof(TrailerViewModel.BnsfETA_Deramp), "ETA Deramp" },
                    { nameof(TrailerViewModel.BnsfPickUpNumber), "Pick-Up Number" },
                    { nameof(TrailerViewModel.BnsfStorageDay), "Storage Day" },
                };
            }
            foreach (var item in bnsfColumns) {
                GridViewColumn bnsfCol;
                if (item.Key == nameof(TrailerViewModel.BnsfAmountDue)) {
                    bnsfCol = gridPatterns.CreateTextColumn(item.Value, $"Trailer.{item.Key}", format: "C2");
                    bnsfCol.AggregateFunctions.Add(new SumFunction() { ResultFormatString = "{0:C2}" });
                } else
                if (item.Key == nameof(TrailerViewModel.BnsfETA_Deramp)) {
                    bnsfCol = gridPatterns.CreateTextColumn(item.Value, $"Trailer.{item.Key}");
                } else {
                    bnsfCol = gridPatterns.CreateTextColumn(item.Value, $"Trailer.{item.Key}");
                }
                bnsfCol.Width = 50;
                bnsfCol.ColumnGroupName = "BNSF";
                bnsfCol.IsVisible = false;
            }
        }

        #endregion [ BNSF Columns ]

        #endregion [ Initialize ]

        #region [ Engine Events ]

        protected override async Task _engine_EventReceived(EventResponse response) {
            await base ._engine_EventReceived(response);
            if (this.Dispatcher.CheckAccess() == true) {
                gridPatterns.CalculateAggregates();
                gridLoads.CalculateAggregates();
            }
        }

        #endregion [ Engine Events ]

        #region [ toolbar ]

        protected override void ExportPatterns_Clicked(object sender, RoutedEventArgs e) {
            base.ExportAction(gridPatterns, "autocall_carrier_dispatch_patterns");
        }

        protected override void ExportLoads_Clicked(object sender, RoutedEventArgs e) {
            base.ExportAction(gridLoads, "autocall_carrier_dispatch_records");
        }

        #endregion [ toolbar ]

        #region [ Filters ]

        protected override void PatternsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.PatternsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Carrier_Patterns;
        }
        protected override void SubPatternsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.SubPatternsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Sub_Carrier_Patterns;
        }
        protected override void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.LoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Carrier_Records;
        }
        protected override void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.SubLoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Sub_Carrier_Records;
        }

        #endregion [ Filters ]
    }
}
