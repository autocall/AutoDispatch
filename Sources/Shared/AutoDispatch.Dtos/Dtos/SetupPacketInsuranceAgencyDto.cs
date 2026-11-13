using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {

    [DataContract]
    public class SetupPacketInsuranceAgencyDto {

        [DataMember]public bool HasInsurance { get; set; } = true;

        [DataMember]public Guid EntityId { get; set; }

        [DataMember]public string CompanyName { get; set; }

        [DataMember]public string Phone { get; set; }

        [DataMember]public string Fax { get; set; }

        [DataMember]public string Email { get; set; }

        [DataMember]public string Number { get; set; }

    }
}
