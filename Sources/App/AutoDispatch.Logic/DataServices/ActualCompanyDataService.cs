using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoDispatch.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     V1 ActualCompany/ActualCompaniesByUserId → Api/ActualCompany (компания — из токена) </summary>
    public class ActualCompanyDataService : BaseDataService {

        public ActualCompanyDataService(HttpClient http) : base(http) { }

        #region [ Public Methods ]

        public Task<SearchActualCompaniesResponse> SearchActualCompaniesAsync(SearchActualCompaniesRequest request) {
            request.UserId = UserSession.Current.UserId;
            return GetResponseAsync(async () => new SearchActualCompaniesResponse() {
                ActualCompanies = JsonConvert.DeserializeObject<List<ActualCompanyDto>>(await SendAsync(HttpMethod.Get, "Api/ActualCompany"))
            });
        }

        #endregion [ Public Methods ]
    }
}
