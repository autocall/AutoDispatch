using System.Linq;
using System.Windows;
using System.Windows.Data;
using AutoCall.Common.Enums;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;

namespace AutoCall.WPF.UserControls.Boards {
    public class DispatchCarrierAccountBoard : DispatchAccountBoard {

        public override DispatchBoardType DispatchBoardType => DispatchBoardType.CarrierAccount;

        public DispatchCarrierAccountBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings) : base(moduleName, boardType, manager, settings) {

            Static_SubLoadsViewSource_OnFilter = SubLoadsViewSource_OnFilter;

            Grids[DispatchFilterTableType.Accounting_Carriers] = gridLoads;
        }

        #region [ Initialize ]

        protected override void AddColumns() {
            base.AddColumns();

            historyFilter.ItemsSource = Model.CarrierHistoryLoads;
            {
                col.CellEditTemplate = null;
                col.CellTemplate = (DataTemplate)col.Resources["carrierLoadDataTemplate"];
                col.IsReadOnly = true;
            }
            {
                var col = gridLoads.CreateSumColumn("Total Expenses", "Load.ExpensesTotal", format: "C2");
                col.ColumnGroupName = "Load";
                col.Width = 55;
            }
            {
                var col = gridLoads.CreateSumColumn("Total Accessorial", "Load.GainsTotal", format: "C2");
                col.ColumnGroupName = "Load";
                col.Width = 55;
            }
            {
                var col = gridLoads.CreateTextColumn("Driver", "HistoryDriver.DisplayName");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Truck", "HistoryTruck.Number");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Current Trailer", nameof(Wrapper1LoadViewModel.HistoryTrailerNumber));
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Assigned Trailer", nameof(Wrapper1LoadViewModel.HistoryAssignedTrailerNumber));
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Current Trailer Date", nameof(Wrapper1LoadViewModel.LoadBindDate));
                col.ColumnGroupName = "Carrier";
                col.Width = 85;
            }
            {
                var col = gridLoads.CreateTextColumn("Assigned Trailer Date", nameof(Wrapper1LoadViewModel.LoadAssignedTrailerDate));
                col.ColumnGroupName = "Carrier";
                col.Width = 85;
            }
            {
                var col = gridLoads.CreateTextColumn("Warehouse", nameof(Wrapper1LoadViewModel.HistoryWarehouseName));
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Total Pickups", "PickupCount");
                col.ColumnGroupName = "Load";
                col.Width = 40;
            }
            {
                var col = gridLoads.CreateTextColumn("Total Deliveries", "DeliveryCount");
                col.ColumnGroupName = "Load";
                col.Width = 40;
            }

            foreach (GridViewColumn column in gridLoads.Columns.OfType<GridViewColumn>().ToList()) {
                if (column.ColumnGroupName == "Carrier" && column.UniqueName == "HistoryDatDriver.Name") {
                    gridLoads.Columns.Remove(column);
                }
            }
            gridLoads.ColumnGroups.First(g => g.Name == "Carrier").Header = "Carrier/Driver";
            gridLoads.Columns.Remove(this.facComCol);

            CreateGainAndExpenseColumns(gridLoads);

            if (_session.HasBnsfRailway) {
                GridViewColumn col = gridLoads.CreateTextColumn("BNSF Unit", $"Trailer.BnsfUnit");
                col.Width = 70;
                col.ColumnGroupName = "Carrier";
                col.IsVisible = true;
            }

            // Checks display indexes
            var columns = gridLoads.Columns.OfType<GridViewColumn>().OrderBy(c => c.DisplayIndex).ToList();
            for (int i = 0; i < columns.Count(); i++) {
                if (columns[i].DisplayIndex != i) {
                    columns[i].DisplayIndex = i;
                }
            }

            // LinkedWrapper
            gridLoads.Columns.Remove(lc0);
            gridLoads.Columns.Remove(lc1);
            gridLoads.Columns.Remove(lc2);
            gridLoads.Columns.Remove(lc3);
            gridLoads.Columns.Remove(lc4);
            gridLoads.Columns.Remove(lc5);
            gridLoads.Columns.Remove(lc6);
            gridLoads.Columns.Remove(lc7);
            gridLoads.Columns.Remove(lc8);
            gridLoads.Columns.Remove(lc9);
            gridLoads.Columns.Remove(lc10);
            gridLoads.Columns.Remove(lc11);
            gridLoads.Columns.Remove(lc12);
            gridLoads.Columns.Remove(lc13);
            gridLoads.Columns.Remove(lc14);
            gridLoads.Columns.Remove(lc15);
            gridLoads.Columns.Remove(lc16);
        }

        #endregion [ Initialize ]

        #region [ Filters ]

        protected override void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.LoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Accounting_Carriers;
        }
        protected override void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.SubLoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Sub_Accounting_Carriers;
        }

        #endregion [ Filters ]

        #region [ toolbar ]

        protected override void Factoring_Company_OnClick(object sender, RoutedEventArgs args) {
            var dialog = new FactoringCompanyDialog(_dispatchService, _mapper);
            var clonedCompanies = Model.ActualCompanies
                .Where(e => e.BoardType == DispatchBoardType.Carrier)
                .Select(e => (ActualCompanyViewModel)e.Clone()).ToList();
            dialog.Model = new FactoringCompanyDialog.FactoringCompanyDialogViewModel {
                ActualCompanies = clonedCompanies,
                SelectedActualCompany = clonedCompanies.FirstOrDefault(e => e.EntityId == Model.SelectedCarrierCompany?.EntityId)
            };
            dialog.ShowDialog();
        }

        #endregion [ toolbar ]

    }
}
