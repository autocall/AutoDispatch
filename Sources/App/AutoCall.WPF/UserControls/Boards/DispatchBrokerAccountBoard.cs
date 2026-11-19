using AutoCall.Common.Enums;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Converters;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.UserControls.Boards {
    public class DispatchBrokerAccountBoard : DispatchAccountBoard {

        public override DispatchBoardType DispatchBoardType => DispatchBoardType.BrokerAccount;

        public DispatchBrokerAccountBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings) : base(moduleName, boardType, manager, settings) {

            Static_SubLoadsViewSource_OnFilter = SubLoadsViewSource_OnFilter;

            Grids[DispatchFilterTableType.Accounting_Brokers] = gridLoads;
        }

        #region [ Initialize ]

        protected override void AddColumns() {
            base.AddColumns();

            historyFilter.ItemsSource = Model.BrokerHistoryLoads;

            {
                var col = gridLoads.CreateTextColumn("MC#", "HistoryDriverCarrier_McNumber");
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTemplateColumn("Carrier", "HistoryDriverCarrier_Name",
                    (DataTemplate)this.Resources["HistoryDatCompanyNameColumnTemplate"]);
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTextColumn("Phone", "HistoryDriverCarrier_Contacts");
                col.ColumnGroupName = "Carrier";
                col.Width = 100;
            }
            #region [ Tracker ]
            {
                var col = gridLoads.CreateTemplateColumn(
                    "Tracker",
                    "Load.CarrierViewPhone",
                     (DataTemplate)Application.Current.Resources["CarrierViewTrackerColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 100;
            }
            {
                var col = gridLoads.CreateTemplateColumn(
                    "Load Status1",
                    "Load.CarrierViewStatuses1String",
                     (DataTemplate)Application.Current.Resources["CarrierViewStatus1ColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTemplateColumn(
                    "Load Status2",
                    "Load.CarrierViewStatuses2String",
                     (DataTemplate)Application.Current.Resources["CarrierViewStatus2ColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTemplateColumn("Tracker Pickup Comment", "Load.CarrierViewPickupComment",
                    (DataTemplate)Application.Current.Resources["CarrierViewPickupCommentColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            {
                var col = gridLoads.CreateTemplateColumn("Tracker Delivery Comment", "Load.CarrierViewDeliveryComment",
                    (DataTemplate)Application.Current.Resources["CarrierViewDeliveryCommentColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            gridLoads.ColumnGroups.Add(new Telerik.Windows.Controls.GridViewColumnGroup() { Header = "Tracker", Name = "Tracker" });
            #endregion [ Tracker ]


            CreateGainAndExpenseColumns(gridLoads);
            CreateGainAndExpenseColumns(gridLoads, true);

            gridLoads.Columns.Remove(this.podColumn);
            gridLoads.ColumnGroups.Add(new GridViewColumnGroup() { Header = "Linked Load", Name = "LinkedWrapper" });
        }

        #endregion [ Initialize ]

        #region [ Filters ]

        protected override void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.LoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Accounting_Brokers;
        }
        protected override void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.SubLoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Sub_Accounting_Brokers;
        }

        #endregion [ Filters ]
    }
}
