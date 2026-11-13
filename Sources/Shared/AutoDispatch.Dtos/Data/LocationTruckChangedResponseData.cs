using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationTruckChangedResponseData : BaseEventResponseData {
        [TargetResult]
        public TruckDto Truck { get; set; }
    }
}
