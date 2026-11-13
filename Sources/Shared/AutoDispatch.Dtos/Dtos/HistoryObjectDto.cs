using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class HistoryObjectDto {

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public byte[] Data { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public Guid ReferenceId { get; set; }
    }
}
