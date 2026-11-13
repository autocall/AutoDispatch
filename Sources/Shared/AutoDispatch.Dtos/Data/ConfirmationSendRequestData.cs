using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class ConfirmationSendRequestData : BaseEventRequestData {

        public Guid LoadId { get; set; }

        public List<string> Emails { get; set; }

        public ConfirmationSendRequestData(Guid userId) : base(userId) {
        }
    }
}
