using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteTruckRequestData : BaseEventRequestData {
        public Guid TruckId { get; set; }

        public DeleteTruckRequestData(Guid userId) : base(userId) {
        }
    }
}
