using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request to send message </summary>
    [Serializable]
    public class SendMessageRequestData : BaseEventRequestData {
        /// <summary>
        ///     Opponent ID </summary>
        public Guid OpponentId { get; set; }
        /// <summary>
        ///     Text of message </summary>
        public string Text { get; set; }

        public SendMessageRequestData(Guid userId) : base(userId) {
        }
    }
}
