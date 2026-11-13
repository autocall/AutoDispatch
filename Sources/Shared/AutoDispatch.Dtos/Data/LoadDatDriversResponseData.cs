using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadDatDriversResponseData : BaseEventResponseData {
        [TargetResult]
        public List<DatDriverDto> DatDrivers { get; set; }
    }
}
