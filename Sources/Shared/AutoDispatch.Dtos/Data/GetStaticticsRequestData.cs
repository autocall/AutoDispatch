using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class GetStatisticsRequestData : BaseEventRequestData {
        public GetStatisticsRequestData(Guid userId) : base(userId) {
        }
    }
}
