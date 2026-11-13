using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveWarehouseRequestData : BaseEventRequestData {
        public WarehouseDto Warehouse { get; set; }

        public SaveWarehouseRequestData(Guid userId) : base(userId) {
        }
    }
}
