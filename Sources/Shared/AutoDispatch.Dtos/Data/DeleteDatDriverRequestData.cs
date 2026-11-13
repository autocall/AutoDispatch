using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDatDriverRequestData : BaseEventRequestData {
        public Guid DatDriverId { get; set; }

        public DeleteDatDriverRequestData(Guid userId) : base(userId) {
        }
    }
}
