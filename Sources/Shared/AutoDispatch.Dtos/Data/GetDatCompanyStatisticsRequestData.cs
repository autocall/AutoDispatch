using AutoDispatch.Dtos.Enums;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class GetDatCompanyStatisticsRequestData : BaseEventRequestData {
        public GetDatCompanyStatisticsRequestData(Guid userId) : base(userId) {
        }
    }
}
