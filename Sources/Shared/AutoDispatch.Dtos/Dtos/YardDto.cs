using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class YardDto : BaseObjectDto, IBaseEventDto {
        [DataMember] public string Name { get; set; }
        [DataMember] public string City { get; set; }
        [DataMember] public string Address { get; set; }
        [DataMember] public LocationDto Location { get; set; }

        [DataMember] public int EventId { get; set; }
    }
}
