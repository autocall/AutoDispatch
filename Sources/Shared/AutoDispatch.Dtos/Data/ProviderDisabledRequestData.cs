using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of disabled gps provider </summary>
    [Serializable]
    public class ProviderDisabledRequestData : BaseEventRequestData {
        public ProviderDisabledRequestData(Guid userId) : base(userId) {
        }
    }
}
