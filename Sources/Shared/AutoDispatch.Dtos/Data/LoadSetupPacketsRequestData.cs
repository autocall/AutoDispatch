using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of list SetupPackets </summary>
    [Serializable]
    public class LoadSetupPacketsRequestData : BaseEventRequestData {
        public LoadSetupPacketsRequestData(Guid userId) : base(userId) {
        }
    }
}
