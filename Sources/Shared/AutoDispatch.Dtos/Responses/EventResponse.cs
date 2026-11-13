using System;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Responses {
    [Serializable]
    public class EventResponse {
        /// <summary>
        ///     Event ID </summary>
        public int EventId { get; set; }
        /// <summary>
        ///     Response status </summary>
        public ResponseStatus Status { get; set; }
        /// <summary>
        ///     Error message </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        ///     Data of event response </summary>
        public BaseEventResponseData Data { get; set; }
    }
}
