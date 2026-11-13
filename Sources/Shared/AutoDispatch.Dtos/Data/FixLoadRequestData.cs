using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FixLoadRequestData : BaseEventRequestData {
        public Guid LoadId { get; set; }

        public FixLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
