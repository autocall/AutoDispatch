using System;
using AutoCall.Common.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class UndoApplyResponseData : BaseEventResponseData {
        [TargetResult]
        public BaseDto Dto { get; set; }
    }
}
