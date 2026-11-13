using System;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class UndoApplyRequestData : BaseEventRequestData {

        public EventTypeModel Type { get; set; }

        public Guid EntityId { get; set; }

        public UndoApplyRequestData(Guid userId) : base(userId) {
        }
    }
}
