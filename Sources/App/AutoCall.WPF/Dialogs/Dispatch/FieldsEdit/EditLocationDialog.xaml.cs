using System;
using System.Windows;
using AutoCall.Logic.Services;
using AutoCall.WPF.Extensions;
using AutoDispatch.Data.Api;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;

namespace AutoCall.WPF.Dialogs.Dispatch {
    /// <summary>
    ///     Выбор места на карте. V1: EO.WebBrowser открывал Dispatch/SelectLocation?latlng= с Bearer, OK разбирал HTML страницы
    ///     (HtmlAgilityPack, id address/city/formatted_address/latlng). Теперь WebView2 и страница сервера Map/SelectLocation?r=
    ///     (ссылка — Api/Maps/SelectLocationLink), каждый выбор страница шлёт сообщением { type: "location", latitude, longitude,
    ///     formattedAddress, address, city }, OK берёт последнее (AI/wiki/public-site.md «Карты для App») </summary>
    public partial class EditLocationDialog : Window, IDisposable {

        protected ICallService _callService;

        /// <summary>
        ///     Последнее выбранное на странице место </summary>
        private JObject _selected;

        /// <summary>
        ///     Окно закрыто: Initialize после await не трогает освобождённый WebView2 (аудит D17) </summary>
        private bool _closed;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public EditLocationViewModel Model {
            get { return DataContext as EditLocationViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Constructor </summary>
        public EditLocationDialog(bool withComments = false) {
            InitializeComponent();

            if (withComments == false) {
                bComments.Visibility = Visibility.Collapsed;
            }
        }

        public async void Initialize() {
            try {
                var location = Model.Location.Location;
                var point = location != null ? new MapLatLngDto(location.Latitude, location.Longitude) : null;
                var url = await App.Services.GetRequiredService<MapApi>().CreateSelectLocationLinkAsync(point);
                if (_closed) return;

                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: MapWindow.UserDataFolder);
                if (_closed) return;
                await browser.EnsureCoreWebView2Async(environment);
                if (_closed) return;
                browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                browser.CoreWebView2.WebMessageReceived += Browser_WebMessageReceived;
                browser.Source = new Uri(url + "&embedded=true");
            } catch (Exception exception) {
                if (_closed) return;
                Serilog.Log.Error(exception, "Select location map failed");
                MessageBoxEx.Error(this, exception.GetBaseException().Message);
            }
        }

        private void Browser_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e) {
            try {
                var message = JObject.Parse(e.WebMessageAsJson);
                if ((string)message["type"] == "location") {
                    _selected = message;
                }
            } catch (Exception exception) {
                Serilog.Log.Warning(exception, "Select location: unexpected message");
            }
        }

        /// <summary>
        ///     On window closed handler </summary>
        private void MapDialog_OnClosed(object sender, EventArgs e) {
            _closed = true;
            this.Dispose();
        }

        /// <summary>
        ///     OK clicked handler </summary>
        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (_selected != null) {
                // Comments via binding
                Model.Location.Address = (string)_selected["address"];
                Model.Location.City = (string)_selected["city"];
                Model.Location.Location = new LocationViewModel() {
                    Latitude = (float)_selected["latitude"],
                    Longitude = (float)_selected["longitude"],
                    FormattedAddress = (string)_selected["formattedAddress"],
                };
                DialogResult = true;
            } else {
                MessageBoxEx.Alert(this, "The location is not selected");
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        public void Dispose() {
            browser.Dispose();
        }
    }
}
