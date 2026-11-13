using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class QueryLoadDto {
        [DataMember]public string City { get; set; }
        [DataMember]public string Address { get; set; }
        [DataMember]public LocationDto Location { get; set; }
        [DataMember]public string Contacts { get; set; }
        [DataMember]public string Company { get; set; }
        [DataMember]public Guid CompanyId { get; set; }
    }
}
