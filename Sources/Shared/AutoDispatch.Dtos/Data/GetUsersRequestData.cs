using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request users of chat </summary>
    [Serializable]
    public class GetUsersRequestData : BaseEventRequestData {
        public GetUsersRequestData(Guid userId) : base(userId) {
        }
    }
}
