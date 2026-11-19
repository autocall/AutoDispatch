using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace AutoDispatch.Logic.Converters {
    /// <summary> V1 Load/DtoToVm, Load/VmToDto (тела ConvertCore как есть) </summary>
    public static class LoadConverter {

        public static LoadViewModel ToVm(LoadDto source) {
            if (source == null) return null;

            var destination = new LoadViewModel();
            BaseConverter.FillDispatchVm(source, destination);
            destination.Index = source.Index;
            destination.IndexExtra = source.IndexExtra;
            destination.IndexPrefix = source.IndexPrefix;
            destination.RecordId = source.RecordId;
            destination.BOLNumber = source.BOLNumber;

            destination.TrackerId = source.TrackerId;
            destination.CarrierViewPhone = source.CarrierViewPhone;
            destination.CarrierViewLink = source.CarrierViewLink;
            destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
            destination.CarrierViewStatus = source.CarrierViewStatus;
            destination.CarrierViewLate = source.CarrierViewLate;
            destination.CarrierViewPickupId = source.CarrierViewPickupId;
            destination.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
            destination.CarrierViewPickupComment = source.CarrierViewPickupComment;
            destination.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
            if (source.CarrierViewStatusesJson != null) {
                destination.CarrierViewStatuses = JsonExtension.DeserializeObject<List<string>>(source.CarrierViewStatusesJson, TypeNameHandling.None);
            }

            destination.ActualCompanyId = source.ActualCompanyId;
            destination.OldOrderId = source.OldOrderId;
            destination.TrailerId = source.TrailerId;
            destination.DestinationWarehouseId = source.DestinationWarehouseId;
            destination.WarehouseId = source.WarehouseId;
            destination.DatCompanyId = source.DatCompanyId;
            destination.DatDriverId = source.DatDriverId;
            destination.HistoryDriverId = source.HistoryDriverId;
            destination.HistoryTruckId = source.HistoryTruckId;
            destination.HistoryTrailerId = source.HistoryTrailerId;
            destination.HistoryAssignedTrailerId = source.HistoryAssignedTrailerId;
            destination.HistoryWarehouseId = source.HistoryWarehouseId;
            destination.HistoryDatCompanyId = source.HistoryDatCompanyId;
            destination.HistoryDatDriverId = source.HistoryDatDriverId;
            destination.OriginCity = source.OriginCity;
            destination.DestinationCity = source.DestinationCity;
            destination.OriginAddress = source.OriginAddress;
            destination.DestinationAddress = source.DestinationAddress;
            destination.OriginLocation = LocationConverter.ToVm(source.OriginLocation);
            destination.DestinationLocation = LocationConverter.ToVm(source.DestinationLocation);
            destination.CurrentOriginUtcOffset = source.CurrentOriginUtcOffset;
            destination.CurrentDestinationUtcOffset = source.CurrentDestinationUtcOffset;
            destination.Distance = source.Distance;
            destination.ReceiverId = source.ReceiverId;
            destination.ShipperId = source.ShipperId;
            destination.ShowPickupAddress = source.ShowPickupAddress;
            destination.ReceiverCompany = source.ReceiverCompany;
            destination.ShipperCompany = source.ShipperCompany;
            destination.PickupNumber = source.PickupNumber;
            destination.DeliveryNumber = source.DeliveryNumber;
            destination.LoadType = (LoadType)source.LoadType;
            destination.Equipment = (AutoCallEquipmentType)source.Equipment;
            destination.PalletType = (PalletType)source.PalletType;
            destination.Weight = source.Weight;
            destination.Length = source.Length;
            destination.Pallets = source.Pallets;
            destination.Spots = source.Spots;
            destination.WeightType = (WeightType)source.WeightType;
            destination.Commodity = source.Commodity;
            destination.DivideType = source.DivideType;
            destination.ContainerType = source.ContainerType;
            destination.ShipmentId = source.ShipmentId;
            destination.HistoryShipmentId = source.HistoryShipmentId;
            destination.ParentId = source.ParentId;
            destination.TrailerType = source.TrailerType;
            destination.Temperature = source.Temperature;
            destination.CarrierViewTracking = source.CarrierViewTracking;
            destination.LineHaul = source.LineHaul;
            destination.DatRate = source.DatRate;
            destination.Expenses = ExpensesConverter.ToVm(source.Expenses);
            destination.Gains = GainsConverter.ToVm(source.Gains);
            destination.Currency = (CurrencyType)source.Currency;
            destination.QuikPay = source.QuikPay;
            destination.BookedDate = BaseConverter.ConvertToLocal(source.BookedDate);
            destination.BoardChanged = BaseConverter.ConvertToLocal(source.BoardChanged);
            destination.PickupDate = source.PickupFrom;
            destination.PickupFrom = source.PickupFrom.TimeOfDay;
            destination.PickupTo = source.PickupTo.TimeOfDay;
            destination.DeliveryDate = source.DeliveryFrom;
            destination.DeliveryFrom = source.DeliveryFrom.TimeOfDay;
            destination.DeliveryTo = source.DeliveryTo.TimeOfDay;
            destination.Comments = source.Comments;
            destination.Comments2 = source.Comments2;
            destination.Consolidation = source.Consolidation;
            destination.Foreground = source.Foreground != null ? BinaryToColor(source.Foreground) : LoadViewModel.DefaultForeground;
            destination.Background = source.Background != null ? BinaryToColor(source.Background) : LoadViewModel.DefaultBackground;
            destination.BindDate = BaseConverter.ConvertToLocal(source.BindDate);
            destination.AssignedTrailerDate = BaseConverter.ConvertToLocal(source.AssignedTrailerDate);
            destination.Status = source.Status;
            destination.CustomStatusId = source.CustomStatusId;
            destination.LastStatusChangedDate = BaseConverter.ConvertToLocal(source.LastStatusChangedDate);
            destination.LastRateChanged = source.LastRateChanged;
            destination.BoardType = source.BoardType;

            destination.WasAtPickup = source.WasAtPickup;
            destination.WasAtDelivery = source.WasAtDelivery;

            destination.BOLDate = BaseConverter.ConvertToLocal(source.BOLDate);
            destination.BOLFile = source.BOLFile;
            destination.PODDate = BaseConverter.ConvertToLocal(source.PODDate);
            destination.PODFile = source.PODFile;
            destination.LumperDate = source.LumperDate;
            destination.LumperFile = source.LumperFile;
            destination.PaperworkDate = source.PaperworkDate;
            destination.PaperworkFile = source.PaperworkFile;
            destination.ConfirmationDate = BaseConverter.ConvertToLocal(source.ConfirmationDate);
            destination.ConfirmationFile = source.ConfirmationFile;
            destination.ConfirmedDate = BaseConverter.ConvertToLocal(source.ConfirmedDate);
            destination.ConfirmedFile = source.ConfirmedFile;
            destination.EmailConfirmationSent = source.EmailConfirmationSent;
            destination.EmailConfirmationSentDate = BaseConverter.ConvertToLocal(source.EmailConfirmationSentDate);

            destination.TrailerSchemeDate = source.TrailerSchemeDate;
            destination.TrailerSchemeFile = source.TrailerSchemeFile;
            destination.TrailerSchemeId = source.TrailerSchemeId;

            destination.InvoiceDate = BaseConverter.ConvertToLocal(source.InvoiceDate);
            destination.InvoiceFile = source.InvoiceFile;
            destination.InvoiceNumber = source.InvoiceNumber;
            destination.EmailInvoiceSentDate = BaseConverter.ConvertToLocal(source.EmailInvoiceSentDate);
            destination.EmailInvoiceSent = source.EmailInvoiceSent;

            destination.CheckNumber = source.CheckNumber;
            destination.CheckFile = source.CheckFile;
            destination.CheckDate = BaseConverter.ConvertToLocal(source.CheckDate);
            destination.Payout = source.Payout;

            destination.NewScheduledAppointment = source.NewScheduledAppointment;

            destination.PhotoFile = source.PhotoFile;

            destination.PickupOrder = source.PickupOrder;
            destination.DeliveryOrder = source.DeliveryOrder;

            destination.ShipperContacts = ContactsConverter.ToVm(source.ShipperContacts);
            destination.ReceiverContacts = ContactsConverter.ToVm(source.ReceiverContacts);

            destination.DeliveredDate = source.DeliveredDate;

            #region [ Company ]

            destination.McNumber = source.McNumber;
            destination.DatBrokerId = source.DatBrokerId;
            destination.CustomerId = source.CustomerId;
            if (source.BoardType == DispatchBoardType.Carrier || source.BoardType == DispatchBoardType.CarrierAccount) {
                destination.Company = source.DatBrokerCompany?.ToUpper();
            }
            if (source.BoardType == DispatchBoardType.Broker || source.BoardType == DispatchBoardType.BrokerAccount) {
                destination.Company = source.CustomerCompany?.ToUpper();
            }
            destination.Contacts = ContactsConverter.ToVm(source.Contacts);

            #endregion [ Company ]

            destination.Layout = new ObservableCollection<LoadPalletViewModel>();
            if (source.Layout != null) {
                foreach (var dto in source.Layout) {
                    destination.Layout.Add(new LoadPalletViewModel(destination) { Index = dto.Index, Level = dto.Level });
                }
            }

            return destination;
        }

        public static LoadDto ToDto(LoadViewModel source) {
            if (source == null) return null;

            var destination = new LoadDto();
            BaseConverter.FillDispatchDto(source, destination);
            destination.Index = source.Index;
            destination.IndexExtra = source.IndexExtra;
            destination.IndexPrefix = source.IndexPrefix;
            destination.RecordId = source.RecordId;
            destination.BOLNumber = source.BOLNumber;

            destination.TrackerId = source.TrackerId;
            destination.CarrierViewPhone = source.CarrierViewPhone;
            destination.CarrierViewLink = source.CarrierViewLink;
            destination.CarrierViewLastUpdatedDate = source.CarrierViewLastUpdatedDate;
            destination.CarrierViewStatus = source.CarrierViewStatus;
            destination.CarrierViewLate = source.CarrierViewLate;
            destination.CarrierViewPickupId = source.CarrierViewPickupId;
            destination.CarrierViewDeliveryId = source.CarrierViewDeliveryId;
            destination.CarrierViewPickupComment = source.CarrierViewPickupComment;
            destination.CarrierViewDeliveryComment = source.CarrierViewDeliveryComment;
            destination.CarrierViewStatusesJson = JsonExtension.SerializeObject<List<string>>(source.CarrierViewStatuses);

            destination.ActualCompanyId = source.ActualCompanyId;
            destination.OldOrderId = source.OldOrderId;
            destination.TrailerId = source.TrailerId;
            destination.DatCompanyId = source.DatCompanyId;
            destination.DestinationWarehouseId = source.DestinationWarehouseId;
            destination.WarehouseId = source.WarehouseId;
            destination.DatDriverId = source.DatDriverId;
            destination.HistoryDriverId = source.HistoryDriverId;
            destination.HistoryTruckId = source.HistoryTruckId;
            destination.HistoryTrailerId = source.HistoryTrailerId;
            destination.HistoryAssignedTrailerId = source.HistoryAssignedTrailerId;
            destination.HistoryWarehouseId = source.HistoryWarehouseId;
            destination.HistoryDatCompanyId = source.HistoryDatCompanyId;
            destination.HistoryDatDriverId = source.HistoryDatDriverId;
            destination.OriginCity = StringExtension.NormalizeCityName(source.OriginCity);
            destination.DestinationCity = StringExtension.NormalizeCityName(source.DestinationCity);
            destination.CurrentOriginUtcOffset = source.CurrentOriginUtcOffset;
            destination.CurrentDestinationUtcOffset = source.CurrentDestinationUtcOffset;
            destination.OriginAddress = StringExtension.FirstCharToUpper(source.OriginAddress);
            destination.DestinationAddress = StringExtension.FirstCharToUpper(source.DestinationAddress);
            destination.OriginLocation = LocationConverter.ToDto(source.OriginLocation);
            destination.DestinationLocation = LocationConverter.ToDto(source.DestinationLocation);
            destination.Distance = source.Distance;
            if (destination.CurrentLocation == null) {
                destination.CurrentLocation = destination.OriginLocation;
            }
            destination.ReceiverId = source.ReceiverId;
            destination.ShipperId = source.ShipperId;
            destination.ReceiverCompany = source.ReceiverCompany?.ToUpper();
            destination.ShipperCompany = source.ShipperCompany?.ToUpper();
            destination.PickupNumber = source.PickupNumber;
            destination.DeliveryNumber = source.DeliveryNumber;
            destination.ShowPickupAddress = source.ShowPickupAddress;
            destination.LoadType = (byte)source.LoadType;
            destination.Equipment = (byte)source.Equipment;
            destination.PalletType = (byte)source.PalletType; 
            destination.Weight = source.Weight;
            destination.Length = source.Length;
            destination.Pallets = source.Pallets;
            destination.Spots = source.Spots;
            destination.WeightType = (byte)source.WeightType;
            destination.Commodity = source.Commodity;
            destination.DivideType = source.DivideType;
            destination.ContainerType = source.ContainerType;
            destination.ParentId = source.ParentId;
            destination.ShipmentId = source.ShipmentId;
            destination.HistoryShipmentId = source.HistoryShipmentId;
            destination.TrailerType = source.TrailerType;
            destination.Temperature = source.Temperature;
            destination.CarrierViewTracking = source.CarrierViewTracking;
            destination.LineHaul = source.LineHaul;
            destination.DatRate = source.DatRate;
            destination.Expenses = ExpensesConverter.ToDto(source.Expenses);
            destination.Gains = GainsConverter.ToDto(source.Gains);
            destination.Currency = (byte)source.Currency;
            destination.QuikPay = source.QuikPay;
            destination.BookedDate = BaseConverter.ConvertToUtc(source.BookedDate);
            destination.BoardChanged = BaseConverter.ConvertToUtc(source.BoardChanged);
            destination.PickupFrom = new DateTime(source.PickupDate.Year, source.PickupDate.Month, source.PickupDate.Day, source.PickupFrom.Hours, source.PickupFrom.Minutes, source.PickupFrom.Seconds);
            destination.PickupTo = new DateTime(source.PickupDate.Year, source.PickupDate.Month, source.PickupDate.Day, source.PickupTo.Hours, source.PickupTo.Minutes, source.PickupTo.Seconds);
            destination.DeliveryFrom = new DateTime(source.DeliveryDate.Year, source.DeliveryDate.Month, source.DeliveryDate.Day, source.DeliveryFrom.Hours, source.DeliveryFrom.Minutes, source.DeliveryFrom.Seconds);
            destination.DeliveryTo = new DateTime(source.DeliveryDate.Year, source.DeliveryDate.Month, source.DeliveryDate.Day, source.DeliveryTo.Hours, source.DeliveryTo.Minutes, source.DeliveryTo.Seconds);
            destination.Comments = source.Comments;
            destination.Comments2 = source.Comments2;
            destination.Consolidation = source.Consolidation;
            destination.Background = ColorToBinary(source.Background);
            destination.Foreground = ColorToBinary(source.Foreground);
            destination.BindDate = BaseConverter.ConvertToUtc(source.BindDate);
            destination.AssignedTrailerDate = BaseConverter.ConvertToUtc(source.AssignedTrailerDate);
            destination.Status = source.Status;
            destination.CustomStatusId = source.CustomStatusId;
            destination.LastStatusChangedDate = BaseConverter.ConvertToUtc(source.LastStatusChangedDate);
            destination.LastRateChanged = source.LastRateChanged;
            destination.BoardType = source.BoardType;

            destination.BOLDate = BaseConverter.ConvertToUtc(source.BOLDate);
            destination.BOLFile = source.BOLFile;
            destination.PODDate = BaseConverter.ConvertToUtc(source.PODDate);
            destination.PODFile = source.PODFile;
            destination.LumperDate = source.LumperDate;
            destination.LumperFile = source.LumperFile;
            destination.PaperworkDate = source.PaperworkDate;
            destination.PaperworkFile = source.PaperworkFile;
            destination.ConfirmationDate = BaseConverter.ConvertToUtc(source.ConfirmationDate);
            destination.ConfirmationFile = source.ConfirmationFile;
            destination.ConfirmedDate = BaseConverter.ConvertToUtc(source.ConfirmedDate);
            destination.ConfirmedFile = source.ConfirmedFile;
            destination.EmailConfirmationSent = source.EmailConfirmationSent;
            destination.EmailConfirmationSentDate = BaseConverter.ConvertToUtc(source.EmailConfirmationSentDate);

            destination.TrailerSchemeDate = source.TrailerSchemeDate;
            destination.TrailerSchemeFile = source.TrailerSchemeFile;
            destination.TrailerSchemeId = source.TrailerSchemeId;

            destination.InvoiceNumber = source.InvoiceNumber;
            destination.InvoiceFile = source.InvoiceFile;
            destination.InvoiceDate = BaseConverter.ConvertToUtc(source.InvoiceDate);
            destination.EmailInvoiceSentDate = BaseConverter.ConvertToUtc(source.EmailInvoiceSentDate);
            destination.EmailInvoiceSent = source.EmailInvoiceSent;

            destination.CheckNumber = source.CheckNumber;
            destination.CheckFile = source.CheckFile;
            destination.CheckDate = BaseConverter.ConvertToUtc(source.CheckDate);
            destination.Payout = source.Payout;

            destination.NewScheduledAppointment = source.NewScheduledAppointment;
         
            destination.PhotoFile = source.PhotoFile;

            destination.PickupOrder = source.PickupOrder;
            destination.DeliveryOrder = source.DeliveryOrder;

            destination.ShipperContacts = ContactsConverter.ToDto(source.ShipperContacts);
            destination.ReceiverContacts = ContactsConverter.ToDto(source.ReceiverContacts);

            destination.DeliveredDate = source.DeliveredDate;
          
            destination.WasAtPickup = source.WasAtPickup;
            destination.WasAtDelivery = source.WasAtDelivery;

            destination.IndexCorrected = source.IndexCorrected;

            #region [ Company ]

            if (source.BoardType == DispatchBoardType.Carrier || source.BoardType == DispatchBoardType.CarrierAccount) {
                destination.McNumber = source.McNumber;
                destination.DatBrokerId = source.DatBrokerId;
                destination.DatBrokerCompany = source.Company;
            }
            if (source.BoardType == DispatchBoardType.Broker || source.BoardType == DispatchBoardType.BrokerAccount) {
                destination.CustomerId = source.CustomerId;
                destination.CustomerCompany = source.Company;
            }
            destination.Contacts = ContactsConverter.ToDto(source.Contacts);

            #endregion [ Company ]

            destination.Layout = new List<LoadLayoutDto>();
            foreach (var dto in source.Layout) {
                destination.Layout.Add(new LoadLayoutDto() { Index = dto.Index, Level = dto.Level });
            }

            return destination;
        }

        private static Color BinaryToColor(byte[] binary) {
            return Color.FromArgb(binary[0], binary[1], binary[2], binary[3]);
        }

        private static byte[] ColorToBinary(Color color) {
            return new[] { color.A, color.R, color.G, color.B };
        }
    }
}
