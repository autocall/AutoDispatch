using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

// Driver, Truck, Trailer, DatCompany, DatDriver, Warehouse, Yard — V1 Converters/Dispatch/* без изменений логики,
// geography → decimal-пары через LocationConverter

#region [ Driver ]

public static class DriverConverter {

    public static Driver ToModel(DriverDto source) {
        if (source == null) return null;
        var destination = new Driver();
        BaseConverter.FillObjectModel(source, destination);
        destination.TruckId = source.TruckId;
        destination.Name = source.Name;
        destination.Email = source.Email;
        destination.Phone = source.Phone;
        destination.CurrentLoadId = source.LoadId;
        destination.DriverStatus = (byte)source.DriverStatus;
        LocationConverter.SetCurrent(destination, source.CurrentLocation);
        destination.CarrierViewId = source.CarrierViewId;
        destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;

        destination.THE_APPLICATION_OF_EMPLOYMENT_File = source.THE_APPLICATION_OF_EMPLOYMENT_File;
        destination.MVR_File = source.MVR_File;
        destination.MEDICAL_CARD_File = source.MEDICAL_CARD_File;
        destination.CDL_File = source.CDL_File;
        destination.DOT_INSPECTIONS_File = source.DOT_INSPECTIONS_File;
        destination.BACKGROUND_File = source.BACKGROUND_File;
        destination.DRUG_TEST_File = source.DRUG_TEST_File;
        destination.RANDOM_DRUG_TEST_File = source.RANDOM_DRUG_TEST_File;
        destination.ACH_File = source.ACH_File;
        destination.W9_File = source.W9_File;

        destination.MVR_Expiration = source.MVR_Expiration;
        destination.MEDICAL_CARD_Expiration = source.MEDICAL_CARD_Expiration;
        destination.CDL_Expiration = source.CDL_Expiration;
        destination.ACH_Expiration = source.ACH_Expiration;
        destination.W9_Expiration = source.W9_Expiration;
        return destination;
    }

    public static DriverDto ToDto(Driver source) {
        if (source == null) return null;
        var destination = new DriverDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.TruckId = source.TruckId;
        destination.Name = source.Name;
        destination.Email = source.Email;
        destination.Phone = source.Phone;
        destination.LoadId = source.CurrentLoadId;
        destination.DriverStatus = (DriverStatus)source.DriverStatus;
        destination.CurrentLocation = LocationConverter.ToDto(source.CurrentLatitude, source.CurrentLongitude);
        destination.CarrierViewId = source.CarrierViewId;
        destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;

        destination.THE_APPLICATION_OF_EMPLOYMENT_File = source.THE_APPLICATION_OF_EMPLOYMENT_File;
        destination.MVR_File = source.MVR_File;
        destination.MEDICAL_CARD_File = source.MEDICAL_CARD_File;
        destination.CDL_File = source.CDL_File;
        destination.DOT_INSPECTIONS_File = source.DOT_INSPECTIONS_File;
        destination.BACKGROUND_File = source.BACKGROUND_File;
        destination.DRUG_TEST_File = source.DRUG_TEST_File;
        destination.RANDOM_DRUG_TEST_File = source.RANDOM_DRUG_TEST_File;
        destination.ACH_File = source.ACH_File;
        destination.W9_File = source.W9_File;

        destination.MVR_Expiration = source.MVR_Expiration;
        destination.MEDICAL_CARD_Expiration = source.MEDICAL_CARD_Expiration;
        destination.CDL_Expiration = source.CDL_Expiration;
        destination.ACH_Expiration = source.ACH_Expiration;
        destination.W9_Expiration = source.W9_Expiration;
        return destination;
    }
}

#endregion [ Driver ]

#region [ Truck ]

public static class TruckConverter {

    public static Truck ToModel(TruckDto source) {
        if (source == null) return null;
        var destination = new Truck();
        BaseConverter.FillObjectModel(source, destination);
        destination.TrailerId = source.TrailerId;
        destination.Number = source.Number;
        LocationConverter.SetCurrent(destination, source.CurrentLocation);
        destination.AnnualInspection_File = source.AnnualInspection_File;
        destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
        destination.Registration_File = source.Registration_File;
        destination.Registration_Expiration = source.Registration_Expiration;
        return destination;
    }

