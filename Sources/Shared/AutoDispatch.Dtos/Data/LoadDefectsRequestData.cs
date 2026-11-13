using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadDefectsRequestData : BaseEventRequestData {
        public Guid? TruckId { get; set; }
        public Guid? TrailerId { get; set; }

        public LoadDefectsRequestData(Guid userId) : base(userId) {
        }
    }
}
