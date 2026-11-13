using System;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadLoadsRequestData : BaseEventRequestData {
        public DispatchBoardType[] Types { get; set; }
        public DateTime? HistoryBegin { get; set; }
        public DateTime? HistoryEnd { get; set; }

        public LoadLoadsRequestData(Guid userId) : base(userId) {
        }
    }
}
