using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SendSchemeEmailRequestData : BaseEventRequestData {

        public Guid ActualCompanyId { get; set; }
        public string Emails { get; set; }
        public string TrailerNumber { get; set; }
        public byte[] Data { get; set; }

        public SendSchemeEmailRequestData(Guid userId) : base(userId) {
        }
    }
}
