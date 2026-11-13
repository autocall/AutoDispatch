using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class ConfirmedSendResponseData : BaseEventResponseData {
        [TargetResult]
        public LoadDto Load { get; set; }
    }
}