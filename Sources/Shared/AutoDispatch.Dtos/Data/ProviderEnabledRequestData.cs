using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of enable gps provider </summary>
    [Serializable]
    public class ProviderEnabledRequestData : BaseEventRequestData {
        public ProviderEnabledRequestData(Guid userId) : base(userId) {
        }
    }
}
