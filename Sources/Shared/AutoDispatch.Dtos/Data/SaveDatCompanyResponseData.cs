using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveDatCompanyResponseData : BaseEventResponseData {
        [TargetResult]
        public DatCompanyDto DatCompany { get; set; }
    }
}
