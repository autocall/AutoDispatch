using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadCustomerCompaniesResponseData : BaseEventResponseData {
        [TargetResult]
        public List<CustomerCompanyDto> Companies { get; set; }
    }
}
