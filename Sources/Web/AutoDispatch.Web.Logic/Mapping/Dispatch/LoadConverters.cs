using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoCall.Common.Enums;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

/// <summary>
///     V1 Load_DtoToModel: ActualCompanyId контракта → DispatchCompanyId, geography → decimal-пары.
///     Также используется для сериализации снимков истории </summary>
public class Load_DtoToModel : BaseObject_DtoToModel<LoadDto, Load> {
    protected override Load ConvertCore(LoadDto source) {
        if (source.OriginLocation == null) {
            throw new Exception($"[{source.IndexString}]: Require Origin");
        }
        if (source.DestinationLocation == null) {
            throw new Exception($"[{source.IndexString}]: Require Destination");
        }
        var destination = base.ConvertCore(source);
        destination.OldOrderId = source.OldOrderId;
        destination.DispatchCompanyId = source.ActualCompanyId;
        destination.Index = source.Index;
        destination.IndexExtra = source.IndexExtra;
        destination.IndexPrefix = source.IndexPrefix;
        destination.TrailerId = source.TrailerId;
        destination.DestinationWarehouseId = source.DestinationWarehouseId;
        destination.WarehouseId = source.WarehouseId;
        destination.DatDriverId = source.DatDriverId;
        destination.DatCompanyId = source.DatCompanyId;
        destination.HistoryDriverId = source.HistoryDriverId;
        destination.HistoryTruckId = source.HistoryTruckId;
        destination.HistoryTrailerId = source.HistoryTrailerId;
        destination.HistoryAssignedTrailerId = source.HistoryAssignedTrailerId;
        destination.HistoryWarehouseId = source.HistoryWarehouseId;
        destination.HistoryDatCompanyId = source.HistoryDatCompanyId;
        destination.HistoryDatDriverId = source.HistoryDatDriverId;
        destination.OriginCity = source.OriginCity;
        destination.DestinationCity = source.DestinationCity;
        destination.CurrentOriginUtcOffset = source.CurrentOriginUtcOffset;
        destination.CurrentDestinationUtcOffset = source.CurrentDestinationUtcOffset;
        destination.OriginAddress = source.OriginAddress;
        destination.DestinationAddress = source.DestinationAddress;
        destination.OriginLatitude = (decimal)source.OriginLocation.Latitude;
        destination.OriginLongitude = (decimal)source.OriginLocation.Longitude;
        destination.DestinationLatitude = (decimal)source.DestinationLocation.Latitude;
        destination.DestinationLongitude = (decimal)source.DestinationLocation.Longitude;
        destination.Distance = source.Distance;
        LocationConverter.SetCurrent(destination, source.CurrentLocation);
        destination.DeliveryFrom = source.DeliveryFrom;
        destination.DeliveryTo = source.DeliveryTo;
        destination.PickupFrom = source.PickupFrom;
        destination.PickupTo = source.PickupTo;
        destination.Comments = source.Comments;
        destination.Comments2 = source.Comments2;
        destination.Consolidation = source.Consolidation;
        destination.ShipperId = source.ShipperId;
        destination.HistoryShipmentId = source.HistoryShipmentId;
        destination.ReceiverId = source.ReceiverId;
        destination.PickupNumber = source.PickupNumber;
        destination.DeliveryNumber = source.DeliveryNumber;
        destination.ShowPickupAddress = source.ShowPickupAddress;
        destination.Equipment = source.Equipment;
        destination.PalletType = source.PalletType;
        destination.BookedDate = source.BookedDate;
        destination.BoardChanged = source.BoardChanged;
        destination.CarrierViewTracking = source.CarrierViewTracking;
        destination.LineHaul = source.LineHaul;
        destination.DatRate = source.DatRate;
        destination.Expenses = source.Expenses;
        destination.Gains = source.Gains;
        destination.Currency = source.Currency;
        destination.QuikPay = source.QuikPay;
        destination.LoadType = source.LoadType;
        destination.Length = source.Length;
        destination.Weight = source.Weight;
        destination.Pallets = source.Pallets;
        destination.Spots = source.Spots;
        destination.WeightType = source.WeightType;
        destination.Commodity = source.Commodity;
        destination.DivideType = (byte)source.DivideType;
        destination.ContainerType = (byte)source.ContainerType;
        destination.ParentId = source.ParentId;
        destination.ShipmentId = source.ShipmentId;
        destination.TrailerType = (byte)source.TrailerType;
        destination.Temperature = source.Temperature;
        destination.RecordId = source.RecordId;
        destination.BOLNumber = source.BOLNumber;
        destination.TrackerId = source.TrackerId;
        destination.CarrierViewPhone = source.CarrierViewPhone;
        destination.CarrierViewLink = source.CarrierViewLink;
        destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
        destination.CarrierViewStatus = source.CarrierViewStatus;
        destination.CarrierViewStatusesJson = source.CarrierViewStatusesJson;
        destination.CarrierViewLate = source.CarrierViewLate;
        destination.CarrierViewPickupId = source.CarrierViewPickupId;
        destination.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
        destination.CarrierViewPickupComment = source.CarrierViewPickupComment;
        destination.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
        destination.Background = source.Background;
        destination.Foreground = source.Foreground;
        destination.BindDate = source.BindDate;
        destination.AssignedTrailerDate = source.AssignedTrailerDate;
        destination.Status = (byte)source.Status;
        destination.CustomStatusId = source.CustomStatusId;
        if (source.LastStatusChangedDate > new DateTime(1990, 1, 1)) {
            destination.LastStatusChangedDate = source.LastStatusChangedDate;
        } else {
            destination.LastStatusChangedDate = new DateTime(1990, 1, 1);
        }
        unchecked {
            destination.LastRateChanged = (byte?)source.LastRateChanged;
        }
        destination.PickupOrder = source.PickupOrder;
        destination.DeliveryOrder = source.DeliveryOrder;
        destination.Board = (byte)source.BoardType;

        destination.WasAtPickup = source.WasAtPickup;
        destination.WasAtDelivery = source.WasAtDelivery;

        destination.BOLDate = source.BOLDate;
        destination.BOLFile = source.BOLFile;
        destination.PODDate = source.PODDate;
        destination.PODFile = source.PODFile;
        destination.LumperDate = source.LumperDate;
        destination.LumperFile = source.LumperFile;
        destination.PaperworkDate = source.PaperworkDate;
        destination.PaperworkFile = source.PaperworkFile;
        destination.ConfirmationFile = source.ConfirmationFile;
        destination.ConfirmationDate = source.ConfirmationDate;
        destination.ConfirmedFile = source.ConfirmedFile;
        destination.ConfirmedDate = source.ConfirmedDate;
        destination.EmailConfirmationSentDate = source.EmailConfirmationSentDate;
        if (source.EmailConfirmationSent != null) {
            destination.EmailConfirmationSent = DispatchJson.Serialize(source.EmailConfirmationSent);
        }

        destination.TrailerSchemeDate = source.TrailerSchemeDate;
        destination.TrailerSchemeFile = source.TrailerSchemeFile;
        destination.TrailerSchemeId = source.TrailerSchemeId;

        destination.InvoiceNumber = source.InvoiceNumber;
        destination.InvoiceFile = source.InvoiceFile;
        destination.InvoiceDate = source.InvoiceDate;
        destination.EmailInvoiceSentDate = source.EmailInvoiceSentDate;
        if (source.EmailInvoiceSent != null) {
            destination.EmailInvoiceSent = DispatchJson.Serialize(source.EmailInvoiceSent);
        }

        destination.CheckNumber = source.CheckNumber;
        destination.CheckFile = source.CheckFile;
        destination.CheckDate = source.CheckDate;
        destination.Payout = source.Payout;

        destination.NewScheduledAppointment = source.NewScheduledAppointment;

        destination.PhotoFile = source.PhotoFile;

        // companies
        destination.DatBrokerId = source.DatBrokerId;
        destination.CustomerId = source.CustomerId;

        destination.DeliveredDate = source.DeliveredDate;

        destination.Layout = LoadLayoutConverter.ToModel(source.Layout);
        return destination;
    }
}

