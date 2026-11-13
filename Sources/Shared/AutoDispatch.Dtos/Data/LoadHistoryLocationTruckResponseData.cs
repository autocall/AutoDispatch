using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationTruckResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryLocationTruckDto> History { get; set; }
    }
}
