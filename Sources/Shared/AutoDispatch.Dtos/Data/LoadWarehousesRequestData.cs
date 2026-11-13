using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadWarehousesRequestData : BaseEventRequestData {
        public LoadWarehousesRequestData(Guid userId) : base(userId) {
        }
    }
}
