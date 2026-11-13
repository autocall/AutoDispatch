using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadTrailersResponseData : BaseEventResponseData {
        [TargetResult]
        public List<TrailerDto> Trailers { get; set; }
    }
}
