using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadDefectsResponseData : BaseEventResponseData {
        [TargetResult]
        public List<DefectDto> Defects { get; set; }
    }
}
