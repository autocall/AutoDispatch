using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteWarehouseResponseData : BaseEventResponseData {
        [TargetResult]
        public WarehouseDto Warehouse { get; set; }
    }
}
