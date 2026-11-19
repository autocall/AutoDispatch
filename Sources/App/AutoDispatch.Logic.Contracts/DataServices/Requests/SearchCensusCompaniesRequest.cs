using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Logic.DataServices.Requests {
    public class SearchCensusCompaniesRequest : BaseRequest {
        public CensusCompanySearchDto Search { get; set; }
    }
}
