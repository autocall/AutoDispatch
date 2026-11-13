using System;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     Driver's carrier </summary>
    [DataContract]
    public class DatDriverDto : BaseDispatchDto, IBaseEventDto {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TruckNumber { get; set; }

        [DataMember]
        public string TrailerNumber { get; set; }

        [DataMember]
        public string ContactPhone { get; set; }

        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public string Comments { get; set; }

        [DataMember]
        public int? CarrierViewId { get; set; }
        [DataMember]
        public string CarrierViewLoadId { get; set; }
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
        public bool CarrierViewEdit { get; set; }
    }
}
