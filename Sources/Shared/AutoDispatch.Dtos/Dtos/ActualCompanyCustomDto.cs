using System;
using System.Runtime.Serialization;
using AutoCall.Common.Dtos;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class ActualCompanyCustomDto : BaseDto {

        [DataMember]
        public Guid ActualCompanyId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Params { get; set; }

        [DataMember]
        public int Type { get; set; }
    }
}
