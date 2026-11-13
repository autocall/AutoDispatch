using System;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     DTO of message </summary>
    [DataContract]
    public class MessageDto : BaseDispatchDto, IBaseEventDto {
        [DataMember] public short ChatId { get; set; }
        /// <summary>
        ///     Text of Message </summary>
        [DataMember] public string Text { get; set; }
        /// <summary>
        ///     Text of Message </summary>
        [DataMember] public string Image { get; set; }
        /// <summary>
        ///     User ID who sent message </summary>
        [DataMember] public Guid FromId { get; set; }
        /// <summary>
        ///     User ID whom sent message </summary>
        [DataMember] public Guid ToId { get; set; }
        /// <summary>
        ///     New or read </summary>
        [DataMember] public bool New { get; set; }

        [DataMember] public int EventId { get; set; }
    }
}