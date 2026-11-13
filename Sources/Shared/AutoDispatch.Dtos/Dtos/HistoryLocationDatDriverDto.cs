using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class HistoryLocationDatDriverDto : BaseEventLocationDto {
        [DataMember]
        public Guid DatDriverId { get; set; }
    }
}
