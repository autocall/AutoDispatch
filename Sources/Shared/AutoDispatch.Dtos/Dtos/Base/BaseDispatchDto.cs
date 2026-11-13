using System;
using System.Runtime.Serialization;

namespace AutoDispatch.Dtos.Dtos.Base {
    /// <summary>
    ///     Base Dispatch DTO </summary>
    /// <remarks> Represent database object </remarks>
    [DataContract]
    public class BaseDispatchDto : BaseObjectDto {
        [DataMember]
        public LocationDto CurrentLocation { get; set; }
    }
}