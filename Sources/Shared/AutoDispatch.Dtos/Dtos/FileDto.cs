using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class FileDto {

        /// <summary>
        ///     Load/Company ID </summary>
        [DataMember]
        public Guid EntityId { get; set; }
        /// <summary>
        ///     Name </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        ///     Size of file </summary>
        [DataMember]
        public long Size { get; set; }
        /// <summary>
        ///     Created date </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }
        /// <summary>
        ///     Binary data </summary>
        [DataMember]
        public byte[] Data { get; set; }
        [DataMember]
        public bool Preview { get; set; }
    }
}
