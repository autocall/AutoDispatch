using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request today messages </summary>
    [Serializable]
    public class GetTodayMessagesRequestData : BaseEventRequestData {
        /// <summary>
        ///     Opponent ID </summary>
        public Guid OpponentId { get; set; }
        /// <summary>
        ///     Time of today </summary>
        public DateTime DateTime { get; set; }

        public GetTodayMessagesRequestData(Guid userId) : base(userId) {
        }
    }
}
