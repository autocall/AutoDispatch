using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryLocationDatDriverResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryLocationDatDriverDto> History { get; set; }
    }
}
