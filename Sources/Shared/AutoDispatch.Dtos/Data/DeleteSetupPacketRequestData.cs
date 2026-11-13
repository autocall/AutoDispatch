using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteSetupPacketRequestData : BaseEventRequestData {
        public Guid SetupPacketId { get; set; }

        public DeleteSetupPacketRequestData(Guid userId) : base(userId) {
        }
    }
}