    public static TruckDto ToDto(Truck source) {
        if (source == null) return null;
        var destination = new TruckDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.TrailerId = source.TrailerId;
        destination.Number = source.Number;
        destination.CurrentLocation = LocationConverter.ToDto(source.CurrentLatitude, source.CurrentLongitude);
        destination.AnnualInspection_File = source.AnnualInspection_File;
        destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
        destination.Registration_File = source.Registration_File;
        destination.Registration_Expiration = source.Registration_Expiration;
        return destination;
    }
}

#endregion [ Truck ]

#region [ Trailer ]

public static class TrailerConverter {

    public static Trailer ToModel(TrailerDto source) {
        if (source == null) return null;
        var destination = new Trailer();
        BaseConverter.FillObjectModel(source, destination);
        destination.Number = source.Number;
        destination.Type = source.Type;
        LocationConverter.SetCurrent(destination, source.CurrentLocation);

        destination.AnnualInspection_File = source.AnnualInspection_File;
        destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
        destination.Registration_File = source.Registration_File;
        destination.Maintenance_File = source.Maintenance_File;
        destination.Maintenance_Expiration = source.Maintenance_Expiration;
        destination.SchemeDate = source.SchemeDate;
        destination.SchemeFile = source.SchemeFile;
        destination.OriginSchemeFile = source.OriginSchemeFile;

        destination.BnsfOwner = source.BnsfOwner;
        destination.BnsfShipper = source.BnsfShipper;
        destination.BnsfUnit = source.BnsfUnit;
        destination.BnsfLoadEmptyStatus = source.BnsfLoadEmptyStatus;
        destination.BnsfCurrentLocation = source.BnsfCurrentLocation;
        destination.BnsfEventCode = source.BnsfEventCode;
        destination.BnsfEventDate = source.BnsfEventDate;
        destination.BnsfETA_Deramp = source.BnsfETA_Deramp;
        destination.BnsfOrigin = source.BnsfOrigin;
        destination.BnsfDestination = source.BnsfDestination;
        destination.BnsfBeyondGoalHours = source.BnsfBeyondGoalHours;
        destination.BnsfTimeDeramp = source.BnsfTimeDeramp;
        destination.BnsfTimeNotify = source.BnsfTimeNotify;
        destination.BnsfFinalTrainArrival = source.BnsfFinalTrainArrival;
        destination.BnsfGoalDate = source.BnsfGoalDate;
        destination.BnsfNextevent = source.BnsfNextevent;
        destination.BnsfPickUpNumber = source.BnsfPickUpNumber;
        destination.BnsfStorageDay = source.BnsfStorageDay;
        destination.BnsfTrain = source.BnsfTrain;
        destination.BnsfTrucker = source.BnsfTrucker;
        destination.BnsfWaybillDate = source.BnsfWaybillDate;
        destination.BnsfRateAuthority = source.BnsfRateAuthority;
        destination.BnsfAmountDue = source.BnsfAmountDue;
        destination.BnsfDueDate = source.BnsfDueDate;
        destination.BnsfWaybillNumber = source.BnsfWaybillNumber;
        destination.BnsfBillNumber = source.BnsfBillNumber;
        destination.BnsfAdjustments = source.BnsfAdjustments;

        destination.History = source.History;
        destination.YardId = source.YardId;
        return destination;
    }

