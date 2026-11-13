using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteLoadsResponseData : BaseEventResponseData {
        [TargetResult]
        public List<LoadDto> Loads { get; set; }
    }
}
