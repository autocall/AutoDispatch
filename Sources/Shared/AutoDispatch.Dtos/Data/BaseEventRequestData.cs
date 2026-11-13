using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Base class of request to engine service </summary>
    [Serializable]
    public abstract class BaseEventRequestData {

        /// <summary>
        ///     .ctor </summary>
        protected BaseEventRequestData(Guid userId) {
            UserId = userId;
        }

        /// <summary>
        ///     User ID </summary>
        public Guid UserId { get; private set; }
    }
}
