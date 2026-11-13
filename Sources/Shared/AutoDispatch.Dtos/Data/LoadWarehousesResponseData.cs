using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadWarehousesResponseData : BaseEventResponseData {
        [TargetResult]
        public List<WarehouseDto> Warehouses { get; set; }
    }
}
