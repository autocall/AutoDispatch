using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {

    [DataContract]
    public class SetupPacketFactoringCompanyDto  {

        [DataMember]public short EntityId { get; set; }

        [DataMember]public string Address { get; set; }

        [DataMember]public string Name { get; set; }

        [DataMember]public string City { get; set; }

        [DataMember]public string ZipCode { get; set; }

        [DataMember]public string Email { get; set; }

        [DataMember]public byte StateId { get; set; }

        [DataMember]public byte CountryId { get; set; }
    }
}
