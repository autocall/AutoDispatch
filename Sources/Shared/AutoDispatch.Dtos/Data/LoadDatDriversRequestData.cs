using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadDatDriversRequestData : BaseEventRequestData {
        public LoadDatDriversRequestData(Guid userId) : base(userId) {
        }
    }
}
