using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Common.Collections;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class HistoryLoadLocationsDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private DispatchService _service;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public HistoryLoadLocationsViewModel Model {
            get { return DataContext as HistoryLoadLocationsViewModel; }
            set { DataContext = value; }
        }
   
        /// <summary>
        ///     Constructor </summary>
        public HistoryLoadLocationsDialog(DispatchService service, LoadViewModel load) {
            _service = service;

            InitializeComponent();

            Model = new HistoryLoadLocationsViewModel() {
                Load = load
            };
        }

        private void Initialize() {
            _service.LoadHistoryLocationLoad(Model.Load.EntityId, LoadHistoryLocationLoad_Completed);
            base.BusyStart();
        }

        private async Task LoadHistoryLocationLoad_Completed(ViewModelWrapper<List<HistoryLocationLoadViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadHistoryLocationLoad_Completed(wrapper));
                return;
            }
            await base.SaveCompleted(wrapper, false);
            loading.Visibility = Visibility.Collapsed;
            if (wrapper.Status == ResponseStatus.Success) {
                Model.Histories
                    .AddRange(wrapper.Data.Where(e => e.Location != null).GroupBy(h => h.Location.Latitude + h.Location.Longitude).Select(group => group.OrderBy(h => h.CreatedDate).Last())
                    .Select(h => new HistoryLoadViewModel() { HistoryLocation = h })
                    .OrderByDescending(c => c.HistoryLocation.CreatedDate));

                // Loads histories
                _service.LoadHistoryObjects(Model.Histories.Select(h => h.HistoryLocation.HistoryId).Distinct().ToList(), LoadHistoryLocationLoad_Completed);
            }
        }

        private async Task LoadHistoryLocationLoad_Completed(ViewModelWrapper<List<HistoryObjectViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadHistoryLocationLoad_Completed(wrapper));
                return;
            }
            foreach (HistoryLoadViewModel pair in Model.Histories) {
                foreach (HistoryObjectViewModel historyObject in wrapper.Data) {
                    if (pair.HistoryLocation.HistoryId == historyObject.Id) {
                        try {
                            pair.History = ZipExtension.DeserializeObject<LoadDto>(historyObject.Data);
                        } catch (Exception e) {
                        }
                        break;
                    }
                }
            }
        }

        private void HistoryLoadLocationsDialog_OnLoaded(object sender, RoutedEventArgs e) {
            Initialize();
            // TODO: loads settions
            //try {
            //    var data = _settings.GetDatCompaniesDialogGridView();
            //    if (data != null) {
            //        gridHistories.SetSettings(data);
            //    }
            //} finally {
            //}
        }

        private void HistoryLoadLocationsDialog_OnClosed(object sender, EventArgs e) {
            // TODO: saves settions
            //_settings.SetDatCompaniesDialogGridView(gridHistories.GetSettings());
        }

        private void gridHistories_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}