using AutoCall.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class DefectDto : BaseDto {
        [DataMember] public Guid CompanyId { get; set; }
        [DataMember] public Guid? TrailerId { get; set; }
        [DataMember] public Guid? TruckId { get; set; }
        [DataMember] public Guid? RepairId { get; set; }
        [DataMember] public int DefectId { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public bool ServiceNeeded { get; set; }
        [DataMember] public bool Repaired { get; set; }

        [DataMember] public DefectRepairDto Repair { get; set; }

        [DataMember] public List<string> Files { get; set; }
    }
}
