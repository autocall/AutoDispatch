using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class EnableCarrierViewTrackerRequestData : BaseEventRequestData {
        public string Phone { get; set; }
        public Guid LoadId { get; set; }

        public EnableCarrierViewTrackerRequestData(Guid userId) : base(userId) {
        }
    }
}
