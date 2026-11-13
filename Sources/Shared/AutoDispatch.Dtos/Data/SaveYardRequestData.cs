using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveYardRequestData : BaseEventRequestData {
        public YardDto Yard { get; set; }

        public SaveYardRequestData(Guid userId) : base(userId) {
        }
    }
}
