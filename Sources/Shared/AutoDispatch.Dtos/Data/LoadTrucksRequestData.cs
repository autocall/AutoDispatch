using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadTrucksRequestData : BaseEventRequestData {

        public bool AllDefects { get; set; }

        public LoadTrucksRequestData(Guid userId) : base(userId) {
        }
    }
}
