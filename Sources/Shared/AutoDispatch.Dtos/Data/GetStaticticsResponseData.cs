using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class GetStatisticsResponseData : BaseEventResponseData {
        /// <summary>
        ///     Total for week </summary>
        [TargetResult]
        public int Total { get; set; }
    }
}
