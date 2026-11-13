using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Груз. Координаты Origin/Destination — пары decimal вместо geography V1 (радиус в LoadHistory2 — гаверсинус в SQL) </summary>
[Index(nameof(SendSms))]
public class Load : BaseDispatchEntity {

    public int? Index { get; set; }

    [MaxLength(8)]
    public string IndexExtra { get; set; }

    [MaxLength(8)]
    public string IndexPrefix { get; set; }

    public Guid? DispatchCompanyId { get; set; }

    #region [ Tracker ]

    public int? TrackerId { get; set; }

    [MaxLength(64)]
    public string CarrierViewPhone { get; set; }

    public string CarrierViewLink { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? CarrierViewLastUpdatedDate { get; set; }

    [MaxLength(64)]
    public string CarrierViewStatus { get; set; }

    [MaxLength(256)]
    public string CarrierViewStatusesJson { get; set; }

    public bool CarrierViewLate { get; set; }

    public int? CarrierViewPickupId { get; set; }

    public int? CarrierViewDeliveryId { get; set; }

    public string CarrierViewPickupComment { get; set; }

    public string CarrierViewDeliveryComment { get; set; }

    #endregion [ Tracker ]

    public string RecordId { get; set; }

    public string BOLNumber { get; set; }

    public Guid? DestinationWarehouseId { get; set; }

    public Guid? WarehouseId { get; set; }

    public Guid? TrailerId { get; set; }

    public Guid? DatCompanyId { get; set; }

    public Guid? DatDriverId { get; set; }

    public Guid? HistoryDriverId { get; set; }

    public Guid? HistoryTruckId { get; set; }

    public Guid? HistoryTrailerId { get; set; }

    public Guid? HistoryAssignedTrailerId { get; set; }

    public Guid? HistoryWarehouseId { get; set; }

    public Guid? HistoryDatCompanyId { get; set; }

    public Guid? HistoryDatDriverId { get; set; }

    [MaxLength(128), Required]
    public string OriginCity { get; set; }

    [MaxLength(128), Required]
    public string DestinationCity { get; set; }

    /// <summary>
    ///     Daylight or Standart UTC Offset of Origin Location </summary>
    public int CurrentOriginUtcOffset { get; set; }

    /// <summary>
    ///     Daylight or Standart UTC Offset of Destination Location </summary>
    public int CurrentDestinationUtcOffset { get; set; }

    [MaxLength(256)]
    public string OriginAddress { get; set; }

    [MaxLength(256)]
    public string DestinationAddress { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal OriginLatitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal OriginLongitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal DestinationLatitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal DestinationLongitude { get; set; }

    public int? Distance { get; set; }

    public string Comments { get; set; }

    public string Comments2 { get; set; }

    public string Consolidation { get; set; }

    public byte Equipment { get; set; }

    public byte PalletType { get; set; }

    public int? Weight { get; set; }

    public int? Length { get; set; }

    public short? Pallets { get; set; }

    public short? Spots { get; set; }

    public byte WeightType { get; set; }

    public string Commodity { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime PickupFrom { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime PickupTo { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime DeliveryFrom { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime DeliveryTo { get; set; }

    public Guid? ReceiverId { get; set; }

    public Guid? ShipperId { get; set; }

    public string PickupNumber { get; set; }

    public string DeliveryNumber { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime BookedDate { get; set; }

    [Required, Column(TypeName = "datetime2")]
    public DateTime BoardChanged { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CarrierViewTracking { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LineHaul { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DatRate { get; set; }

    public string Expenses { get; set; }

    public string Gains { get; set; }

    public byte Currency { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? QuikPay { get; set; }

    public byte LoadType { get; set; }

    [ForeignKey("DestinationWarehouseId")]
    public Warehouse DestinationWarehouse { get; set; }

    [ForeignKey("WarehouseId")]
    public Warehouse Warehouse { get; set; }

    [ForeignKey("TrailerId")]
    public Trailer Trailer { get; set; }

    [ForeignKey("DatDriverId")]
    public DatDriver DatDriver { get; set; }

    [ForeignKey("DatCompanyId")]
    public DatCompany DatCompany { get; set; }

    [ForeignKey("HistoryWarehouseId")]
    public Warehouse HistoryWarehouse { get; set; }

    [ForeignKey("HistoryAssignedTrailerId")]
    public Trailer HistoryAssignedTrailer { get; set; }

    [ForeignKey("HistoryTrailerId")]
    public Trailer HistoryTrailer { get; set; }

    [ForeignKey("HistoryTruckId")]
    public Truck HistoryTruck { get; set; }

    [ForeignKey("HistoryDriverId")]
    public Driver HistoryDriver { get; set; }

    [ForeignKey("HistoryDatDriverId")]
    public DatDriver HistoryDatDriver { get; set; }

    [ForeignKey("HistoryDatCompanyId")]
    public DatCompany HistoryDatCompany { get; set; }

    [ForeignKey("ReceiverId")]
    public ShipperReceiverCompany ReceiverCompany1 { get; set; }

    [ForeignKey("ShipperId")]
    public ShipperReceiverCompany ShipperCompany1 { get; set; }

    public bool ShowPickupAddress { get; set; }

    [Required, Column(TypeName = "binary(4)")]
    public byte[] Background { get; set; }

    [Required, Column(TypeName = "binary(4)")]
    public byte[] Foreground { get; set; }

    public byte DivideType { get; set; }

    public byte ContainerType { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? ShipmentId { get; set; }

    public Guid? HistoryShipmentId { get; set; }

    public byte TrailerType { get; set; } // TODO

    public short? Temperature { get; set; }

    public byte[] Layout { get; set; }

    public DateTime? BindDate { get; set; }

    public DateTime? AssignedTrailerDate { get; set; }

    [ForeignKey("ParentId")]
    public virtual Load Parent { get; set; }

    [ForeignKey("ShipmentId")]
    public virtual Load Shipment { get; set; }

    /// <summary>
    ///     Uses ParentId </summary>
    [InverseProperty("Parent")]
    public virtual ICollection<Load> Children1 { get; set; }

    /// <summary>
    ///     Uses ShipmentId </summary>
    [InverseProperty("Shipment")]
    public virtual ICollection<Load> Children2 { get; set; }

    public byte Status { get; set; }

    public Guid? CustomStatusId { get; set; }

    public DateTime LastStatusChangedDate { get; set; }

    public byte? LastRateChanged { get; set; }

    public bool WasAtPickup { get; set; }

    public bool WasAtDelivery { get; set; }

    public byte? PickupOrder { get; set; }

    public byte? DeliveryOrder { get; set; }

    public byte Board { get; set; }

    #region [ Company ]

    // Using in Carrier Dispatch
    public Guid? DatBrokerId { get; set; }

    // Using in Broker Dispatch
    public Guid? CustomerId { get; set; }

    [ForeignKey("DatBrokerId")]
    public DatCompany DatBroker { get; set; }

    [ForeignKey("CustomerId")]
    public CustomerCompany Customer { get; set; }

    #endregion [ Company ]

    #region [ files ]

    [MaxLength(256)]
    public string BOLFile { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? BOLDate { get; set; }

    [MaxLength(256)]
    public string PODFile { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? PODDate { get; set; }

    [MaxLength(256)]
    public string LumperFile { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? LumperDate { get; set; }

    [MaxLength(256)]
    public string PaperworkFile { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? PaperworkDate { get; set; }

    [MaxLength(64)]
    public string InvoiceNumber { get; set; }

    [MaxLength(256)]
    public string InvoiceFile { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? InvoiceDate { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? EmailInvoiceSentDate { get; set; }

    /// <summary>
    ///     JSON of emails </summary>
    public string EmailInvoiceSent { get; set; }

    [MaxLength(64)]
    public string CheckNumber { get; set; }

    [MaxLength(256)]
    public string CheckFile { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? CheckDate { get; set; }

    public Guid? OldOrderId { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? ConfirmationDate { get; set; }

    [MaxLength(256)]
    public string ConfirmationFile { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? EmailConfirmationSentDate { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? ConfirmedDate { get; set; }

    [MaxLength(256)]
    public string ConfirmedFile { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? TrailerSchemeDate { get; set; }

    [MaxLength(256)]
    public string TrailerSchemeFile { get; set; }

    public Guid? TrailerSchemeId { get; set; }

    /// <summary>
    ///     JSON of emails </summary>
    public string EmailConfirmationSent { get; set; }

    [MaxLength(256)]
    public string PhotoFile { get; set; }

    #endregion [ files ]

    public float? Payout { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? NewScheduledAppointment { get; set; }

    public bool Submited { get; set; }

    /// <summary>
    /// Need to send sms </summary>
    public bool SendSms { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DeliveredDate { get; set; }

    [ForeignKey("DispatchCompanyId")]
    public DispatchCompany DispatchCompany1 { get; set; }
}
