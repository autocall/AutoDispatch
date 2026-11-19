using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GridViewColumnCollection = Telerik.Windows.Controls.GridViewColumnCollection;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;
using Telerik.Windows.Controls;
using AutoCall.WPF.UserControls.Boards;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.WPF.Helpers;
using AutoDispatch.Data;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;

namespace AutoCall.WPF.Converters {

    public class ColumnsToMenuItemsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var session = UserSession.Current;
            var columns = ((GridViewColumnCollection)value).Cast<GridViewColumn>()
                .Where(e => String.IsNullOrEmpty(e.Header as string) == false &&
                    (((RoleName)session.Roles).CheckPermission(RoleName.Admin) || e.UniqueName != "Id") &&
                    e.UniqueName != "Source" &&
                    e.UniqueName != "CreditCheck")
                    // V1: && (e.UniqueName != "DispatchRating" || …FindVisualParent2<DatPowerBoard>().BoardType != BoardType.DatPowerPostTrucks) —
                    // колонка DispatchRating только у бордов DatPower (модуль AutoCall)
                .OrderBy(e => e.DisplayIndex)
                .ToList();
            return columns;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class GroupColumnsToMenuItemsConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var items = ((GridViewColumnCollection)value).Cast<GridViewColumn>()
                .Where(e => String.IsNullOrEmpty(e.Header as string) == false)
                .OrderBy(e => e.DisplayIndex)
                .GroupBy(e => new { 
                    GroupName = e.ColumnGroupName, 
                    GroupHeader = ((RadGridView)e.DataControl).ColumnGroups.First(g => g.Name == e.ColumnGroupName).Header 
                })
                .Select(e => new GroupMenuItemViewModel() { Header = e.Key.GroupHeader.ToString(), Columns = e.ToList() })
                .ToList();
            if (items.Count <= 1) {
                return items.Select(e => e.Columns).ToList();
            } else {
                return items;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class GroupMenuItemViewModel {
        public string Header { get; set; }
        public List<GridViewColumn> Columns { get; set; }
    }
}