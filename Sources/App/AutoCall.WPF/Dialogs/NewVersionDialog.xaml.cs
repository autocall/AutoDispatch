using AutoCall.WPF.Extensions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace AutoCall.WPF.Dialogs {
    /// <summary>
    ///     Сообщение о выходе AutoCall V2 со ссылками на загрузку и инструкцией,
    ///     как получить у DAT сервисную учётку: V2 работает через REST API,
    ///     а он требует отдельного Service Account, а не обычного логина DAT. </summary>
    public partial class NewVersionDialog : Window {

        public NewVersionDialog() {
            InitializeComponent();
        }

        private void OnLinkNavigate(object sender, RequestNavigateEventArgs e) {
            try {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            } catch (Exception exception) {
                MessageBoxEx.Error(this, exception.GetBaseException().Message);
            }
            e.Handled = true;
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
