using System;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class UndoPreviewRequestData : BaseEventRequestData {

        public EventTypeModel Type { get; set; }

        public Guid EntityId { get; set; }

        public UndoPreviewRequestData(Guid userId) : base(userId) {
        }
    }
}
