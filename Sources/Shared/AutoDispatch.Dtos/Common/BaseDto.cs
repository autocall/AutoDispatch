using System;
using System.Runtime.Serialization;

namespace AutoCall.Common.Dtos {
    /// <summary>
    ///     Base DTO </summary>
    /// <remarks> Represent database object </remarks>
    [DataContract]
    public class BaseDto {
        /// <summary>
        ///     .ctor </summary>
        public BaseDto() {
            EntityId = Guid.NewGuid();
        }

        /// <summary>
        ///     Entity ID </summary>
        [DataMember]
        public Guid EntityId { get; set; }
        /// <summary>
        ///     First time creation date </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }
        /// <summary>
        ///     Fist time created by person id </summary>
        [DataMember]
        public Guid CreatedById { get; set; }
        /// <summary>
        ///     Fist time created by person </summary>
        [DataMember]
        public string CreatedByName { get; set; }
        /// <summary>
        ///     Last modified date </summary>
        [DataMember]
        public DateTime LastModifiedDate { get; set; }
        /// <summary>
        ///     Last modified by person id </summary>
        [DataMember]
        public Guid LastModifiedById { get; set; }
        /// <summary>
        ///     Last modified person </summary>
        [DataMember]
        public string LastModifiedByName { get; set; }
        /// <summary>
        ///     Marked as deleted </summary>
        [DataMember]
        public bool IsDeleted { get; set; }
    }
}