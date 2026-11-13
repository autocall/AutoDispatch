using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationDatDriverChangedResponseData : BaseEventResponseData {
        [TargetResult]
        public DatDriverDto DatDriver{ get; set; }
    }
}
