using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveWarehouseResponseData : BaseEventResponseData {
        [TargetResult]
        public WarehouseDto Warehouse { get; set; }
    }
}
