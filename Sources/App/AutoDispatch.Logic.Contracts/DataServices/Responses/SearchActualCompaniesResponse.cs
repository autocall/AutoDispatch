using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices.Responses {
    public class SearchActualCompaniesResponse : BaseResponse {
        [JsonProperty("ActualCompanies")]
        public List<ActualCompanyDto> ActualCompanies { get; set; }
    }
}
