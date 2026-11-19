using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoCall.WPF.Extensions;
using AutoDispatch.Logic.Heplers;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

namespace AutoCall.WPF.UserControls {
    public class RadMenuItem : Telerik.Windows.Controls.RadMenuItem {

        protected override void OnInitialized(EventArgs e) {
            Visibility = RolesExtension.CheckPermission(
                (RoleName)UserSession.Current.Roles,
                PropertyExtensions.GetMinPermission(this)) ?
                System.Windows.Visibility.Visible :
                System.Windows.Visibility.Collapsed;

            IsEnabledChanged += (sender, args) => Visibility = args.NewValue.Equals(true) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            base.OnInitialized(e);
        }
    }
}
