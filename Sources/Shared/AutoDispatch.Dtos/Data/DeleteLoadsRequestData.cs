using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteLoadsRequestData : BaseEventRequestData {
        public List<Guid> LoadIds { get; set; }

        public DeleteLoadsRequestData(Guid userId) : base(userId) {
        }
    }
}
