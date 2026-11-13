using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadShipperReceiverCompaniesRequestData : BaseEventRequestData {
        public LoadShipperReceiverCompaniesRequestData(Guid userId) : base(userId) {
        }
    }
}
