using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationDriverChangedResponseData : BaseEventResponseData {
        [TargetResult]
        public DriverDto Driver { get; set; }
    }
}
