using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class InsuranceSaveRequestData : BaseEventRequestData {

        public Guid EntityId { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }

        public InsuranceSaveRequestData(Guid userId) : base(userId) {
        }
    }
}
