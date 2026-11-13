using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     Represents location DTO </summary>
    [DataContract]
    public class LocationDto {
        [DataMember]public float Longitude { get; set; }
        [DataMember]public float Latitude { get; set; }
        [DataMember]public string FormattedAddress { get; set; }
    }
}