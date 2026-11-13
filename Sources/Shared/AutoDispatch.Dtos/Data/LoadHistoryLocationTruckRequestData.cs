using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationTruckRequestData : BaseEventRequestData {
        public DateTime DateTime { get; set; }

        public Guid TruckId { get; set; }

        public LoadHistoryLocationTruckRequestData(Guid userId) : base(userId) {
            DateTime = DateTime.Now;
        }
    }
}
