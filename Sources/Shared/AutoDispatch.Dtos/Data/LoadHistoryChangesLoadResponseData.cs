using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryChangesLoadResponseData : BaseEventResponseData {
        [TargetResult]
        public List<HistoryObjectDto> History { get; set; }
    }
}
