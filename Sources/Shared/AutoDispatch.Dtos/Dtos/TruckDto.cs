using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class TruckDto : BaseDispatchDto, IBaseEventDto {
        [DataMember] public string Number { get; set; }

        [DataMember] public int EventId { get; set; }

        [DataMember] public Guid? TrailerId { get; set; }

        #region [ Files ]

        [DataMember] public string AnnualInspection_File { get; set; }
        [DataMember] public DateTime? AnnualInspection_Expiration { get; set; }
        [DataMember] public string Registration_File { get; set; }
        [DataMember] public DateTime? Registration_Expiration { get; set; }

        #endregion [ Files ]

        [DataMember] public List<DefectDto> Defects { get; set; }
    }
}
