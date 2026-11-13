using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class HistoryLocationTruckDto : BaseEventLocationDto {
        [DataMember]
        public Guid TruckId { get; set; }
    }
}
