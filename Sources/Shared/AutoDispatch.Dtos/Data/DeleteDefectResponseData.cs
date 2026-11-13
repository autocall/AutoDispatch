using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDefectResponseData : BaseEventResponseData {
        [TargetResult]
        public DefectDto Defect { get; set; }
    }
}
