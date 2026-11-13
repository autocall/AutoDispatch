using AutoDispatch.Dtos.Enums;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadTrailersRequestData : BaseEventRequestData {

        public bool AllDefects { get; set; }

        public DispatchBoardType[] Types { get; set; }    

        public DateTime? HistoryBegin { get; set; }
        public DateTime? HistoryEnd { get; set; }

        public LoadTrailersRequestData(Guid userId) : base(userId) {
        }
    }
}
