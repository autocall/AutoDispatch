using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDriverRequestData : BaseEventRequestData {
        public Guid DriverId { get; set; }

        public DeleteDriverRequestData(Guid userId) : base(userId) {
        }
    }
}
