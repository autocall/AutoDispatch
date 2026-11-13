using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response of send message </summary>
    [Serializable]
    public class SendMessageResponseData : BaseEventResponseData {
        [TargetResult]
        public MessageDto Message { get; set; }
    }
}
