using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDefectRepairRequestData : BaseEventRequestData {
        public Guid RepairId { get; set; }

        public DeleteDefectRepairRequestData(Guid userId) : base(userId) {
        }
    }
}
