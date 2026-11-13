using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request available modules </summary>
    [Serializable]
    public class GetModulesRequestData : BaseEventRequestData {
        public GetModulesRequestData(Guid userId) : base(userId) {
        }
    }
}
