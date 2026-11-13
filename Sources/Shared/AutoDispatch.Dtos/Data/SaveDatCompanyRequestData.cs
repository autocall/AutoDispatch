using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveDatCompanyRequestData : BaseEventRequestData {
        public DatCompanyDto DatCompany { get; set; }

        public SaveDatCompanyRequestData(Guid userId) : base(userId) {
        }
    }
}
