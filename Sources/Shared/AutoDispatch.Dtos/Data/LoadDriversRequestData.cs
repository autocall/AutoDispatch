using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of list drivers </summary>
    [Serializable]
    public class LoadDriversRequestData : BaseEventRequestData {
        public LoadDriversRequestData(Guid userId) : base(userId) {
        }
    }
}
