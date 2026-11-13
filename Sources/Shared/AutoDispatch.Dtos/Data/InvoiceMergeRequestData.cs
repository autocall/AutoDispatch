using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class InvoiceMergeRequestData : BaseEventRequestData {

        public Guid LoadId { get; set; }

        public bool SendInvoice { get; set; }
        public bool SendPOD { get; set; }
        public bool SendConfirmation { get; set; }
        public bool SendLumper { get; set; }

        public InvoiceMergeRequestData(Guid userId) : base(userId) {
        }
    }
}
