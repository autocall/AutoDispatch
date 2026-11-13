using AutoDispatch.Dtos.Enums;
using System;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of request of list companies </summary>
    [Serializable]
    public class LoadDatCompaniesRequestData : BaseEventRequestData {
        public LoadDatCompaniesRequestData(Guid userId) : base(userId) {
        }
    }
}
