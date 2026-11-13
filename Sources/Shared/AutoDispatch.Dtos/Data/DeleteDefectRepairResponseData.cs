using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDefectRepairResponseData : BaseEventResponseData {
        [TargetResult]
        public DefectRepairDto Repair { get; set; }
    }
}
