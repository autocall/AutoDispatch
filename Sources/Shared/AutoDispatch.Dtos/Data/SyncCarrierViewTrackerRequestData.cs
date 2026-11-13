using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SyncCarrierViewTrackerRequestData : BaseEventRequestData {
        public int TrakerId { get; set; }

        public SyncCarrierViewTrackerRequestData(Guid userId) : base(userId) {
        }
    }
}
