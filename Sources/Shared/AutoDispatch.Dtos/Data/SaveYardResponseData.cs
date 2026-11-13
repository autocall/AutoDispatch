using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveYardResponseData : BaseEventResponseData {
        [TargetResult]
        public YardDto Yard { get; set; }
    }
}
