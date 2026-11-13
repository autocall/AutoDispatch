using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteLoadRequestData : BaseEventRequestData {
        public Guid LoadId { get; set; }

        public DeleteLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
