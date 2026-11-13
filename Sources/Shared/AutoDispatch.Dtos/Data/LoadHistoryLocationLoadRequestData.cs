using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationLoadRequestData : BaseEventRequestData {
        public Guid LoadId { get; set; }

        public LoadHistoryLocationLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
