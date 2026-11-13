using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteYardResponseData : BaseEventResponseData {
        [TargetResult]
        public YardDto Yard { get; set; }
    }
}
