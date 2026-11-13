using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadTrucksResponseData : BaseEventResponseData {
        [TargetResult]
        public List<TruckDto> Trucks { get; set; }
    }
}
