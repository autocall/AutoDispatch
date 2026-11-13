using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FixLoadResponseData : BaseEventResponseData {
        [TargetResult]
        public LoadDto Load { get; set; }
    }
}
