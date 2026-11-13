using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveTrailerResponseData : BaseEventResponseData {
        [TargetResult]
        public TrailerDto Trailer { get; set; }
    }
}
