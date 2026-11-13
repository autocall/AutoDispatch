using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos.Base {
    [DataContract]
    public abstract class BaseEventLocationDto : BaseEventDto {
        [DataMember]
        public LocationDto Location { get; set; }
    }
}