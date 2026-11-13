using AutoDispatch.Dtos.Enums;
using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     DTO of user </summary>
    [DataContract]
    public class UserDispatchDto {
        /// <summary>   User ID (Primary Key) </summary>
        [DataMember]public Guid Id { get; set; }
        /// <summary>   Email </summary>
        [DataMember]public string Email { get; set; }
        /// <summary>   Role id </summary>
        [DataMember]public Guid RoleId { get; set; }
        /// <summary>   Status </summary>
        [DataMember]public ChatStatus Status { get; set; }
        /// <summary>   ame </summary>
        [DataMember]public string Name { get; set; }
        /// <summary>   Phone </summary>
        [DataMember]public string Phone { get; set; }
        /// <summary>   Url of photo </summary>
        [DataMember]public string PhotoUrl { get; set; }
    }
}