using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     V1 api/DatCompany/Search, Home/SearchCensusCompanies, CarrierRegistration/FactoringCompanies → Api/DatCompany/Search,
    ///     Api/Census/Search, Api/SetupPacket/FactoringCompanies (AI/wiki/protocol.md) </summary>
    public class DatCompanyDataService : BaseDataService {

        public DatCompanyDataService(HttpClient http) : base(http) { }

        #region [ Public Methods ]

        public SearchByMcNumberResponse SearchByMcDotNumber(SearchByMcDotNumberRequest request) {
            return SearchByMcDotNumberAsync(request).GetAwaiter().GetResult();
        }

        public Task<SearchByMcNumberResponse> SearchByMcDotNumberAsync(SearchByMcDotNumberRequest request) {
            return GetResponseAsync(async () => new SearchByMcNumberResponse() {
                Fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    await SendAsync(HttpMethod.Get, $"Api/DatCompany/Search?dot={request.DOT}&mc={request.MC}"))
            });
        }

        public Task<GetFactoringCompaniesResponse> GetFactoringCompanies(GetFactoringCompaniesRequest request) {
            return GetResponseAsync(async () => new GetFactoringCompaniesResponse() {
                Data = JsonConvert.DeserializeObject<List<SetupPacketFactoringCompanyDto>>(await SendAsync(HttpMethod.Get, "Api/SetupPacket/FactoringCompanies"))
            });
        }

        // мёртвый код V1: api/DatCompany/SendSetupPocket на сервере V1 отсутствует (AI/wiki/protocol.md)
        //public SendSetupPackageResponse SendSetupPocket(SendSetupPackageRequest request) {
        //    // Create and send request
        //    HttpRequestResult result = DoRequest(SendSetupPocketUrl, "POST", "\"" + request.CompanyId.ToString() + "\"", "application/json");
        //    if (result.Success) {
        //        var response = JsonConvert.DeserializeObject<SendSetupPackageResponse>(result.Data);
        //        return response;
        //    } else {
        //        return new SendSetupPackageResponse() { Status = ResponseStatus.Failed, ErrorMessage = result.Errors };
        //    }
        //}

        public Task<SearchCensusCompaniesResponse> SearchCensusCompaniesAsync(SearchCensusCompaniesRequest request) {
            return GetResponseAsync(async () => new SearchCensusCompaniesResponse() {
                CensusCompanies = JsonConvert.DeserializeObject<List<CensusCompanyDto>>(await SendAsync(HttpMethod.Post, "Api/Census/Search", request.Search))
            });
        }

        #endregion [ Public Methods ]
    }
}
