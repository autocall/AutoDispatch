using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos.Base {
    [DataContract]
    public abstract class BaseEventDto : IBaseEventDto {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int HistoryId { get; set; }

        [DataMember]
        public int EventId { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public Guid CreatedById { get; set; }
    }
}
