using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class ReportSimilarLoadsResponseData : BaseEventResponseData {
        [TargetResult]
        public List<SimilarLoadDto> Loads { get; set; }
    }
}
