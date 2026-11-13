using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of long pooling request </summary>
    [Serializable]
    public class EventNotificationRequestData : BaseEventRequestData {

        /// <summary>
        ///     .ctor </summary>
        public EventNotificationRequestData(Guid userId, int? eventId, byte[] eventTypes) : base(userId) {
            EventId = eventId;
            EventTypes = eventTypes;
        }

        /// <summary>
        ///     Event ID </summary>
        public int? EventId { get; private set; }

        public byte[] EventTypes { get; private set; }
    }
}
