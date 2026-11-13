using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationTrailerRequestData : BaseEventRequestData {
        public DateTime DateTime { get; set; }

        public Guid TrailerId { get; set; }

        public LoadHistoryLocationTrailerRequestData(Guid userId) : base(userId) {
            DateTime = DateTime.Now;
        }
    }
}
