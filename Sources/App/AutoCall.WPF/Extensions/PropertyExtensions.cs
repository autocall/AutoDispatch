using AutoCall.Common.Enums;
using AutoDispatch.Logic.Attributes;
using System;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace AutoCall.WPF.Extensions {
    public class PropertyExtensions {
        public static readonly DependencyProperty ErrorProperty =
               DependencyProperty.RegisterAttached("Error", typeof(ValidationEnum), typeof(PropertyExtensions),
                   new PropertyMetadata() { DefaultValue = ValidationEnum.Normal });

        public static void SetError(DependencyObject element, ValidationEnum value) {
            element.SetValue(ErrorProperty, value);
        }

        public static ValidationEnum GetError(DependencyObject element) {
            return (ValidationEnum)element.GetValue(ErrorProperty);
        }

        public static readonly DependencyProperty RelativeDisplayIndexProperty =
               DependencyProperty.RegisterAttached("RelativeDisplayIndex", typeof(int?), typeof(PropertyExtensions), new PropertyMetadata(null));

        public static void SetRelativeDisplayIndex(GridViewColumn element, int? value) {
            element.SetValue(RelativeDisplayIndexProperty, value);
        }

        public static int? GetRelativeDisplayIndex(GridViewColumn element) {
            return (int?)element.GetValue(RelativeDisplayIndexProperty);
        }

        public static readonly DependencyProperty RelativeGroupProperty =
               DependencyProperty.RegisterAttached("RelativeGroup", typeof(string), typeof(PropertyExtensions), new PropertyMetadata(null));

        public static void SetRelativeGroup(GridViewColumn element, string value) {
            element.SetValue(RelativeGroupProperty, value);
        }

        public static string GetRelativeGroup(GridViewColumn element) {
            return (string)element.GetValue(RelativeGroupProperty);
        }

        // TODO: NOT USING
        public static readonly DependencyProperty HiddenForSubLoadProperty =
               DependencyProperty.RegisterAttached("HiddenForSubLoad", typeof(bool), typeof(PropertyExtensions), new PropertyMetadata(false));

        public static void SetHiddenForSubLoad(GridViewColumn element, bool value) {
            element.SetValue(HiddenForSubLoadProperty, value);
        }

        public static bool GetHiddenForSubLoad(GridViewColumn element) {
            return (bool)element.GetValue(HiddenForSubLoadProperty);
        }

        // TODO: NOT USING
        public static readonly DependencyProperty SimpleStyleForSubLoadProperty =
               DependencyProperty.RegisterAttached("SimpleStyleForSubLoad", typeof(bool), typeof(PropertyExtensions), new PropertyMetadata(false));

        public static void SetSimpleStyleForSubLoad(GridViewColumn element, bool value) {
            element.SetValue(SimpleStyleForSubLoadProperty, value);
        }

        public static bool GetSimpleStyleForSubLoad(GridViewColumn element) {
            return (bool)element.GetValue(SimpleStyleForSubLoadProperty);
        }

        public static readonly DependencyProperty MinPermissionProperty =
               DependencyProperty.RegisterAttached("MinPermission", typeof(RoleName), typeof(PropertyExtensions), new PropertyMetadata(RoleName.User));

        public static void SetMinPermission(AutoCall.WPF.UserControls.RadMenuItem element, RoleName value) {
            element.SetValue(MinPermissionProperty, value);
        }

        public static RoleName GetMinPermission(AutoCall.WPF.UserControls.RadMenuItem element) {
            return (RoleName)element.GetValue(MinPermissionProperty);
        }
    }
}
