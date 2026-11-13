using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class ReportSimilarLoadsRequestData : BaseEventRequestData {

        public SimilarLoadDto Load { get; set; }

        public ReportSimilarLoadsRequestData(Guid userId) : base(userId) {
        }
    }
}
