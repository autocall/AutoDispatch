using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SyncCarrierViewTrackerResponseData : BaseEventResponseData {
        [TargetResult]
        public int? TrackerId { get; set; }
    }
}
