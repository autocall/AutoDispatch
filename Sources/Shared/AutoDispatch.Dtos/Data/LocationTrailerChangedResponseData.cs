using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationTrailerChangedResponseData : BaseEventResponseData {
        [TargetResult]
        public TrailerDto Trailer { get; set; }
    }
}
