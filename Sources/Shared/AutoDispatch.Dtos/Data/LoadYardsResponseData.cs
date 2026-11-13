using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadYardsResponseData : BaseEventResponseData {
        [TargetResult]
        public List<YardDto> Yards { get; set; }
    }
}
