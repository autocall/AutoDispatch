using System;
using AutoCall.WPF.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Services;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using AutoDispatch.Data.Api;
using AutoDispatch.Logic.Converters;
using Newtonsoft.Json;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditStopsDialog : BaseDispatchDialog, IDisposable {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;


        protected ICallService _callService;
        private DispatchService _dispatchService;
        private IMapper _mapper;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public EditStopsViewModel Model {
            get { return DataContext as EditStopsViewModel; }
            set { DataContext = value; }
        }

        private IList<LoadViewModel> _loads;

        public EditStopsDialog(DispatchService dispatchService, ICallService callService, IMapper mapper, TrailerViewModel trailer, IList<LoadViewModel> loads) {
            _dispatchService = dispatchService;
            _mapper = mapper;
            _callService = callService;

            InitializeComponent();

            var stops = new ObservableCollection<StopViewModel>();
            _loads = loads;
            foreach (var load in loads) {
                var tmps = StopsConverter.ToVm(load);
                foreach (var stop in tmps) {
                    stops.Add(stop);
                }
            }

            Model = new EditStopsViewModel() { Stops = new ObservableCollection<StopViewModel>(stops.OrderBy(s => s.Order)), Trailer = trailer };

            //browser.LoadCompleted += Browser_LoadCompleted;
        }

        //private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e) {
        //    ((dynamic)browser.Document).body.Focus();
        //}

        /// <summary>
        ///     V1: EO.WebBrowser, Dispatch/OrderStops?TrailerId= с Bearer; теперь WebView2 и Map/OrderStops?r= сервера (ссылка —
        ///     Api/Maps/OrderStopsLink, AI/wiki/public-site.md «Карты для App») </summary>
        public async void Initialize() {
            try {
                var url = await App.Services.GetRequiredService<MapApi>().CreateOrderStopsLinkAsync(Model.Trailer.EntityId);
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: MapWindow.UserDataFolder);
                await browser.EnsureCoreWebView2Async(environment);
                browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
                browser.Source = new Uri(url + "&embedded=true");
            } catch (Exception exception) {
                Serilog.Log.Error(exception, "Order stops map failed");
                MessageBoxEx.Error(this, exception.GetBaseException().Message);
            }
        }

        /// <summary>
        ///     On window closed handler </summary>
        private void MapDialog_OnClosed(object sender, EventArgs e) {
            this.Dispose();
        }

        /// <summary>
        ///     Close, when escape pressed </summary>
        private void MapDialog_OnKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
            }
        }

        private ViewModelWrapper<LoadViewModel>[] results;

        /// <summary>
        ///     OK clicked handler </summary>
        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            if (_loads.Count == 0) {
                DialogResult = true;
                return;
            }

            // Reset orders
            foreach (var load in _loads) {
                load.DeliveryOrder = load.PickupOrder = null;
            }
            // Recalc orders
            for (byte i = 0; i < Model.Stops.Count; i++) {
                foreach (var load in _loads) {
                    if (Model.Stops[i].LoadId == load.EntityId) {
                        if (Model.Stops[i].Type == StopType.Pickup) {
                            load.PickupOrder = i;
                            break;
                        }
                        if (Model.Stops[i].Type == StopType.Delivery) {
                            load.DeliveryOrder = i;
                            break;
                        }
                    }
                }
            }

            results = new ViewModelWrapper<LoadViewModel>[_loads.Count];
            foreach (var load in _loads) {
                _dispatchService.SaveLoad(load, SaveCompleted);
            }
            base.BusyStart();
        }

        private async Task SaveCompleted(ViewModelWrapper<LoadViewModel> wrapper) {
            for (int i = 0; i < results.Length; i++) {
                if (results[i] == null) {
                    results[i] = wrapper;
                    break;
                }
            }
            if (results[results.Length - 1] != null) {
                await base.SaveCompleted(results.OrderBy(w => w.Status).Last()); // Gets Failed result
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        public void Dispose() {
            browser.Dispose();
        }

        private void stopsListView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is ListBoxItem) {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void stopsListView_OnDrop(object sender, DragEventArgs e) {
            StopViewModel droppedData = e.Data.GetData(typeof(StopViewModel)) as StopViewModel;
            StopViewModel target = ((ListBoxItem)sender).DataContext as StopViewModel;

            int removedIdx = stopsListView.Items.IndexOf(droppedData);
            int targetIdx = stopsListView.Items.IndexOf(target);

            if (removedIdx == targetIdx) {
                return;
            }

            if (removedIdx < targetIdx) {
                Model.Stops.Insert(targetIdx + 1, droppedData);
                Model.Stops.RemoveAt(removedIdx);
            } else {
                int remIdx = removedIdx + 1;
                if (Model.Stops.Count + 1 > remIdx) {
                    Model.Stops.Insert(targetIdx, droppedData);
                    Model.Stops.RemoveAt(remIdx);
                }
            }

            // страница ждёт window.reorder([{ loadId, type }…]) (V1 — EvalScript("reorder(JSON.parse('…'))") со всеми полями StopViewModel)
            string stops = JsonConvert.SerializeObject(Model.Stops.Select(s => new { loadId = s.LoadId, type = (int)s.Type }));
            _ = browser.ExecuteScriptAsync("reorder(" + stops + ")");
        }
    }
}