using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of list actual companies </summary>
    [Serializable]
    public class LoadActualCompaniesRequestData : BaseEventRequestData {
        public LoadActualCompaniesRequestData(Guid userId) : base(userId) {
        }
    }
}
