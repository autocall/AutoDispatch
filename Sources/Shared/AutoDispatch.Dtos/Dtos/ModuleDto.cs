using System;
using System.Runtime.Serialization;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Dtos {
    /// <summary>
    ///     Module DTO </summary>
    [DataContract]
    public class ModuleDto : BaseDispatchDto {
        /// <summary>
        ///     Title of module </summary>
        [DataMember]public string Title { get; set; }
        /// <summary>
        ///     Exists news in module </summary>
        [DataMember]public bool News { get; set; }
    }
}
