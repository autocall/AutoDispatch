using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class DeleteDatCompanyResponseData : BaseEventResponseData {
        [TargetResult]
        public DatCompanyDto DatCompany { get; set; }
    }
}