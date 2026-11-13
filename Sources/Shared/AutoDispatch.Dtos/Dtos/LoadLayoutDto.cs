using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {

    [DataContract]
    public class LoadLayoutDto {
        /// <summary>
        ///     0 - 30 </summary>
        [DataMember]public byte Index { get; set; }
        /// <summary>
        ///     0 - 4 </summary>
        [DataMember]public byte Level { get; set; }
    }
}
