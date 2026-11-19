using AutoCall.Common.Enums;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using System.Windows.Data;
using System.Windows;
using Telerik.Windows.Controls.GridView;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Data;
using AutoCall.Common.Extensions;
using AutoCall.WPF.Dialogs;
using AutoCall.WPF.Converters;
using Telerik.Windows.Controls;
using AutoCall.WPF.Converters.Dispatch;

namespace AutoCall.WPF.UserControls.Boards {
    public class DispatchBrokerBoard : DispatchBoard {

        #region [ Properties ]

        public sealed override DispatchBoardType DispatchBoardType => DispatchBoardType.Broker;

        #endregion [ Properties ]

        #region [ .ctor ]

        public DispatchBrokerBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings) :
            base(moduleName, boardType, manager, settings) {

            Grids[DispatchFilterTableType.Broker_Patterns] = gridPatterns;
            Grids[DispatchFilterTableType.Broker_Records] = gridLoads;
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        protected override void AddColumns() {
            base.AddColumns();

            {
                var col = gridPatterns.CreateTextColumn("MC", "DatCompanyView.McNumber");
                col.UniqueName = "DatCompany.McNumber";
                col.ColumnGroupName = "Carrier";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTemplateColumn("Carrier", "DatCompanyView.Name",
                    (DataTemplate)this.Resources["DatCompanyViewNameColumnTemplate"]);
                col.UniqueName = "DatCompany.Name";
                col.ColumnGroupName = "Carrier";
                col.Width = 200;
                #region incuranse_style 
                col.CellStyle = new Style(typeof(GridViewCell));
                var binding1 = new Binding("DatCompanyView.InsuranceState") {
                    Converter = (IValueConverter)Resources["InsuranceStateConverter"],
                    ConverterParameter = "Background"
                };
                var binding2 = new Binding("DatCompanyView.InsuranceState") {
                    Converter = (IValueConverter)Resources["InsuranceStateConverter"],
                    ConverterParameter = "Foreground"
                };
                var binding3 = new Binding("DatCompanyView.InsuranceState") {
                    Converter = (IValueConverter)Resources["InsuranceStateConverter"],
                    ConverterParameter = "FontWeight"
                };
                col.CellStyle.Setters.Add(new Setter(GridViewCell.BackgroundProperty, binding1));
                col.CellStyle.Setters.Add(new Setter(GridViewCell.ForegroundProperty, binding2));
                // NOTE: Like to DispatchBoard.xaml
                // NOTE: Sets Normal or Bold
                //column4.CellStyle.Setters.Add(new Setter(GridViewCell.FontWeightProperty, binding3));
                // NOTE: Sets Value of row or Bold
                col.CellStyle.Triggers.Add(new DataTrigger() {
                    Binding = binding3,
                    Value = FontWeights.Bold,
                    Setters = { new Setter(GridViewCell.FontWeightProperty, FontWeights.Bold) }
                });
                #endregion incuranse_style 
            }
            {
                var col = gridPatterns.GridViewPhoneColumn("Phone", "DatCompanyView.Contacts");
                col.UniqueName = "DatCompany.Contacts";
                col.ColumnGroupName = "Carrier";
                col.Width = 100;
            }
            #region [ Tracker ]
            {
                var col = gridPatterns.CreateTemplateColumn(
                    "Tracker", 
                    "Load.CarrierViewPhone",
                     (DataTemplate)Application.Current.Resources["CarrierViewTrackerColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 100;
            }
            {
                var col = gridPatterns.CreateTemplateColumn(
                    "Tracker Status1",
                    "Load.CarrierViewStatuses1String",
                     (DataTemplate)Application.Current.Resources["CarrierViewStatus1ColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTemplateColumn(
                    "Tracker Status2",
                    "Load.CarrierViewStatuses2String",
                     (DataTemplate)Application.Current.Resources["CarrierViewStatus2ColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTemplateColumn("Tracker Pickup Comment", "Load.CarrierViewPickupComment",
                    (DataTemplate)Application.Current.Resources["CarrierViewPickupCommentColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            {
                var col = gridPatterns.CreateTemplateColumn("Tracker Delivery Comment", "Load.CarrierViewDeliveryComment",
                    (DataTemplate)Application.Current.Resources["CarrierViewDeliveryCommentColumnTemplate"]);
                col.ColumnGroupName = "Tracker";
                col.Width = 80;
            }
            gridPatterns.ColumnGroups.Add(new GridViewColumnGroup() { Header = "Tracker", Name = "Tracker" });
            #endregion [ Tracker ]
            gridLoads.Columns.Remove(gridLoads_confCol);
            gridLoads.Columns.Remove(gridLoads_bolCol);
            gridLoads.Columns.Remove(gridLoads_podCol);
            gridPatterns.Columns.Remove(gridPatterns_podCol);
            gridPatterns.Columns.Remove(gridPatterns_trailerCol);

            var session = UserSession.Current;
            if (((RoleName)session.Roles).CheckPermission(RoleName.Dispatcher) == true) {
                CreateGainAndExpenseColumns(gridPatterns);
                CreateGainAndExpenseColumns(gridPatterns, true);
            } else {
                // Warehouse Condition
                gridPatterns.Columns.Remove(lc2); // LinkedWrapper -> Line Haul
                gridPatterns.Columns.Remove(lc3); // LinkedWrapper -> Rate
                gridPatterns.Columns.Remove(lc7); // LinkedWrapper -> Invoice
                gridPatterns.Columns.Remove(lc8); // LinkedWrapper -> Check
                gridPatterns.Columns.Remove(lc9); // LinkedWrapper -> Payout
            }

            gridPatterns.ColumnGroups.Add(new GridViewColumnGroup() { Header = "Linked Load", Name = "LinkedWrapper" });
        }

        #endregion [ Initialize ]

        #region [ toolbar ]

        protected override void ExportPatterns_Clicked(object sender, RoutedEventArgs e) {
            base.ExportAction(gridPatterns, "autocall_broker_dispatch_patterns");
        }

        protected override void ExportLoads_Clicked(object sender, RoutedEventArgs e) {
            base.ExportAction(gridLoads, "autocall_broker_dispatch_records");
        }

        #endregion [ toolbar ]

        #region [ Filters ]

        protected override void PatternsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.PatternsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Broker_Patterns;
        }
        protected override void SubPatternsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.SubPatternsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Sub_Broker_Patterns;
        }
        protected override void LoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.LoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Broker_Records;
        }
        protected override void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs e) {
            base.SubLoadsViewSource_OnFilter(sender, e);
            e.Accepted = e.Accepted && Static_ViewSource_OnFilter((Wrapper1LoadViewModel)e.Item) == DispatchFilterLoadType.Sub_Broker_Records;
        }

        #endregion [ Filters ]
    }
}