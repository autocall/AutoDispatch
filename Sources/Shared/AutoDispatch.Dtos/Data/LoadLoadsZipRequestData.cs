using System;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadLoadsZipRequestData : BaseEventRequestData {
        public DispatchBoardType[] Types { get; set; }
        public DateTime? HistoryBegin { get; set; }
        public DateTime? HistoryEnd { get; set; }

        public LoadLoadsZipRequestData(Guid userId) : base(userId) {
        }
    }
}
