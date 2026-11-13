using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SetupPacketEditSendRequestData : BaseEventRequestData {

        public Guid DatCompanyId { get; set; }

        public SetupPacketEditSendRequestData(Guid userId) : base(userId) {
        }
    }
}
