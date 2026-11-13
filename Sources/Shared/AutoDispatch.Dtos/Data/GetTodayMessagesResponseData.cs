using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response today messages </summary>
    [Serializable]
    public class GetTodayMessagesResponseData : BaseEventResponseData {
        /// <summary>
        ///     List of today messages </summary>
        [TargetResult]
        public List<MessageDto> Messages { get; set; }
    }
}
