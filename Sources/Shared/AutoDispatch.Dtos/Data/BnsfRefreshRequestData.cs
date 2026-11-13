using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class BnsfRefreshRequestData : BaseEventRequestData {

        public Guid ActualCompanyId { get; set; }

        public BnsfRefreshRequestData(Guid userId) : base(userId) {
        }
    }
}
