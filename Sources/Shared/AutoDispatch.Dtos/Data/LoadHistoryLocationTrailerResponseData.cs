using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationTrailerResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryLocationTrailerDto> History { get; set; }
    }
}
