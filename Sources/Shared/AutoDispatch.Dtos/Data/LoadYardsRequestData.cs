using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadYardsRequestData : BaseEventRequestData {
        public LoadYardsRequestData(Guid userId) : base(userId) {
        }
    }
}
