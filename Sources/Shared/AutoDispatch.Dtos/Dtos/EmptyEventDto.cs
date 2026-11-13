using AutoDispatch.Dtos.Dtos.Base;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     Empty event DTO </summary>
    /// <remarks>191 bytes</remarks>
    /// <example>{"EventId":221,"Status":0,"ErrorMessage":null,"Data":{"$type":"AutoDispatch.Dtos.Data.EventNotificationResponseData, AutoDispatch.Dtos","Events":[{"$type":"AutoDispatch.Dtos.Dtos.EmptyEventDto, AutoDispatch.Dtos","EventId":221}]}}</example>
    [DataContract]
    public class EmptyEventDto : IBaseEventDto {
        [JsonIgnore]
        public int EventId { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
        [JsonIgnore]
        public Guid CreatedById { get; set; }
    }
}
