using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class InsuranceSaveResponseData : BaseEventResponseData {
        [TargetResult]
        public string FileName { get; set; }
    }
}
