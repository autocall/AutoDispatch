using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos.Base {
    public interface IBaseEventDto {
        [DataMember]
        int EventId { get; set; }

        [DataMember]
        DateTime CreatedDate { get; set; }

        [DataMember]
        Guid CreatedById { get; set; }
    }
}
