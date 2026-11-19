using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace AutoCall.WPF {
    public static class WindowExtension {

        public static void ShowOne(this Window win) {
            //win.Show();
            var existWin = App.Current.Windows.Cast<Window>().FirstOrDefault(e => e != win && e.GetType() == win.GetType());
            if (existWin == null) {
                win.Show();
            } else {
                win.Close();
                existWin.Show();
                if (existWin.WindowState == WindowState.Minimized) {
                    existWin.WindowState = WindowState.Normal;
                }
                existWin.Focus();
            }
        }

        public static void ShowOneTitle(this RadWindow win, string comparePath) {
            //win.Show();
            var existWin = App.Current.Windows
                .Cast<Window>()
                .FirstOrDefault(e =>
                e.Content is ContentControl &&
                    Object.Equals(
                        ((ContentControl)e.Content).DataContext.GetType().GetProperty(comparePath)?.GetValue(((ContentControl)e.Content).DataContext),
                        win.DataContext.GetType().GetProperty(comparePath).GetValue(win.DataContext)
                        ));
            if (existWin == null) {
                win.Show();
            } else {
                win.Close();
                existWin.Show();
                if (existWin.WindowState == WindowState.Minimized) {
                    existWin.WindowState = WindowState.Normal;
                }
                existWin.Focus();
            }
        }
    }
}
