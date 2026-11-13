using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationDriverResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryLocationDriverDto> History { get; set; }
    }
}
