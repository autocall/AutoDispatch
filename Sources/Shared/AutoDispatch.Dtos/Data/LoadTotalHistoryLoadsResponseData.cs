using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response list of histories of loads </summary>
    [Serializable]
    public class LoadTotalHistoryLoadsResponseData : BaseEventResponseData {
        /// <summary>
        ///     Histories </summary>
        [TargetResult]
        public Dictionary<DateTime, int> Histories { get; set; }
    }
}
