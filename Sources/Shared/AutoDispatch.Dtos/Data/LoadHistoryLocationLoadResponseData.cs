using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationLoadResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryLocationLoadDto> History { get; set; }
    }
}
