using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response list of SetupPackets </summary>
    [Serializable]
    public class LoadSetupPacketsResponseData : BaseEventResponseData {
        /// <summary>
        ///     Orders </summary>
        [TargetResult]
        public List<SetupPacketDto> SetupPackets { get; set; }
    }
}
