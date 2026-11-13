using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteTrailerRequestData : BaseEventRequestData {
        public Guid TrailerId { get; set; }

        public DeleteTrailerRequestData(Guid userId) : base(userId) {
        }
    }
}
