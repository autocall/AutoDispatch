using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveLoadsResponseData : BaseEventResponseData {
        [TargetResult]
        public List<LoadDto> Loads { get; set; }
    }
}
