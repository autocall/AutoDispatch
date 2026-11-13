using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos.Base;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of long pooling response </summary>
    [Serializable]
    public class EventNotificationResponseData : BaseEventResponseData {
        /// <summary>
        ///     List of DTO of events </summary>
        public List<IBaseEventDto> Events { get; set; }
    }
}
