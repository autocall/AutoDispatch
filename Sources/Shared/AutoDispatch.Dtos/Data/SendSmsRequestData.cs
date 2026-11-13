using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SendSmsRequestData : BaseEventRequestData {

        public Guid ActualCompanyId { get; set; }
        public string ToPhone { get; set; }
        public string Message { get; set; }

        public SendSmsRequestData(Guid userId) : base(userId) {
        }
    }
}
