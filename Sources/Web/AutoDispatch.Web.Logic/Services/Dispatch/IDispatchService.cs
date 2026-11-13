using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Attributes;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Контракт App (V1 IDispatchService, 74 метода — AI/wiki/protocol.md): Api/Dispatch/Request вызывает метод по имени,
///     MinAccess — минимальная роль. Без чата (GetTodayMessages, SendMessage) и ReportSimilarLoads (решено 2026-11-13) </summary>
public interface IDispatchService : IService {

    Task<GetUsersResponseData> GetUsers(GetUsersRequestData request);

    Task<GetModulesResponseData> GetModules(GetModulesRequestData request);

    ProviderEnabledResponseData ProviderEnabled(ProviderEnabledRequestData request);

    ProviderDisabledResponseData ProviderDisabled(ProviderDisabledRequestData request);

    Task<LocationDriverChangedResponseData> LocationDriverChanged(LocationDriverChangedRequestData request);

    Task<LocationTruckChangedResponseData> LocationTruckChanged(LocationTruckChangedRequestData request);

    Task<LocationTrailerChangedResponseData> LocationTrailerChanged(LocationTrailerChangedRequestData request);

    Task<LocationLoadChangedResponseData> LocationLoadChanged(LocationLoadChangedRequestData request);

    Task<LocationDatDriverChangedResponseData> LocationDatDriverChanged(LocationDatDriverChangedRequestData request);

    Task<LoadHistory2ResponseData> LoadHistory2(LoadHistory2RequestData request);

    Task<LoadHistoryLocationDriverResponseData> LoadHistoryLocationDriver(LoadHistoryLocationDriverRequestData request);

    Task<LoadHistoryLocationTruckResponseData> LoadHistoryLocationTruck(LoadHistoryLocationTruckRequestData request);

    Task<LoadHistoryLocationTrailerResponseData> LoadHistoryLocationTrailer(LoadHistoryLocationTrailerRequestData request);

    Task<LoadHistoryLocationLoadResponseData> LoadHistoryLocationLoad(LoadHistoryLocationLoadRequestData request);

    Task<LoadHistoryLocationDatDriverResponseData> LoadHistoryLocationDatDriver(LoadHistoryLocationDatDriverRequestData request);

    Task<LoadHistoryChangesLoadResponseData> LoadHistoryChangesLoad(LoadHistoryChangesLoadRequestData request);

    Task<LoadDriversResponseData> LoadDrivers(LoadDriversRequestData request);

    Task<LoadTrucksResponseData> LoadTrucks(LoadTrucksRequestData request);

    Task<LoadTrailersResponseData> LoadTrailers(LoadTrailersRequestData request);

    Task<LoadWarehousesResponseData> LoadWarehouses(LoadWarehousesRequestData request);

    Task<LoadYardsResponseData> LoadYards(LoadYardsRequestData request);

    Task<LoadLoadsResponseData> LoadLoads(LoadLoadsRequestData request);

    Task<LoadLoadsZipResponseData> LoadLoadsZip(LoadLoadsZipRequestData request);

    Task<LoadHistoryObjectsResponseData> LoadHistoryObjects(LoadHistoryObjectsRequestData request);

    Task<LoadTotalHistoryLoadsResponseData> LoadTotalHistoryLoads(LoadTotalHistoryLoadsRequestData request);

    Task<LoadDatCompaniesResponseData> LoadDatCompanies(LoadDatCompaniesRequestData request);

    Task<LoadDatDriversResponseData> LoadDatDrivers(LoadDatDriversRequestData request);

    Task<LoadSetupPacketsResponseData> LoadSetupPackets(LoadSetupPacketsRequestData request);

    Task<LoadShipperReceiverCompaniesResponseData> LoadShipperReceiverCompanies(LoadShipperReceiverCompaniesRequestData request);

