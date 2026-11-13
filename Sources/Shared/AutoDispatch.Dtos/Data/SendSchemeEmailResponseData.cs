using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SendSchemeEmailResponseData : BaseEventResponseData {
        [TargetResult]
        public bool Result { get; set; }
    }
}
