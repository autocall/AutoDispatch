using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveDriverResponseData : BaseEventResponseData {
        [TargetResult]
        public DriverDto Driver { get; set; }
    }
}
