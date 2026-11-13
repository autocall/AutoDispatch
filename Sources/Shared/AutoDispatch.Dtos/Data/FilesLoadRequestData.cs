using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class FilesLoadRequestData : BaseEventRequestData {
        public Guid EntityId { get; set; }
        public bool LoadData { get; set; }
        public bool Preview { get; set; }

        public FilesLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
