using AutoCall.Common.Enums;
using AutoDispatch.Data;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels;
using System.Linq;

namespace AutoCall.WPF.UserControls.Boards {
    public class DispatchCommands {

        private DispatchActions da;
        private IDispatchControl dc;

        #region [ Commands ]

        public Command CreateLoadCommand { get; set; }
        public Command GenerateLoadCommand { get; set; }

        public Command CarrierDispatchAtPickUpCommand { get; set; }
        public Command CarrierDispatchPickedUpCommand { get; set; }
        public Command CarrierDispatchInTransitCommand { get; set; }
        public Command CarrierDispatchMoveToDestinationWarehouseCommand { get; set; }
        public Command CarrierDispatchAtDeliveryCommand { get; set; }
        public Command CarrierDispatchDeliveredCommand { get; set; }

        public Command BrokerDispatchAtPickUpCommand { get; set; }
        public Command BrokerDispatchPickedUpCommand { get; set; }
        public Command BrokerDispatchDeliveredCommand { get; set; }

        public Command ViewTrailerCommand { get; set; }
        public Command ViewCarrierCommand { get; set; }
        public Command ViewHistoryCarrierCommand { get; set; }

        public Command SetDestinationWarehouseCommand { get; set; }
        public Command MoveToWarehouseCommand { get; set; }
        public Command MoveToYardCommand { get; set; }
        public Command MoveToTrailerCommand { get; set; }
        public Command MoveToAccountingCommand { get; set; }
        public Command MoveToAccountingAsDeliveredCommand { get; set; }
        public Command MoveToAccountingTNUCommand { get; set; }

        public Command EditLoadCommand { get; set; }
        public Command EditCarrierCommand { get; set; }
        public Command EditDatDriverCommand { get; set; }
        public Command EditComments1Command { get; set; }
        public Command EditComments2Command { get; set; }
        public Command EditConsolidationCommand { get; set; }
        public Command EditExpensesCommand { get; set; }
        public Command EditGainsCommand { get; set; }
        public Command EditConfirmationCommand { get; set; }
        public Command EditLineHaulCommand { get; set; }
        public Command EditCheckCommand { get; set; }
        public Command EditInvoiceCommand { get; set; }

        public Command ChangeLocationCommand { get; set; }

        public Command ViewHistoryLocationsCommand { get; set; }
        public Command ViewHistoryChangesCommand { get; set; }
        public Command CopySummaryCommand { get; set; }
        public Command ShowTripCommand { get; set; }
        public Command ShowRatesCommand { get; set; }
        public Command PreviewImagesCommand { get; set; }

        public Command UnassignCommand { get; set; }
        public Command UndoCommand { get; set; }

        public Command DeleteLoadCommand { get; set; }
        public Command DeleteConfirmationCommand { get; set; }
        public Command DeleteBOLCommand { get; set; }
        public Command DeletePODCommand { get; set; }
        public Command DeletePhotoCommand { get; set; }

        public Command CreateInvoiceCommand { get; set; }
        public Command CreateConfirmationCommand { get; set; }
        public Command CreateBOLCommand { get; set; }
        public Command IssuesCommand { get; set; }
        public Command BondCommand { get; set; }

        public Command SendConfirmationCommand { get; set; }
        public Command SendInvoiceCommand { get; set; }

        public Command ResendConfirmationCommand { get; set; }
        public Command ResendInvoiceCommand { get; set; }

        public Command AssignDriverCommand { get; set; }
        public Command AssignTrailerCommand { get; set; }
        public Command AssignCarrierCommand { get; set; }
        public Command AssignBrokerLoadCommand { get; set; }

        public Command AttachConfirmationCommand { get; set; }
        public Command AttachPhotoCommand { get; set; }
        public Command AttachBOLCommand { get; set; }
        public Command AttachPODCommand { get; set; }
        public Command AttachInvoiceCommand { get; set; }
        public Command AttachCheckCommand { get; set; }

        public Command PaidCommand { get; set; }
        public Command UnpaidCommand { get; set; }
        public Command OpenPhotoCommand { get; set; }
        public Command OpenCheckCommand { get; private set; }
        public Command OpenConfirmationCommand { get; set; }
        public Command OpenBOLCommand { get; set; }
        public Command OpenPODCommand { get; set; }
        public Command OpenInvoiceCommand { get; set; }
        public Command OpenWebSetupPacketCommand { get; set; }
        public Command OpenTrailerSchemeCommand { get; set; }
        public Command OpenPaperworkCommand { get; set; }

        public Command OpenTrailerCommand { get; set; }

