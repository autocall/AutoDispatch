using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryObjectsResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryObjectDto> Histories { get; set; }
    }
}
