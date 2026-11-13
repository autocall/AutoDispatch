using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteWarehouseRequestData : BaseEventRequestData {
        public Guid WarehouseId { get; set; }

        public DeleteWarehouseRequestData(Guid userId) : base(userId) {
        }
    }
}
