using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class InvoiceMergeResponseData : BaseEventResponseData {
        [TargetResult]
        public FileDto File { get; set; }
    }
}
