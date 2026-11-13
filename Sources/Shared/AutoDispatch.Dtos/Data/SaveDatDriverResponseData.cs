using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveDatDriverResponseData : BaseEventResponseData {
        [TargetResult]
        public DatDriverDto DatDriver { get; set; }
    }
}
