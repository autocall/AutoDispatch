using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryChangesLoadRequestData : BaseEventRequestData {
        public Guid LoadId { get; set; }

        public LoadHistoryChangesLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
