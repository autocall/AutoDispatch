using System;
using System.Runtime.Serialization;
using AutoCall.Common.Dtos;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class CustomerCompanyDto : BaseDto {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Contacts { get; set; }
        [DataMember]
        public Guid CompanyId { get; set; }
    }
}