    public static TrailerDto ToDto(Trailer source) {
        if (source == null) return null;
        var destination = new TrailerDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.Number = source.Number;
        destination.Type = source.Type;
        destination.CurrentLocation = LocationConverter.ToDto(source.CurrentLatitude, source.CurrentLongitude);

        destination.AnnualInspection_File = source.AnnualInspection_File;
        destination.AnnualInspection_Expiration = source.AnnualInspection_Expiration;
        destination.Registration_File = source.Registration_File;
        destination.Maintenance_File = source.Maintenance_File;
        destination.Maintenance_Expiration = source.Maintenance_Expiration;
        destination.SchemeDate = source.SchemeDate;
        destination.SchemeFile = source.SchemeFile;
        destination.OriginSchemeFile = source.OriginSchemeFile;

        destination.BnsfOwner = source.BnsfOwner;
        destination.BnsfShipper = source.BnsfShipper;
        destination.BnsfUnit = source.BnsfUnit;
        destination.BnsfLoadEmptyStatus = source.BnsfLoadEmptyStatus;
        destination.BnsfCurrentLocation = source.BnsfCurrentLocation;
        destination.BnsfEventCode = source.BnsfEventCode;
        destination.BnsfEventDate = source.BnsfEventDate;
        destination.BnsfETA_Deramp = source.BnsfETA_Deramp;
        destination.BnsfOrigin = source.BnsfOrigin;
        destination.BnsfDestination = source.BnsfDestination;
        destination.BnsfBeyondGoalHours = source.BnsfBeyondGoalHours;
        destination.BnsfTimeDeramp = source.BnsfTimeDeramp;
        destination.BnsfTimeNotify = source.BnsfTimeNotify;
        destination.BnsfFinalTrainArrival = source.BnsfFinalTrainArrival;
        destination.BnsfGoalDate = source.BnsfGoalDate;
        destination.BnsfNextevent = source.BnsfNextevent;
        destination.BnsfPickUpNumber = source.BnsfPickUpNumber;
        destination.BnsfStorageDay = source.BnsfStorageDay;
        destination.BnsfTrain = source.BnsfTrain;
        destination.BnsfTrucker = source.BnsfTrucker;
        destination.BnsfWaybillDate = source.BnsfWaybillDate;
        destination.BnsfRateAuthority = source.BnsfRateAuthority;
        destination.BnsfAmountDue = source.BnsfAmountDue;
        destination.BnsfDueDate = source.BnsfDueDate;
        destination.BnsfWaybillNumber = source.BnsfWaybillNumber;
        destination.BnsfBillNumber = source.BnsfBillNumber;
        destination.BnsfAdjustments = source.BnsfAdjustments;

        destination.History = source.History;
        destination.YardId = source.YardId;
        return destination;
    }
}

#endregion [ Trailer ]

#region [ DatCompany ]

public static class DatCompanyConverter {

    public static DatCompany ToModel(DatCompanyDto source) {
        if (source == null) return null;
        var destination = new DatCompany();
        BaseConverter.FillObjectModel(source, destination);
        destination.DispatchCompanyId = source.ActualCompanyId;
        destination.Name = source.Name;
        destination.Phy_UpsStore = source.Phy_UpsStore;
        destination.Mai_UpsStore = source.Mai_UpsStore;
        destination.Phy_MailBox = source.Phy_MailBox;
        destination.Mai_MailBox = source.Mai_MailBox;
        destination.Comments = source.Comments;
        destination.Rating = source.Rating;
        destination.HyperlinkFile = source.HyperlinkFile;
        destination.EntityType = (byte)source.EntityType;
        destination.InsuranceExpiration = source.InsuranceExpiration;
        destination.InsuranceFile = source.InsuranceFilePath;
        destination.Contacts = source.Contacts;
        destination.McNumber = source.McNumber;
        destination.DotNumber = source.DotNumber;

        destination.SetupPacketId = source.SetupPacketId;
        if (source.SetupPacketEmailSent != null) {
            destination.SetupPacketEmailSent = DispatchJson.Serialize(source.SetupPacketEmailSent);
        }
        destination.SetupPacketSent = source.SetupPacketSent;
        destination.SetupPacketCompleted = source.SetupPacketCompleted;
        destination.SetupPacketAccept = source.SetupPacketAccept;
        destination.SetupPacketFile = source.SetupPacketFile;
        destination.W9File = source.W9File;
        destination.DoNotUse = source.DoNotUse;
        return destination;
    }

