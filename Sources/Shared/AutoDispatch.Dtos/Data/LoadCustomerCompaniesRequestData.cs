using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadCustomerCompaniesRequestData : BaseEventRequestData {
        public LoadCustomerCompaniesRequestData(Guid userId) : base(userId) {
        }
    }
}
