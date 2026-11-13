using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {

    [DataContract]
    public class SetupPacketReferenceDto {

        [DataMember]public Guid EntityId { get; set; }

        [DataMember]public DateTime CreatedDate { get; set; }

        [DataMember]public string Contact { get; set; }

        [DataMember]public string Company { get; set; }

        [DataMember]public string Phone { get; set; }

        [DataMember]public string Email { get; set; }

        [DataMember]public Guid SetupPacketId { get; set; }
    }
}
