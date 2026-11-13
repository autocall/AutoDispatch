using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDefectRequestData : BaseEventRequestData {
        public Guid DefectId { get; set; }

        public DeleteDefectRequestData(Guid userId) : base(userId) {
        }
    }
}