    public static DatCompanyDto ToDto(DatCompany source) {
        if (source == null) return null;
        var destination = new DatCompanyDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.ActualCompanyId = source.DispatchCompanyId;
        destination.Name = source.Name;
        destination.Phy_UpsStore = source.Phy_UpsStore;
        destination.Mai_UpsStore = source.Mai_UpsStore;
        destination.Phy_MailBox = source.Phy_MailBox;
        destination.Mai_MailBox = source.Mai_MailBox;
        destination.Comments = source.Comments;
        destination.Rating = source.Rating;
        destination.HyperlinkFile = source.HyperlinkFile;
        destination.EntityType = (DatCompanyType)source.EntityType;
        destination.InsuranceExpiration = source.InsuranceExpiration;
        destination.InsuranceFilePath = source.InsuranceFile;
        destination.Contacts = source.Contacts;
        destination.McNumber = source.McNumber;
        destination.DotNumber = source.DotNumber;

        destination.SetupPacketId = source.SetupPacketId;
        if (String.IsNullOrEmpty(source.SetupPacketEmailSent) == false) {
            destination.SetupPacketEmailSent = DispatchJson.Deserialize<List<string>>(source.SetupPacketEmailSent);
        }
        destination.SetupPacketSent = source.SetupPacketSent;
        destination.SetupPacketCompleted = source.SetupPacketCompleted;
        destination.SetupPacketAccept = source.SetupPacketAccept;
        destination.SetupPacketFile = source.SetupPacketFile;
        destination.W9File = source.W9File;
        destination.DoNotUse = source.DoNotUse;
        return destination;
    }
}

#endregion [ DatCompany ]

#region [ DatDriver ]

public static class DatDriverConverter {

    public static DatDriver ToModel(DatDriverDto source) {
        if (source == null) return null;
        var destination = new DatDriver();
        BaseConverter.FillObjectModel(source, destination);
        LocationConverter.SetCurrent(destination, source.CurrentLocation);
        destination.Name = source.Name;
        destination.Comments = source.Comments;
        destination.TruckNumber = source.TruckNumber;
        destination.TrailerNumber = source.TrailerNumber;
        destination.ContactPhone = source.ContactPhone;
        destination.CarrierViewStatus = source.CarrierViewStatus;
        destination.CarrierViewStatusesJson = source.CarrierViewStatusesJson;
        destination.CarrierViewLate = source.CarrierViewLate;
        destination.CarrierViewId = source.CarrierViewId;
        destination.CarrierViewLoadId = source.CarrierViewLoadId;
        destination.CarrierViewLink = source.CarrierViewLink;
        destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
        destination.CarrierViewPickupComment = source.CarrierViewPickupComment;
        destination.CarrierViewPickupId = source.CarrierViewPickupId;
        destination.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
        destination.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
        return destination;
    }

    public static DatDriverDto ToDto(DatDriver source) {
        if (source == null) return null;
        var destination = new DatDriverDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.Name = source.Name;
        destination.Comments = source.Comments;
        destination.TruckNumber = source.TruckNumber;
        destination.TrailerNumber = source.TrailerNumber;
        destination.ContactPhone = source.ContactPhone;
        destination.CarrierViewStatus = source.CarrierViewStatus;
        destination.CarrierViewStatusesJson = source.CarrierViewStatusesJson;
        destination.CarrierViewLate = source.CarrierViewLate;
        destination.CarrierViewId = source.CarrierViewId;
        destination.CarrierViewLoadId = source.CarrierViewLoadId;
        destination.CarrierViewLink = source.CarrierViewLink;
        destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
        destination.CarrierViewPickupComment = source.CarrierViewPickupComment;
        destination.CarrierViewPickupId = source.CarrierViewPickupId;
        destination.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
        destination.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
        destination.CurrentLocation = LocationConverter.ToDto(source.CurrentLatitude, source.CurrentLongitude);
        return destination;
    }
}

#endregion [ DatDriver ]

#region [ Warehouse, Yard ]

public static class WarehouseConverter {

    public static Warehouse ToModel(WarehouseDto source) {
        if (source == null) return null;
        var destination = new Warehouse();
        BaseConverter.FillObjectModel(source, destination);
        destination.Name = source.Name;
        destination.City = source.City;
        destination.Address = source.Address;
        destination.Latitude = (decimal)source.Location.Latitude;
        destination.Longitude = (decimal)source.Location.Longitude;
        return destination;
    }

