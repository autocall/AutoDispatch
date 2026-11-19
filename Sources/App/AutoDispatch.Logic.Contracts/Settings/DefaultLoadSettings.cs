using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Logic.Contracts.Settings {
    public sealed class DefaultLoadSettings {

        public string OriginCity { get; set; }

        public string DestinationCity { get; set; }

        public string OriginAddress { get; set; }

        public string DestinationAddress { get; set; }

        public LocationDto OriginLocation { get; set; }

        public LocationDto DestinationLocation { get; set; }

        public int Equipment { get; set; }

        public int? Weight { get; set; }

        public int? Length { get; set; }

        public short? Pallets { get; set; }

        public DateTime PickupFrom { get; set; }
        public DateTime PickupTo { get; set; }

        public DateTime DeliveryFrom { get; set; }
        public DateTime DeliveryTo { get; set; }

        public string Comments { get; set; }
        public string Comments2 { get; set; }
        public string Consolidation { get; set; }

        public Guid? BrokerId { get; set; }
        public Guid? ReceiverId { get; set; }
        public Guid? ShipperId { get; set; }
        public string BrokerCompany { get; set; }
        public string ReceiverCompany { get; set; }
        public string ShipperCompany { get; set; }
        /// <summary>
        ///     JSON </summary>
        public string Contacts { get; set; }
        /// <summary>
        ///     JSON </summary>
        public string ShipperContacts { get; set; }
        /// <summary>
        ///     JSON </summary>
        public string ReceiverContacts { get; set; }

        public byte LoadType { get; set; }
        public float? Rate { get; set; }
        public int? McNumber { get; set; }
        public byte Currency { get; set; }
        public byte TrailerType { get; set; }
        public short? Temperature { get; set; }
    }
}
