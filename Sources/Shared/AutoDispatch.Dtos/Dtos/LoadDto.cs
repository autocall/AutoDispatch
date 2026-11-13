using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class LoadDto : BaseDispatchDto, IBaseEventDto {
        [DataMember]
        public int? Index { get; set; }

        [DataMember]
        public string IndexExtra { get; set; }

        [DataMember]
        public string IndexPrefix { get; set; }

        public string IndexString => $"{IndexPrefix}{Index}{IndexExtra}";

        [DataMember]
        public Guid? ActualCompanyId { get; set; }

        [DataMember]
        public int? TrackerId { get; set; }
        [DataMember]
        public string CarrierViewPhone { get; set; }
        [DataMember]
        public string CarrierViewLink { get; set; }
        [DataMember]
        public DateTime? CarrierViewLastUpdatedDate { get; set; }
        [DataMember]
        public string CarrierViewStatus { get; set; }
        [DataMember]
        public string CarrierViewStatusesJson { get; set; }
        [DataMember]
        public bool CarrierViewLate { get; set; }
        [DataMember]
        public int? CarrierViewPickupId { get; set; }
        [DataMember]
        public int? CarrierViewDeliveryId { get; set; }
        [DataMember]
        public string CarrierViewPickupComment { get; set; }
        [DataMember]
        public string CarrierViewDeliveryComment { get; set; }

        [DataMember]
        public string RecordId { get; set; }
        [DataMember]
        public string BOLNumber { get; set; }

        [DataMember]
        public Guid? OldOrderId { get; set; }

        [DataMember]
        public string OriginCity { get; set; }

        [DataMember]
        public string DestinationCity { get; set; }

        [DataMember]
        public string OriginAddress { get; set; }

        [DataMember]
        public string DestinationAddress { get; set; }

        [DataMember]
        public LocationDto OriginLocation { get; set; }

        [DataMember]
        public LocationDto DestinationLocation { get; set; }

        /// <summary>
        ///     Daylight or Standart UTC Offset of Origin Location </summary>
        [DataMember]
        public int CurrentOriginUtcOffset { get; set; }
        /// <summary>
        ///     Daylight or Standart UTC Offset of Destination Location </summary>
        [DataMember]
        public int CurrentDestinationUtcOffset { get; set; }

        [DataMember]
        public int? Distance { get; set; }

        [DataMember]
        public byte Equipment { get; set; }

        [DataMember]
        public byte PalletType { get; set; }

        [DataMember]
        public int? Weight { get; set; }

        [DataMember]
        public int? Length { get; set; }

        [DataMember]
        public short? Pallets { get; set; }

        [DataMember]
        public short? Spots { get; set; }

        [DataMember]
        public byte WeightType { get; set; }

        [DataMember]
        public string Commodity { get; set; }

        [DataMember]
        public DateTime PickupFrom { get; set; }
        [DataMember]
        public DateTime PickupTo { get; set; }

        [DataMember]
        public DateTime DeliveryFrom { get; set; }
        [DataMember]
        public DateTime DeliveryTo { get; set; }

        [DataMember]
        public Guid? DestinationWarehouseId { get; set; }
        [DataMember]
        public Guid? WarehouseId { get; set; }
        [DataMember]
        public Guid? TrailerId { get; set; }
        [DataMember]
        public Guid? DatDriverId { get; set; }
        [DataMember]
        public Guid? DatCompanyId { get; set; }

        [DataMember]
        public Guid? HistoryWarehouseId { get; set; }
        [DataMember]
        public Guid? HistoryTrailerId { get; set; }
        [DataMember]
        public Guid? HistoryAssignedTrailerId { get; set; }
        [DataMember]
        public Guid? HistoryTruckId { get; set; }
        [DataMember]
        public Guid? HistoryDriverId { get; set; }
        [DataMember]
        public Guid? HistoryDatDriverId { get; set; }
        [DataMember]
        public Guid? HistoryDatCompanyId { get; set; }

        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public string Comments { get; set; }
        [DataMember]
        public string Comments2 { get; set; }
        [DataMember]
        public string Consolidation { get; set; }

        [DataMember]
        public Guid? ShipperId { get; set; }
        [DataMember]
        public Guid? ReceiverId { get; set; }
        [DataMember]
        public string ShipperCompany { get; set; }
        [DataMember]
        public string ReceiverCompany { get; set; }
        /// <summary>
        ///     JSON </summary>
        [DataMember]
        public string ShipperContacts { get; set; }
        /// <summary>
        ///     JSON </summary>
        [DataMember]
        public string ReceiverContacts { get; set; }

        [DataMember]
        public string PickupNumber { get; set; }
        [DataMember]
        public string DeliveryNumber { get; set; }

        [DataMember]
        public bool ShowPickupAddress { get; set; }

        [DataMember]
        public byte LoadType { get; set; }
        [DataMember]
        public decimal? LineHaul { get; set; }
        [DataMember]
        public decimal? CarrierViewTracking { get; set; }
        [DataMember]
        public decimal? DatRate { get; set; }
        [DataMember]
        public string Expenses { get; set; }
        [DataMember]
        public string Gains { get; set; }
        [DataMember]
        public DateTime BookedDate { get; set; }
        [DataMember]
        public DateTime BoardChanged { get; set; }
        [DataMember]
        public byte[] Foreground { get; set; }
        [DataMember]
        public byte[] Background { get; set; }
        [DataMember]
        public byte Currency { get; set; }
        [DataMember]
        public DateTime? QuikPay { get; set; }
        /// <summary>
        ///     For the trailer layout </summary>
        [DataMember]
        public DivideType DivideType { get; set; }
        [DataMember]
        public ContainerType ContainerType { get; set; }
        [DataMember]
        public Guid? ParentId { get; set; }
        [DataMember]
        public Guid? ShipmentId { get; set; }
        [DataMember]
        public Guid? HistoryShipmentId { get; set; }
        [DataMember]
        public TrailerType TrailerType { get; set; }
        [DataMember]
        public short? Temperature { get; set; }

        [DataMember]
        public DateTime? BindDate { get; set; }
        [DataMember]
        public DateTime? AssignedTrailerDate { get; set; }

        [DataMember]
        public List<LoadLayoutDto> Layout { get; set; }
        [DataMember]
        public LoadStatus Status { get; set; }
        [DataMember]
        public Guid? CustomStatusId { get; set; }
        [DataMember]
        public DateTime LastStatusChangedDate { get; set; }
        [DataMember]
        public SByte? LastRateChanged { get; set; }

        [DataMember]
        public byte? PickupOrder { get; set; }
        [DataMember]
        public byte? DeliveryOrder { get; set; }
        [DataMember]
        public DispatchBoardType BoardType { get; set; }

        [DataMember] public bool WasAtPickup { get; set; }
        [DataMember] public bool WasAtDelivery { get; set; }

        #region [ Company ]

        [DataMember]
        public Guid? DatBrokerId { get; set; }
        [DataMember]
        public Guid? CustomerId { get; set; }

        // Broker Dispatch
        [DataMember]
        public string CustomerCompany { get; set; }
        // Carrier Dispatch
        [DataMember]
        public string DatBrokerCompany { get; set; }
        [DataMember]
        public int? McNumber { get; set; }
        /// <summary>
        ///     JSON </summary>
        [DataMember]
        public string Contacts { get; set; }


        #endregion [ Company ]

        #region [ files ]

        [DataMember]
        public string BOLFile { get; set; }

        [DataMember]
        public DateTime? BOLDate { get; set; }

        [DataMember]
        public string PODFile { get; set; }

        [DataMember]
        public DateTime? PODDate { get; set; }

        [DataMember]
        public string LumperFile { get; set; }

        [DataMember]
        public DateTime? LumperDate { get; set; }

        [DataMember]
        public string PaperworkFile { get; set; }

        [DataMember]
        public DateTime? PaperworkDate { get; set; }

        [DataMember]
        public string InvoiceNumber { get; set; }

        [DataMember]
        public string InvoiceFile { get; set; }

        [DataMember]
        public DateTime? InvoiceDate { get; set; }

        [DataMember]
        public List<string> EmailInvoiceSent { get; set; }

        [DataMember]
        public DateTime? EmailInvoiceSentDate { get; set; }

        [DataMember]
        public string CheckNumber { get; set; }

        [DataMember]
        public string CheckFile { get; set; }

        [DataMember]
        public DateTime? CheckDate { get; set; }

        [DataMember] public string ConfirmationFile { get; set; }

        [DataMember] public DateTime? ConfirmationDate { get; set; }

        [DataMember] public string ConfirmedFile { get; set; }

        [DataMember] public DateTime? EmailConfirmationSentDate { get; set; }

        [DataMember] public DateTime? ConfirmedDate { get; set; }

        [DataMember] public List<string> EmailConfirmationSent { get; set; }

        [DataMember]
        public string PhotoFile { get; set; }

        [DataMember] public DateTime? TrailerSchemeDate { get; set; }

        [DataMember] public string TrailerSchemeFile { get; set; }

        [DataMember] public Guid? TrailerSchemeId { get; set; }

        #endregion [ files ]

        [DataMember]
        public float? Payout { get; set; }

        [DataMember]
        public DateTime? NewScheduledAppointment { get; set; }
        /// <summary>
        ///     Using if Owner edited ActualCompany. Tag: "EditActualCompany" </summary>
        [DataMember]
        public bool IndexCorrected { get; set; }

        [DataMember]
        public DateTime? DeliveredDate { get; set; }

    }
}