    public static WarehouseDto ToDto(Warehouse source) {
        if (source == null) return null;
        var destination = new WarehouseDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.Name = source.Name;
        destination.City = source.City;
        destination.Address = source.Address;
        destination.Location = LocationConverter.ToDto(source.Latitude, source.Longitude);
        return destination;
    }
}

public static class YardConverter {

    public static Yard ToModel(YardDto source) {
        if (source == null) return null;
        var destination = new Yard();
        BaseConverter.FillObjectModel(source, destination);
        destination.Name = source.Name;
        destination.City = source.City;
        destination.Address = source.Address;
        destination.Latitude = (decimal)source.Location.Latitude;
        destination.Longitude = (decimal)source.Location.Longitude;
        return destination;
    }

    public static YardDto ToDto(Yard source) {
        if (source == null) return null;
        var destination = new YardDto();
        BaseConverter.FillObjectDto(source, destination);
        destination.Name = source.Name;
        destination.City = source.City;
        destination.Address = source.Address;
        destination.Location = LocationConverter.ToDto(source.Latitude, source.Longitude);
        return destination;
    }
}

#endregion [ Warehouse, Yard ]

#region [ HistoryLocation* ]

public static class HistoryLocationLoadConverter {

    public static HistoryLocationLoadDto ToDto(HistoryLocationLoad source) {
        if (source == null) return null;
        return new HistoryLocationLoadDto {
            Id = source.Id,
            HistoryId = source.HistoryId,
            CreatedById = source.CreatedById,
            CreatedDate = source.CreatedDate,
            EventId = source.EventId,
            LoadId = source.LoadId,
            HistoryLocationTrailerId = source.HistoryLocationTrailerId,
            Location = LocationConverter.ToDto(source.Latitude, source.Longitude)
        };
    }
}

public static class HistoryLocationTruckConverter {

    public static HistoryLocationTruckDto ToDto(HistoryLocationTruck source) {
        if (source == null) return null;
        return new HistoryLocationTruckDto {
            Id = source.Id,
            HistoryId = source.HistoryId,
            CreatedById = source.CreatedById,
            CreatedDate = source.CreatedDate,
            EventId = source.EventId,
            TruckId = source.TruckId,
            Location = LocationConverter.ToDto(source.Latitude, source.Longitude)
        };
    }
}

public static class HistoryLocationTrailerConverter {

    public static HistoryLocationTrailerDto ToDto(HistoryLocationTrailer source) {
        if (source == null) return null;
        return new HistoryLocationTrailerDto {
            Id = source.Id,
            HistoryId = source.HistoryId,
            CreatedById = source.CreatedById,
            CreatedDate = source.CreatedDate,
            EventId = source.EventId,
            TrailerId = source.TrailerId,
            Location = LocationConverter.ToDto(source.Latitude, source.Longitude)
        };
    }
}

public static class HistoryLocationDriverConverter {

    public static HistoryLocationDriverDto ToDto(HistoryLocationDriver source) {
        if (source == null) return null;
        return new HistoryLocationDriverDto {
            Id = source.Id,
            HistoryId = source.HistoryId,
            CreatedById = source.CreatedById,
            CreatedDate = source.CreatedDate,
            EventId = source.EventId,
            DriverId = source.DriverId,
            HistoryLocationTruckId = source.HistoryLocationTruckId,
            Location = LocationConverter.ToDto(source.Latitude, source.Longitude)
        };
    }
}

public static class HistoryLocationDatDriverConverter {

    public static HistoryLocationDatDriverDto ToDto(HistoryLocationDatDriver source) {
        if (source == null) return null;
        return new HistoryLocationDatDriverDto {
            Id = source.Id,
            HistoryId = source.HistoryId,
            CreatedById = source.CreatedById,
            CreatedDate = source.CreatedDate,
            EventId = source.EventId,
            DatDriverId = source.DatDriverId,
            Location = LocationConverter.ToDto(source.Latitude, source.Longitude)
        };
    }
}

#endregion [ HistoryLocation* ]
