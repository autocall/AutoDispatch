using AutoCall.WPF.Converters;
using AutoCall.WPF.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.StyleSelector {
    public class GridViewColumnsContextMenuStyleSelector : System.Windows.Controls.StyleSelector {
        public override Style SelectStyle(object item, DependencyObject container) {
            var contextMenu = ((RadMenuItem)container).GetVisualParent<RadContextMenu>();
            if (item is GroupMenuItemViewModel) {
                return (Style)contextMenu.Resources["group"];
            } else {
                return (Style)contextMenu.Resources["item"];
            }
            throw new NotImplementedException();

        }
    }
}