    Task<LoadCustomerCompaniesResponseData> LoadCustomerCompanies(LoadCustomerCompaniesRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SaveDriverResponseData> SaveDriver(SaveDriverRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SaveTruckResponseData> SaveTruck(SaveTruckRequestData request);

    [MinAccess(RoleEnum.Warehouse)]
    Task<SaveTrailerResponseData> SaveTrailer(SaveTrailerRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SaveWarehouseResponseData> SaveWarehouse(SaveWarehouseRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SaveYardResponseData> SaveYard(SaveYardRequestData request);

    Task<FixLoadResponseData> FixLoad(FixLoadRequestData request);

    [MinAccess(RoleEnum.Agent)]
    Task<SaveLoadResponseData> SaveLoad(SaveLoadRequestData request);

    [MinAccess(RoleEnum.Agent)]
    Task<SaveLoadsResponseData> SaveLoads(SaveLoadsRequestData request);

    [MinAccess(RoleEnum.Agent)]
    Task<SaveDatCompanyResponseData> SaveDatCompany(SaveDatCompanyRequestData request);

    [MinAccess(RoleEnum.Agent)]
    Task<SaveDatDriverResponseData> SaveDatDriver(SaveDatDriverRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SaveSetupPacketResponseData> SaveSetupPacket(SaveSetupPacketRequestData request);

    [MinAccess(RoleEnum.Accounting)]
    Task<SaveFactoringCompanyResponseData> SaveFactoringCompany(SaveFactoringCompanyRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteDriverResponseData> DeleteDriver(DeleteDriverRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteTruckResponseData> DeleteTruck(DeleteTruckRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteTrailerResponseData> DeleteTrailer(DeleteTrailerRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteWarehouseResponseData> DeleteWarehouse(DeleteWarehouseRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteYardResponseData> DeleteYard(DeleteYardRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteLoadResponseData> DeleteLoad(DeleteLoadRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteLoadsResponseData> DeleteLoads(DeleteLoadsRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteDatCompanyResponseData> DeleteDatCompany(DeleteDatCompanyRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteDatDriverResponseData> DeleteDatDriver(DeleteDatDriverRequestData request);

    [MinAccess(RoleEnum.Owner)]
    Task<DeleteSetupPacketResponseData> DeleteSetupPacket(DeleteSetupPacketRequestData request);

    Task<DeleteDefectResponseData> DeleteDefect(DeleteDefectRequestData request);

    Task<DeleteDefectRepairResponseData> DeleteDefectRepair(DeleteDefectRepairRequestData request);

    Task<LoadDefectsResponseData> LoadDefects(LoadDefectsRequestData request);

    Task<EventNotificationResponseData> EventNotificationAsync(EventNotificationRequestData request, CancellationTokenSource ts);

    #region [ Files ]

    Task<FilesLoadResponseData> FilesLoad(FilesLoadRequestData request);

    Task<FileLoadResponseData> FileLoad(FileLoadRequestData request);

    [MinAccess(RoleEnum.Agent)]
    Task<FileSaveResponseData> FileSave(FileSaveRequestData request);

    [MinAccess(RoleEnum.Owner)]
    FileDeleteResponseData FileDelete(FileDeleteRequestData request);

    [MinAccess(RoleEnum.Owner)]
    FileRenameResponseData FileRename(FileRenameRequestData request);

    #endregion [ Files ]

    [MinAccess(RoleEnum.Dispatcher)]
    Task<InsuranceLoadResponseData> InsuranceLoad(InsuranceLoadRequestData request);

    Task<BindLoadsResponseData> BindLoads(BindLoadsRequestData request);

    [MinAccess(RoleEnum.Accounting)]
    Task<InvoiceSendResponseData> InvoiceSend(InvoiceSendRequestData request);

    [MinAccess(RoleEnum.Accounting)]
    Task<InvoiceMergeResponseData> InvoiceMerge(InvoiceMergeRequestData request);

    [MinAccess(RoleEnum.Dispatcher, new[] { RoleEnum.Agent })]
    Task<ConfirmationSendResponseData> ConfirmationSend(ConfirmationSendRequestData request);

    [MinAccess(RoleEnum.Dispatcher, new[] { RoleEnum.Agent })]
    Task<ConfirmedSendResponseData> ConfirmedSend(ConfirmedSendRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SetupPacketInviteSendResponseData> SetupPacketInviteSend(SetupPacketInviteSendRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<SetupPacketEditSendResponseData> SetupPacketEditSend(SetupPacketEditSendRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<UndoPreviewResponseData> UndoPreview(UndoPreviewRequestData request);

    [MinAccess(RoleEnum.Dispatcher)]
    Task<UndoApplyResponseData> UndoApply(UndoApplyRequestData request);

    Task<GetStatisticsResponseData> GetStatistics(GetStatisticsRequestData request);

    Task<GetDatCompanyStatisticsResponseData> GetDatCompanyStatictics(GetDatCompanyStatisticsRequestData request);

    Task<SendSmsResponseData> SendSms(SendSmsRequestData request);

    Task<SendSchemeEmailResponseData> SendSchemeEmail(SendSchemeEmailRequestData request);

    Task<BnsfRefreshResponseData> BnsfRefresh(BnsfRefreshRequestData request);

    Task<EnableCarrierViewTrackerResponseData> EnableCarrierViewTracker(EnableCarrierViewTrackerRequestData request);

    Task<DisableCarrierViewTrackerResponseData> DisableCarrierViewTracker(DisableCarrierViewTrackerRequestData request);

    Task<SyncCarrierViewTrackerResponseData> SyncCarrierViewTracker(SyncCarrierViewTrackerRequestData request);
}
