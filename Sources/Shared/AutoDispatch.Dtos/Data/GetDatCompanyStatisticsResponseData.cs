using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class GetDatCompanyStatisticsResponseData : BaseEventResponseData {
        [TargetResult]
        public Dictionary<Guid, int> Totals { get; set; }
    }
}
