using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class ConfirmedSendRequestData : BaseEventRequestData {

        public Guid LoadId { get; set; }

        public List<string> Emails { get; set; }

        public string Signature { get; set; }

        public ConfirmedSendRequestData(Guid userId) : base(userId) {
        }
    }
}
