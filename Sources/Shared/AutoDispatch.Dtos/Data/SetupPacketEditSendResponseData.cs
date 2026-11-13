using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SetupPacketEditSendResponseData : BaseEventResponseData {
        [TargetResult]
        public DatCompanyDto DatCompany { get; set; }
    }
}
