using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class BnsfRefreshResponseData : BaseEventResponseData {
        [TargetResult]
        public BnsfRefreshResultDto Result { get; set; }
    }
}
