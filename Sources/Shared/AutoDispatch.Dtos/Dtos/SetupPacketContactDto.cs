using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {

    [DataContract]
    public class SetupPacketContactDto {

        [DataMember]public bool HasContact { get; set; } = true;

        [DataMember]public Guid EntityId { get; set; }

        [DataMember]public string Contact { get; set; }

        [DataMember]public string Phone { get; set; }

        [DataMember]public string Email { get; set; }

        [DataMember]public string Title { get; set; }

        [DataMember]public string Cell { get; set; }

        [DataMember]public string Fax { get; set; }

    }
}
