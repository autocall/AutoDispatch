using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Logic.DataServices.Responses {
    public class SearchCensusCompaniesResponse : BaseResponse {
        public List<CensusCompanyDto> CensusCompanies { get; set; }
    }
}
