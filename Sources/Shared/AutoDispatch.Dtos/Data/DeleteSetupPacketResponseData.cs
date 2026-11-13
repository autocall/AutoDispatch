using System;
using AutoDispatch.Dtos.Dtos;
using System.Collections.Generic;
using AutoCall.Common.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteSetupPacketResponseData : BaseEventResponseData {
        [TargetResult]
        public SetupPacketDto SetupPacket { get; set; }
    }
}
