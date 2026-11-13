using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationDatDriverRequestData : BaseEventRequestData {
        public DateTime DateTime { get; set; }

        public Guid DatDriverId { get; set; }

        public LoadHistoryLocationDatDriverRequestData(Guid userId) : base(userId) {
            DateTime = DateTime.Now;
        }
    }
}
