using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDatDriverResponseData : BaseEventResponseData {
        [TargetResult]
        public DatDriverDto DatDriver { get; set; }
    }
}
