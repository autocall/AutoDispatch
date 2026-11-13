using System;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SetupPacketInviteSendResponseData : BaseEventResponseData {
        [TargetResult]
        public DatCompanyDto DatCompany { get; set; }
    }
}
