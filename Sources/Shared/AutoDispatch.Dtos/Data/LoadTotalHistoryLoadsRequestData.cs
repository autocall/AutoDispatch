using System;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of list histories of loads </summary>
    [Serializable]
    public class LoadTotalHistoryLoadsRequestData : BaseEventRequestData {

        public DispatchBoardType Board { get; set; }
        
        public LoadTotalHistoryLoadsRequestData(Guid userId) : base(userId) {
        }
    }
}
