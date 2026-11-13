using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveSetupPacketRequestData : BaseEventRequestData {
        public SetupPacketDto SetupPacket { get; set; }

        public SaveSetupPacketRequestData(Guid userId) : base(userId) {
        }
    }
}
