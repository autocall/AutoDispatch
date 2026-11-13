using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SendSmsResponseData : BaseEventResponseData {
        [TargetResult]
        public string Json { get; set; }
    }
}
