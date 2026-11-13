using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class HistoryLocationLoadDto : BaseEventLocationDto {
        [DataMember]
        public Guid LoadId { get; set; }

        [DataMember]
        public int? HistoryLocationTrailerId { get; set; }
    }
}
