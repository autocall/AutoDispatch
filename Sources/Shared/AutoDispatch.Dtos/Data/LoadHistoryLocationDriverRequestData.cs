using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationDriverRequestData : BaseEventRequestData {
        public DateTime DateTime { get; set; }

        public Guid DriverId { get; set; }

        public LoadHistoryLocationDriverRequestData(Guid userId) : base(userId) {
            DateTime = DateTime.Now;
        }
    }
}
