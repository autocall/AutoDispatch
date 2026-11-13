using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveSetupPacketResponseData : BaseEventResponseData {
        [TargetResult]
        public SetupPacketDto SetupPacket { get; set; }
    }
}
