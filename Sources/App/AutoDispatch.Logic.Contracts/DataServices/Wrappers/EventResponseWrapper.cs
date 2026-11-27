using System;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Responses;

namespace AutoDispatch.Logic.DataServices.Wrappers {
    /// <summary>
    ///     Represents base response for data services </summary>
    /// <remarks>Wrapper above response</remarks>
    public class EventResponseWrapper {
        /// <summary>
        ///     Gets request status </summary>
        public RequestStatus Status { get; private set; }
        /// <summary>
        ///     Gets or sets error message </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        ///     Gets is response successed </summary>
        public bool IsSuccessed { get { return Status == RequestStatus.Success; } }
        /// <summary>
        ///     Event Response </summary>
        public EventResponse Response { get; set; }
        /// <summary>
        ///     A size of response </summary>
        public long Size { get; set; }
        /// <summary>
        ///     Транспортная ошибка — по ней EngineManager решает, повторять ли запрос </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        ///     .ctor  </summary>
        /// <param name="response">Response data</param>
        public EventResponseWrapper(EventResponse response) {
            Status = RequestStatus.Success;
            Response = response;
        }
        /// <summary>
        ///     .ctor </summary>
        /// <param name="exception">Exception</param>
        public EventResponseWrapper(Exception exception) {
            Status = RequestStatus.Failed;
            Exception = exception;
#if DEBUG
            ErrorMessage = exception.ToString();
#else
            ErrorMessage = exception.Message;
#endif
        }
    }
}
