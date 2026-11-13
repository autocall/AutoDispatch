using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DisableCarrierViewTrackerRequestData : BaseEventRequestData {
        public Guid LoadId { get; set; }

        public DisableCarrierViewTrackerRequestData(Guid userId) : base(userId) {
        }
    }
}
