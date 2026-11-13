using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteTrailerResponseData : BaseEventResponseData {
        [TargetResult]
        public TrailerDto Trailer { get; set; }
    }
}
