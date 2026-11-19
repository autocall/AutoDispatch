using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AutoCall.WPF.Extensions;
using Microsoft.Web.WebView2.Core;

namespace AutoCall.WPF.Dialogs {
    /// <summary>
    ///     Карта маршрута сервера (/Map?r=…) во встроенном WebView2 — копия MapWindow AutoCall V3. Заменяет MapRouteDialog V1
    ///     (WinForms + EO.WebBrowser, Dispatch/RouteLocation?t=… с Bearer) — AI/wiki/public-site.md «Карты для App».
    ///     Ссылку можно скопировать или открыть в системном браузере; без runtime WebView2 окно не создаётся — Open() сразу открывает браузер </summary>
    public partial class MapWindow : Window {

        /// <summary>
        ///     Профиль WebView2 (cookie настроек карты, кэш) — в профиле пользователя: рядом с exe писать нельзя </summary>
        public static readonly string UserDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoDispatch", "WebView2");

        private string _url;

        private MapWindow(string url) {
            this._url = url;
            InitializeComponent();
            tbUrl.Text = url;
            Loaded += MapWindow_Loaded;
        }

        /// <summary>
        ///     Окно с картой; если runtime WebView2 не установлен или не поднялся — системный браузер </summary>
        public static void Open(string url) {
            try {
                CoreWebView2Environment.GetAvailableBrowserVersionString();
            } catch (Exception exception) {
                Serilog.Log.Warning("WebView2 runtime is not available ({Message}), opening the map in the browser", exception.Message);
                OpenInBrowser(url);
                return;
            }
            new MapWindow(url).Show();
        }

        public static void OpenInBrowser(string url) {
            try {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            } catch (Exception exception) {
                Serilog.Log.Error(exception, "Open map {Url}", url);
                MessageBoxEx.Error(exception.GetBaseException().Message);
            }
        }

        private async void MapWindow_Loaded(object sender, RoutedEventArgs e) {
            try {
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: UserDataFolder);
                await webView.EnsureCoreWebView2Async(environment);
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                // страница подменяет r в адресе при остановках (history.replaceState) — ссылка для копирования и браузера с ними же
                webView.CoreWebView2.SourceChanged += (_, _) => {
                    _url = webView.CoreWebView2.Source.Replace("&embedded=true", "");
                    tbUrl.Text = _url;
                };
                // embedded — страница без навигации сайта; копируемая ссылка и браузер — обычный layout
                webView.Source = new Uri(_url + "&embedded=true");
            } catch (Exception exception) {
                Serilog.Log.Error(exception, "WebView2 failed, opening the map in the browser");
                OpenInBrowser(_url);
                Close();
            }
        }

        private void CopyLink_Click(object sender, RoutedEventArgs e) {
            try {
                Clipboard.SetText(_url);
                tbStatus.Text = "Link copied";
            } catch (Exception exception) {
                tbStatus.Text = exception.GetBaseException().Message;
            }
            tbStatus.Visibility = Visibility.Visible;
        }

        private void OpenInBrowser_Click(object sender, RoutedEventArgs e) {
            OpenInBrowser(_url);
        }
    }
}
