using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class InvoiceSendRequestData : BaseEventRequestData {

        public Guid LoadId { get; set; }

        public List<string> Emails { get; set; }
        public string FactoringCompanyEmail { get; set; }

        public bool SendInvoice { get; set; }
        public bool SendPOD { get; set; }
        public bool SendConfirmation { get; set; }
        public bool SendLumper { get; set; }
        public bool Merged { get; set; }

        public string Text { get; set; }

        public InvoiceSendRequestData(Guid userId) : base(userId) {
        }
    }
}
