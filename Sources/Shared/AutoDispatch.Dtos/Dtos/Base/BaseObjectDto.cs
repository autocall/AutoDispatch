using System;
using System.Runtime.Serialization;
using AutoCall.Common.Dtos;

namespace AutoDispatch.Dtos.Dtos {
    [DataContract]
    public class BaseObjectDto : BaseDto {
        /// <summary>
        ///     Current history object # </summary>
        [DataMember]
        public int HistoryId { get; set; }

        [DataMember]
        public Guid CompanyId { get; set; }
    }
}
