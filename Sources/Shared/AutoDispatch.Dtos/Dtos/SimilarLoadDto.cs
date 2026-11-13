using AutoCall.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class SimilarLoadDto {

        /// <summary>
        ///     Entity ID </summary>
        [DataMember] public Guid EntityId { get; set; }

        [DataMember] public int? Index { get; set; }

        [DataMember] public string OriginCity { get; set; }

        [DataMember] public string DestinationCity { get; set; }

        [DataMember] public string OriginAddress { get; set; }

        [DataMember] public string DestinationAddress { get; set; }

        [DataMember] public LocationDto OriginLocation { get; set; }

        [DataMember] public LocationDto DestinationLocation { get; set; }

        [DataMember] public byte Equipment { get; set; }

        public string EquipmentString => ((AutoCallEquipmentType)Equipment).ToString();

        [DataMember] public int? Distance { get; set; }

        [DataMember] public int? Weight { get; set; }

        [DataMember] public int? Length { get; set; }

        [DataMember] public short? Pallets { get; set; }

        [DataMember] public DateTime PickupFrom { get; set; }
        [DataMember] public DateTime PickupTo { get; set; }

        [DataMember] public DateTime DeliveryFrom { get; set; }
        [DataMember] public DateTime DeliveryTo { get; set; }

        [DataMember] public Guid? ShipperId { get; set; }
        [DataMember] public Guid? ReceiverId { get; set; }
        [DataMember] public string BrokerCompany { get; set; }
        [DataMember] public string ShipperCompany { get; set; }
        [DataMember] public string ReceiverCompany { get; set; }

        [DataMember] public byte LoadType { get; set; }
        [DataMember] public decimal? Rate { get; set; }
        [DataMember] public byte Currency { get; set; }
        [DataMember] public DateTime BookedDate { get; set; }
    }
}
