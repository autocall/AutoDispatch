using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveTruckResponseData : BaseEventResponseData {
        [TargetResult]
        public TruckDto Truck { get; set; }
    }
}
