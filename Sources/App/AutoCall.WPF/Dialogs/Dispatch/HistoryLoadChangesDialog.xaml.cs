using System;
using AutoDispatch.Logic.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common.Collections;
using AutoCall.Common.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class HistoryLoadChangesDialog : BaseDispatchDialog {

        #region [ BaseDispatchDialog ]

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        #endregion [ BaseDispatchDialog ]

        #region [ Properties ]

        private DispatchService _service;
        private IMapper _mapper;

        public DispatchActions DispatchActions { get; }
        public DispatchCommands DispatchCommands { get; }
        
        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public HistoryLoadChangesViewModel Model {
            get { return DataContext as HistoryLoadChangesViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        public HistoryLoadChangesDialog(DispatchService service, LoadViewModel load) {
            _service = service;
            _mapper = App.Services.GetRequiredService<IMapper>();

            Model = new HistoryLoadChangesViewModel() {
                Origin = load
            };

            // don't use Dispatch Control because loads have different DispatchTypes
            DispatchActions = new DispatchActions(null);
            DispatchCommands = new DispatchCommands(DispatchActions, null);
            // NOTES: перенес dc во ViewModel, по какой то причине первая ссылка на Confirmation не срабатывает
            Model.dc = DispatchCommands;

            InitializeComponent();
            Title += $" #{load.IndexString}";
        }

        #endregion [ .ctor ]

        #region [ private ]

        private void Initialize() {
            _service.LoadHistoryChangesLoad(Model.Origin.EntityId, LoadHistoryChangesLoad_Completed);
            base.BusyStart();
        }

        private async Task LoadHistoryChangesLoad_Completed(ViewModelWrapper<List<HistoryObjectViewModel>> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await LoadHistoryChangesLoad_Completed(wrapper));
                return;
            }
            loading.Visibility = Visibility.Collapsed;
            await base.SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                var loadDtos = new List<LoadDto>();
                foreach (HistoryObjectViewModel history in wrapper.Data) {
                    try {
                        loadDtos.Add(ZipExtension.DeserializeObject<LoadDto>(history.Data));
                    } catch (Exception e) {
                    }
                }
                Model.Wrappers
                    .AddRange(
                        loadDtos
                            .Select(h => new Wrapper1LoadViewModel() {
                                Load = LoadConverter.ToVm(h),
                                ActualCompany = DispatchBoardViewModel.Instance.ActualCompanies.FirstOrDefault(e => e.EntityId == h.ActualCompanyId),
                                Trailer = h.TrailerId != null ? DispatchBoardViewModel.Instance.Trailers
                                    .DefaultIfEmpty(new TrailerViewModel() { Number = "unknown" })
                                    .FirstOrDefault(e => e.EntityId == h.TrailerId) : null,
                                Warehouse = h.WarehouseId != null ? DispatchBoardViewModel.Instance.Warehouses
                                    .DefaultIfEmpty(new WarehouseViewModel() { Name = "unknown" })
                                    .FirstOrDefault(e => e.EntityId == h.WarehouseId) : null,
                            })
                            .GroupBy(h => h.Load.Status +
                                (h.Load.ActualCompanyId?.GetHashCode()) +
                                (h.Load.RecordId?.GetHashCode()) +
                                (h.Load.ContainerType.GetHashCode()) +
                                (h.Load.PickupFrom.GetHashCode()) +
                                (h.Load.DeliveryFrom.GetHashCode()) +
                                (h.Load.OriginCity.GetHashCode()) +
                                (h.Load.DestinationCity.GetHashCode()) +
                                (h.Load.TrailerId?.GetHashCode() ?? 0) +
                                (h.Load.WarehouseId?.GetHashCode() ?? 0) +
                                (h.Load.ConfirmedDate?.GetHashCode() ?? 0) +
                                (h.Load.EmailConfirmationSentDate?.GetHashCode() ?? 0) +
                                (h.Load.ConfirmationDate?.GetHashCode() ?? 0) +
                                (h.Load.EmailConfirmationSentDate?.GetHashCode() ?? 0) +
                                (h.Load.PODDate?.GetHashCode() ?? 0) +
                                (h.Load.BOLDate?.GetHashCode() ?? 0) +
                                (h.Load.InvoiceDate?.GetHashCode() ?? 0) +
                                (h.Load.EmailInvoiceSentDate?.GetHashCode() ?? 0) +
                                (h.Load.CheckDate?.GetHashCode() ?? 0) +
                                (h.Load.Comments?.GetHashCode() ?? 0) +
                                (h.Load.Comments2?.GetHashCode() ?? 0) +
                                (h.Load.LineHaul?.GetHashCode() ?? 0) +
                                (h.Load.Rate?.GetHashCode() ?? 0) +
                                (h.Load.Payout?.GetHashCode() ?? 0) +
                                (h.Load.GainsTotal?.GetHashCode() ?? 0) +
                                (h.Load.TrackerId?.GetHashCode() ?? 0) +
                                (h.Load.CarrierViewPhone?.GetHashCode() ?? 0) +
                                (h.Load.ExpensesTotal?.GetHashCode() ?? 0) +
                                (h.Load.IsDeleted.GetHashCode()))
                            .Select(group => group.OrderBy(h => h.Load.LastModifiedDate).Last())
                            .OrderByDescending(c => c.Load.LastModifiedDate));
            }
        }

        #endregion [ private ]

        #region [ Event Handlers ]

        private void HistoryLoadChangesDialog_OnLoaded(object sender, RoutedEventArgs e) {
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

        private void HistoryLoadChangesDialog_OnClosed(object sender, EventArgs e) {
            // TODO: saves settions
            //_settings.SetDatCompaniesDialogGridView(gridHistories.GetSettings());
        }

        private void gridHistories_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Close();
        }

        #endregion [ Event Handlers ]
    }
}