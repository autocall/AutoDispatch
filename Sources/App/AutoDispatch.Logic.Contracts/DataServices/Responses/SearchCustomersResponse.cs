using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Logic.DataServices.Responses {
    public class SearchCustomersResponse : BaseResponse {
        public List<CustomerCompanyDto> Companies { get; set; }
    }
}
