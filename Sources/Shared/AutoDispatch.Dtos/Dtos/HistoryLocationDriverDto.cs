using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class HistoryLocationDriverDto : BaseEventLocationDto {
        [DataMember]
        public Guid DriverId { get; set; }

        [DataMember]
        public int? HistoryLocationTruckId { get; set; }
    }
}
