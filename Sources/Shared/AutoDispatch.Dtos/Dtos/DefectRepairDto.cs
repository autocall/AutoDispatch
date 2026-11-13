using AutoCall.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class DefectRepairDto : BaseDto {
        [DataMember] public Guid CompanyId { get; set; }
        [DataMember] public DateTime Date { get; set; }
        [DataMember] public decimal? Cost { get; set; }
        [DataMember] public string Comment { get; set; }

        [DataMember] public List<string> Files { get; set; }
    }
}