        public Command EnableCarrierViewTrackerCommand { get; set; }
        public Command DisableCarrierViewTrackerCommand { get; set; }
        public Command SyncCarrierViewTrackerCommand { get; set; }

        public Command GoToLinkedWrapperCommand { get; set; }
        public Command ParseBrokerConfirmationCommand { get; set; }
        public Command ParseCarrierConfirmationCommand { get; set; }

        #endregion [ Commands ]

        #region [ .ctor ]

        public DispatchCommands(DispatchActions dispatchActions, IDispatchControl dispatchControl) {

            da = dispatchActions;
            dc = dispatchControl;

            var myRole = (RoleName)UserSession.Current.Roles;

            this.ViewTrailerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent,
                w => this.da.ViewTrailerAction(w));
            this.ViewCarrierCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent,
                w => this.da.ViewCarrierAction(w));
            this.CreateLoadCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CreateLoadAction(), extraRoles: new[] { RoleName.Agent });
            this.GenerateLoadCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.GenerateLoadAction(), extraRoles: new[] { RoleName.Agent });

            this.CarrierDispatchAtPickUpCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CarrierDispatchAtPickUpAction(w),
                w => w.Load.ShipmentId == null && w.Load.ContainerType == ContainerType.None && 
                    (w.Load.Status == LoadStatus.Assigned || w.Load.Status == LoadStatus.Waiting1));
            this.CarrierDispatchPickedUpCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CarrierDispatchPickedUpAction(w),
                w => w.Load.ShipmentId == null && w.Load.ContainerType == ContainerType.None && w.Load.Status == LoadStatus.AtPickUp);
            this.CarrierDispatchInTransitCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CarrierDispatchInTransitAction(w),
                w => w.Load.ShipmentId == null && w.Load.ContainerType == ContainerType.None && w.Load.Status == LoadStatus.PickedUp);
            this.CarrierDispatchMoveToDestinationWarehouseCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CarrierDispatchMoveToDestinationWarehouseAction(w),
                w => w.Load.ShipmentId == null &&
                    (w.Load.Status == LoadStatus.InTransit || w.Load.Status == LoadStatus.Waiting2) &&
                    (w.Load.DestinationWarehouseId != null && w.Load.WarehouseId == null));
            this.CarrierDispatchAtDeliveryCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CarrierDispatchAtDeliveryAction(w),
                w => w.Load.ShipmentId == null && 
                    (w.Load.Status == LoadStatus.InTransit || w.Load.Status == LoadStatus.Waiting2) &&
                    (w.Load.DestinationWarehouseId == null || w.Load.DestinationWarehouseId == w.Load.WarehouseId));
            this.CarrierDispatchDeliveredCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.CarrierDispatchDeliveredAction(w),
                w => w.Load.ShipmentId == null && 
                    (w.Load.Status == LoadStatus.AtDelivery || w.Load.Status == LoadStatus.Waiting2) &&
                    (w.Load.DestinationWarehouseId == null || w.Load.DestinationWarehouseId == w.Load.WarehouseId));
            this.BrokerDispatchAtPickUpCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                w => this.da.BrokerDispatchAtPickUpAction(w),
                w => w.Load.BOLFileView != null &&
                     w.Load.Status == LoadStatus.Assigned || w.Load.Status == LoadStatus.Waiting1,
                extraRoles: new[] { RoleName.Agent });
            this.BrokerDispatchPickedUpCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse,
                w => this.da.BrokerDispatchPickedUpAction(w),
                w => w.Load.BOLFileView != null &&
                     w.Load.Status == LoadStatus.AtPickUp,
                extraRoles: new[] { RoleName.Agent });
            this.BrokerDispatchDeliveredCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse,
                w => this.da.BrokerDispatchDeliveredAction(w),
                w => w.Load.Status == LoadStatus.InTransit || w.Load.Status == LoadStatus.AtDelivery || w.Load.Status == LoadStatus.Waiting2,
                extraRoles: new[] { RoleName.Agent });

            this.AssignCarrierCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.AssignCarrierAction,
                w => w.Load.ParentId == null, extraRoles: new[] { RoleName.Agent });
            this.AssignDriverCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.AssignDriverAction,
                w => w.Load.ContainerType == ContainerType.None || w.Load.ContainerType == ContainerType.Quantity);
            this.AssignTrailerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.AssignTrailerAction,
                w => w.Load.ContainerType == ContainerType.None || w.Load.ContainerType == ContainerType.Quantity);
            this.AssignBrokerLoadCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.AssignBrokerLoadAction,
                extraRoles: new[] { RoleName.Agent });

            this.AttachCheckCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Accounting, 
                this.da.AttachCheckAction, w => w.Load.CheckFile == null, 
                this.da.MultiAttachCheckAction, wList => wList.All(w => w.Load.CheckFile == null));
            this.AttachConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.AttachConfirmationAction,
                w => w.Load.ConfirmationFile == null);
            this.AttachInvoiceCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.AttachInvoiceAction,
                w => w.Load.InvoiceFileView == null);
            this.AttachPhotoCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.AttachPhotoAction,
                w => w.Load.PhotoFileView == null);
            this.AttachBOLCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.AttachBOLAction,
                w => w.Load.BOLFileView == null);
            this.AttachPODCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.AttachPODAction,
                w => w.Load.PODFileView == null);
            this.ChangeLocationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.ChangeLocationAction);
            this.CreateBOLCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.CreateBOLAction,
                w => w.Load.ConfirmationFile != null && w.Load.BOLFileView == null, extraRoles: new[] { RoleName.Agent });
            this.IssuesCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.IssuesAction,
                w => w.Load.Status == LoadStatus.Billed || w.Load.Status == LoadStatus.Delivered);
            this.BondCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.BondAction,
                   w => w.Load.Status == LoadStatus.Issues || w.Load.Status == LoadStatus.Billed || w.Load.Status == LoadStatus.Delivered);
            this.CreateConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.CreateConfirmationAction,
                w => w.Load.ParentId == null && w.Load.DatCompanyId != null && w.Load.ConfirmationFile == null, extraRoles: new[] { RoleName.Agent });
            this.CreateInvoiceCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.CreateInvoiceAction,
                w => w.Load.InvoiceFileView == null && w.Load.ParentId == null);
            this.DeleteBOLCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.DeleteBOLAction,
                w => w.Load.BOLFileView != null);
            this.DeletePODCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.DeletePODAction,
                w => w.Load.PODFileView != null);
            this.DeleteConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Owner, this.da.DeleteConfirmationAction,
                w => w.Load.ConfirmationFile != null);
            this.DeletePhotoCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.DeletePhotoAction,
                w => w.Load.PhotoFileView != null);
            this.DeleteLoadCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Owner, this.da.DeleteLoadAction,
                w => true, this.da.DeleteLoadsAction, w => true);
            this.EditCarrierCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditCarrierAction);
            this.EditDatDriverCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditDatDriverAction,
                w => w.Load.DatCompanyId != null && w.Load.DatDriverId != null);
            this.EditComments1Command = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditComments1Action,
                extraRoles: new[] { RoleName.Agent });
            this.EditComments2Command = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditComments2Action,
                extraRoles: new[] { RoleName.Agent });
            this.EditConsolidationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditConsolidationAction,
                extraRoles: new[] { RoleName.Agent });
            this.EditConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditConfirmationAction,
                w => w.Load.DatCompanyId != null && w.Load.ConfirmationFile != null, extraRoles: new[] { RoleName.Agent });
            this.EditExpensesCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditExpensesAction,
                extraRoles: new[] { RoleName.Agent });
            this.EditGainsCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditGainsAction,
                extraRoles: new[] { RoleName.Agent });
            this.EditInvoiceCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.CreateInvoiceAction,
                w => w.Load.InvoiceFileView != null);
            this.EditLoadCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent, w => this.da.EditLoadAction(w));
            this.EditLineHaulCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EditLineHaulAction,
                extraRoles: new[] { RoleName.Agent });
            this.MoveToAccountingCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.MoveToAccountingAction);
            this.MoveToAccountingAsDeliveredCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.MoveToAccountingAsDeliveredAction,
                w => w.Load.Status == LoadStatus.Delivered || w.Load.Status == LoadStatus.Billed);
            this.MoveToAccountingTNUCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.MoveToAccountingTNUAction);
            this.MoveToTrailerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.AssignTrailerAction,
                w => w.Load.ShipmentId == null && w.Trailer?.YardId == null && (w.Load.ContainerType == ContainerType.None || w.Load.ContainerType == ContainerType.Quantity));
            this.SetDestinationWarehouseCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.SetDestinationWarehouseAction,
                w => w.Load.ShipmentId == null);
            this.MoveToWarehouseCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.MoveToWarehouseAction,
                w => w.Load.ShipmentId == null && (w.Load.ContainerType == ContainerType.None || w.Load.ContainerType == ContainerType.Quantity));
            this.MoveToYardCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.MoveToYardAction,
                w => w.Load.ShipmentId == null && w.Trailer != null && (w.Load.ContainerType == ContainerType.None || w.Load.ContainerType == ContainerType.Quantity));
            this.OpenBOLCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.OpenBOLAction,
                w => w.Load.BOLFileView != null, extraRoles: new[] { RoleName.Agent }, checkBoard: false);
            this.OpenPODCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent, this.da.OpenPODAction,
                w => w.Load.PODFileView != null, checkBoard: false);
            this.OpenConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.OpenConfirmationAction,
                w => w.Load.ConfirmationFile != null, checkBoard: false);
            this.OpenInvoiceCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.OpenInvoiceAction,
                w => w.Load.InvoiceFileView != null, checkBoard: false);
            this.OpenPhotoCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent, this.da.OpenPhotoAction,
                w => w.Load.PhotoFileView != null, checkBoard: false);
            this.OpenCheckCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.OpenCheckAction,
                w => w.Load.CheckFile != null, checkBoard: false);
            this.OpenTrailerSchemeCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.OpenTrailerSchemeAction,
                w => w.Trailer?.SchemeFile != null, checkBoard: false);
            this.OpenPaperworkCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.OpenPaperworkAction,
                w => w.Load.PaperworkFileView != null, checkBoard: false);
            this.OpenTrailerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.OpenTrailerAction,
                w => w.Trailer != null, checkBoard: false);
            this.OpenWebSetupPacketCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.OpenWebSetupPacketAction, checkBoard: false);
            this.PaidCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Accounting, 
                this.da.PaidAction, w => true, this.da.MultiPaidAction, wList =>  true);
            this.UnpaidCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Accounting, 
                this.da.UnpaidAction, w => true, this.da.MultiUnpaidAction, wList => true);
            this.ResendConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.SendConfirmationAction,
                w => w.Load.DatCompanyId != null && w.Load.ConfirmationFile != null && w.Load.EmailConfirmationSentDate != null, extraRoles: new[] { RoleName.Agent });
            this.ResendInvoiceCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.SendInvoiceAction,
                w => w.Load.InvoiceFileView != null && w.Load.EmailInvoiceSentDateView != null && w.Load.ParentId == null);
            this.SendConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.SendConfirmationAction,
                w => w.Load.DatCompanyId != null && w.Load.ConfirmationFile != null && w.Load.EmailConfirmationSentDate == null, extraRoles: new[] { RoleName.Agent });
            this.SendInvoiceCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.SendInvoiceAction,
                w => w.Load.InvoiceFileView != null && w.Load.EmailInvoiceSentDateView == null && w.Load.ParentId == null);
            this.CopySummaryCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.User, this.da.CopySummaryAction);
            this.ShowTripCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent, this.da.ShowTripAction);
            this.ShowRatesCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.ShowRatesAction);
            this.PreviewImagesCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.User, this.da.PreviewImagesAction);
            this.UnassignCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.UnassignAction);
            this.UndoCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.UndoAction);
            this.ViewHistoryLocationsCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Agent, this.da.ViewHistoryLocationsAction);
            this.ViewHistoryChangesCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.ViewHistoryChangesAction);
            this.ViewHistoryCarrierCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Warehouse, this.da.ViewHistoryCarrierAction);
            this.EnableCarrierViewTrackerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.EnableCarrierViewTrackerAction,
                w => w.ActualCompany?.CarrierViewToken != null && w.Load.TrackerId == null);
            this.DisableCarrierViewTrackerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.DisableCarrierViewTrackerAction,
                w => w.ActualCompany?.CarrierViewToken != null && w.Load.TrackerId != null);
            this.SyncCarrierViewTrackerCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.SyncCarrierViewTrackerAction,
                w => w.ActualCompany?.CarrierViewToken != null && w.Load.TrackerId != null);
            this.GoToLinkedWrapperCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher, this.da.GoToLinkedWrapperAction,
                w => w.LinkedWrapperCount == 1);
            this.ParseBrokerConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                (w) => this.da.ParseConfirmationAction(DispatchBoardType.Broker));
            this.ParseCarrierConfirmationCommand = new Command(this.dc?.DispatchBoardType, myRole, RoleName.Dispatcher,
                (w) => this.da.ParseConfirmationAction(DispatchBoardType.Carrier));
        }

        #endregion [ .ctor ]

        #region [ Public ]

        public void OnCanExecuteChanged() {
            foreach (Command command in this.GetType().GetProperties().Where(e => e.PropertyType == typeof(Command)).Select(e => e.GetValue(this))) {
                command?.OnCanExecuteChanged();
            }
        }

        #endregion [ Public ]
    }
}