public class Load_ModelToDto : BaseObject_ModelToDto<Load, LoadDto> {
    protected override LoadDto ConvertCore(Load source) {
        var destination = base.ConvertCore(source);
        destination.OldOrderId = source.OldOrderId;
        destination.ActualCompanyId = source.DispatchCompanyId;
        destination.Index = source.Index;
        destination.IndexExtra = source.IndexExtra;
        destination.IndexPrefix = source.IndexPrefix;
        destination.OriginLocation = LocationConverter.ToDto(source.OriginLatitude, source.OriginLongitude);
        destination.DestinationLocation = LocationConverter.ToDto(source.DestinationLatitude, source.DestinationLongitude);
        destination.CurrentLocation = LocationConverter.ToDto(source.CurrentLatitude, source.CurrentLongitude);
        destination.OriginCity = source.OriginCity;
        destination.DestinationCity = source.DestinationCity;
        destination.OriginAddress = source.OriginAddress;
        destination.DestinationAddress = source.DestinationAddress;
        destination.CurrentOriginUtcOffset = source.CurrentOriginUtcOffset;
        destination.CurrentDestinationUtcOffset = source.CurrentDestinationUtcOffset;
        destination.Distance = source.Distance;
        destination.DeliveryFrom = source.DeliveryFrom;
        destination.DeliveryTo = source.DeliveryTo;
        destination.PickupFrom = source.PickupFrom;
        destination.PickupTo = source.PickupTo;
        destination.ShipperId = source.ShipperId;
        destination.ReceiverId = source.ReceiverId;
        // навигации LinqToDB не грузит — имена компаний дописывает LoadCompanyNames (V1 IncludeCompaniesAsync)
        destination.ShipperCompany = source.ShipperCompany1?.Name;
        destination.ReceiverCompany = source.ReceiverCompany1?.Name;
        destination.ShipperContacts = source.ShipperCompany1?.Contacts;
        destination.ReceiverContacts = source.ReceiverCompany1?.Contacts;
        destination.PickupNumber = source.PickupNumber;
        destination.DeliveryNumber = source.DeliveryNumber;
        destination.ShowPickupAddress = source.ShowPickupAddress;
        destination.Comments = source.Comments;
        destination.Comments2 = source.Comments2;
        destination.Consolidation = source.Consolidation;
        destination.Equipment = source.Equipment;
        destination.PalletType = source.PalletType;
        destination.BookedDate = source.BookedDate;
        destination.BoardChanged = source.BoardChanged;
        destination.CarrierViewTracking = source.CarrierViewTracking;
        destination.LineHaul = source.LineHaul;
        destination.DatRate = source.DatRate;
        destination.Expenses = source.Expenses;
        destination.Gains = source.Gains;
        destination.Currency = source.Currency;
        destination.QuikPay = source.QuikPay;
        destination.LoadType = source.LoadType;
        destination.Length = source.Length;
        destination.Weight = source.Weight;
        destination.RecordId = source.RecordId;
        destination.BOLNumber = source.BOLNumber;
        destination.TrackerId = source.TrackerId;
        destination.CarrierViewPhone = source.CarrierViewPhone;
        destination.CarrierViewLink = source.CarrierViewLink;
        destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
        destination.CarrierViewStatus = source.CarrierViewStatus;
        destination.CarrierViewStatusesJson = source.CarrierViewStatusesJson;
        destination.CarrierViewLate = source.CarrierViewLate;
        destination.CarrierViewPickupId = source.CarrierViewPickupId;
        destination.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
        destination.CarrierViewPickupComment = source.CarrierViewPickupComment;
        destination.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
        destination.DestinationWarehouseId = source.DestinationWarehouseId;
        destination.WarehouseId = source.WarehouseId;
        destination.TrailerId = source.TrailerId;
        destination.DatDriverId = source.DatDriverId;
        destination.DatCompanyId = source.DatCompanyId;
        destination.HistoryDriverId = source.HistoryDriverId;
        destination.HistoryTruckId = source.HistoryTruckId;
        destination.HistoryTrailerId = source.HistoryTrailerId;
        destination.HistoryAssignedTrailerId = source.HistoryAssignedTrailerId;
        destination.HistoryDatCompanyId = source.HistoryDatCompanyId;
        destination.HistoryDatDriverId = source.HistoryDatDriverId;
        destination.HistoryWarehouseId = source.HistoryWarehouseId;
        destination.Pallets = source.Pallets;
        destination.Spots = source.Spots;
        destination.WeightType = source.WeightType;
        destination.Commodity = source.Commodity;
        destination.DivideType = (DivideType)source.DivideType;
        destination.ContainerType = (ContainerType)source.ContainerType;
        destination.ParentId = source.ParentId;
        destination.ShipmentId = source.ShipmentId;
        destination.HistoryShipmentId = source.HistoryShipmentId;
        destination.TrailerType = (TrailerType)source.TrailerType;
        destination.Temperature = source.Temperature;
        destination.Background = source.Background;
        destination.Foreground = source.Foreground;
        destination.BindDate = source.BindDate;
        destination.AssignedTrailerDate = source.AssignedTrailerDate;
        destination.Status = (LoadStatus)source.Status;
        destination.CustomStatusId = source.CustomStatusId;
        destination.LastStatusChangedDate = source.LastStatusChangedDate;
        unchecked {
            destination.LastRateChanged = (sbyte?)source.LastRateChanged;
        }
        destination.Layout = LoadLayoutConverter.ToDto(source.Layout);
        destination.PickupOrder = source.PickupOrder;
        destination.DeliveryOrder = source.DeliveryOrder;
        destination.BoardType = (DispatchBoardType)source.Board;

        destination.WasAtPickup = source.WasAtPickup;
        destination.WasAtDelivery = source.WasAtDelivery;

        destination.BOLDate = source.BOLDate;
        destination.BOLFile = source.BOLFile;
        destination.PODDate = source.PODDate;
        destination.PODFile = source.PODFile;
        destination.LumperDate = source.LumperDate;
        destination.LumperFile = source.LumperFile;
        destination.PaperworkDate = source.PaperworkDate;
        destination.PaperworkFile = source.PaperworkFile;
        destination.ConfirmationFile = source.ConfirmationFile;
        destination.ConfirmationDate = source.ConfirmationDate;
        destination.EmailConfirmationSentDate = source.EmailConfirmationSentDate;
        destination.ConfirmedFile = source.ConfirmedFile;
        destination.ConfirmedDate = source.ConfirmedDate;
        if (String.IsNullOrEmpty(source.EmailConfirmationSent) == false) {
            if (DispatchJson.IsValidJson(source.EmailConfirmationSent)) {
                destination.EmailConfirmationSent = DispatchJson.Deserialize<List<string>>(source.EmailConfirmationSent);
            } else {
                destination.EmailConfirmationSent = new List<string>() { source.EmailConfirmationSent };
            }
        }

        destination.TrailerSchemeDate = source.TrailerSchemeDate;
        destination.TrailerSchemeFile = source.TrailerSchemeFile;
        destination.TrailerSchemeId = source.TrailerSchemeId;

        destination.InvoiceNumber = source.InvoiceNumber;
        destination.InvoiceFile = source.InvoiceFile;
        destination.InvoiceDate = source.InvoiceDate;
        destination.EmailInvoiceSentDate = source.EmailInvoiceSentDate;
        if (String.IsNullOrEmpty(source.EmailInvoiceSent) == false) {
            if (DispatchJson.IsValidJson(source.EmailInvoiceSent)) {
                destination.EmailInvoiceSent = DispatchJson.Deserialize<List<string>>(source.EmailInvoiceSent);
            } else {
                destination.EmailInvoiceSent = new List<string>() { source.EmailInvoiceSent };
            }
        }

        destination.CheckNumber = source.CheckNumber;
        destination.CheckFile = source.CheckFile;
        destination.CheckDate = source.CheckDate;
        destination.Payout = source.Payout;

        destination.NewScheduledAppointment = source.NewScheduledAppointment;

        destination.PhotoFile = source.PhotoFile;

        // companies
        destination.DatBrokerId = source.DatBrokerId;
        destination.CustomerId = source.CustomerId;
        if (source.DatBroker != null) {
            destination.McNumber = source.DatBroker.McNumber;
            destination.DatBrokerCompany = source.DatBroker.Name;
            destination.Contacts = source.DatBroker.Contacts;
        }
        if (source.Customer != null) {
            destination.CustomerCompany = source.Customer.Name;
            destination.Contacts = source.Customer.Contacts;
        }

        destination.DeliveredDate = source.DeliveredDate;

        return destination;
    }
}

/// <summary>
///     Load.Layout: [TrailerLayoutType, Index, Level] троек (V1 LoadLayout_ModelToDto / TrailerLayout_DtoToModel) </summary>
public static class LoadLayoutConverter {

    public static List<LoadLayoutDto> ToDto(byte[] source) {
        if (source == null) return null;
        var result = new List<LoadLayoutDto>();
        int i = 0;
        while (i < source.Length) {
            switch ((TrailerLayoutType)source[i]) {
                case TrailerLayoutType.IndexLevel:
                    result.Add(new LoadLayoutDto { Index = source[i + 1], Level = source[i + 2] });
                    i += 3;
                    break;
                default:
                    throw new Exception("Undefined TrailerLayoutType: " + (TrailerLayoutType)source[i]);
            }
        }
        return result;
    }

    public static byte[] ToModel(List<LoadLayoutDto> source) {
        if (source == null) return null;
        var result = new List<byte>();
        foreach (var layout in source) {
            result.Add((byte)TrailerLayoutType.IndexLevel);
            result.Add(layout.Index);
            result.Add(layout.Level);
        }
        return result.ToArray();
    }
}
