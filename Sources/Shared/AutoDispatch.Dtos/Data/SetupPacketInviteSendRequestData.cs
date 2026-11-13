using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SetupPacketInviteSendRequestData : BaseEventRequestData {

        public Guid DatCompanyId { get; set; }

        public List<string> Emails { get; set; }

        public bool Resend { get; set; }

        public SetupPacketInviteSendRequestData(Guid userId) : base(userId) {
        }
    }
}
