using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDatCompanyRequestData : BaseEventRequestData {
        public Guid DatCompanyId { get; set; }

        public DeleteDatCompanyRequestData(Guid userId) : base(userId) {
        }
    }
}
