using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class InsuranceLoadRequestData : BaseEventRequestData {
        public Guid EntityId { get; set; }

        public InsuranceLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
