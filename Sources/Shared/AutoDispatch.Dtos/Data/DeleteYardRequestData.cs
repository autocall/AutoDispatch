using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteYardRequestData : BaseEventRequestData {
        public Guid YardId { get; set; }

        public DeleteYardRequestData(Guid userId) : base(userId) {
        }
    }
}
