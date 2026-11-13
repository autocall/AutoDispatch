using System;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     Log DTO </summary>
    [DataContract]
    public class LogDto {
        /// <summary>
        ///     ID </summary>
        [DataMember]public Guid Id { get; set; }
        /// <summary>
        ///     Created Date </summary>
        [DataMember]public DateTime CreatedDate { get; set; }
        /// <summary>
        ///     Level of log </summary>
        [DataMember]public LogLevel Level { get; set; }
        /// <summary>
        ///     Text of log </summary>
        [DataMember]public string Message { get; set; }
    }
}
