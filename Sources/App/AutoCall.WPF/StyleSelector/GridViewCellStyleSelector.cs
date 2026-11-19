using AutoDispatch.Logic.Attributes;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.StyleSelector {
    public class GridViewCellStyleSelector : System.Windows.Controls.StyleSelector {
        public override Style SelectStyle(object item, DependencyObject container) {
            var cell = (GridViewCell)container;
            if (cell.Column.UniqueName.StartsWith("LinkedWrapper.")) {
                return null;
            }
            var wrapper = (Wrapper1LoadViewModel)cell.DataContext;
            if (cell.Column == null || cell.Column.UniqueName == null) return base.SelectStyle(item, container);
            PropertyInfo property = ReflectionHelper.GetProp(wrapper.GetType(), cell.Column.UniqueName);
            if (property == null) return base.SelectStyle(item, container);
            var customAttrs = property.GetCustomAttributes(typeof(CustomValidationAttribute));
            foreach (CustomValidationAttribute customAttr in customAttrs) {
                if (customAttr != null) {
                    var result = customAttr.Validate(wrapper, cell.Column.UniqueName);
                    switch (result) {
                        case ValidationEnum.Warning:
                            cell.ToolTip = customAttr.GetPropValue("ErrorMessage");
                            return (Style)Application.Current.Resources["WarningGridViewCellStyle"];
                        case ValidationEnum.Critical:
                            cell.ToolTip = customAttr.GetPropValue("ErrorMessage");
                            return (Style)Application.Current.Resources["ErrorGridViewCellStyle"];
                        case ValidationEnum.Normal:
                            cell.ToolTip = null;
                            return null;

                    }
                }
            }
            return base.SelectStyle(item, container);
        }
    }
}