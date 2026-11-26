using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     Помощники окна груза: V1 Dispatch/SearchLoads|SearchCustomers|GetPlaceAutocomplete|GetPlaceDetails|GetDistance|ExtractLoadFromPdf(2)
    ///     → Api/Dispatch/Helpers/* сервера (AI/wiki/protocol.md). Место/детали — JSON без gzip (V1 — ZipExtension) </summary>
    public class HelperDataService : BaseDataService {

        #region [ Private Properties ]

        private const string Url = "Api/Dispatch/Helpers/";

        #endregion [ Private Properties ]

        public HelperDataService(HttpClient http) : base(http) { }

        #region [ Public Methods ]

        public Task<SearchLoadsResponse> SearchLoads(SearchLoadsRequest request) {
            return GetResponseAsync(async () => new SearchLoadsResponse() {
                Loads = JsonConvert.DeserializeObject<List<QueryLoadDto>>(await SendAsync(HttpMethod.Post, Url + "SearchLoads", request.SearchTexts))
            });
        }

        public Task<SearchCustomersResponse> SearchCustomers(SearchCustomersRequest request) {
            return GetResponseAsync(async () => new SearchCustomersResponse() {
                Companies = JsonConvert.DeserializeObject<List<CustomerCompanyDto>>(await SendAsync(HttpMethod.Post, Url + "SearchCustomers", request.Text))
            });
        }

        public async Task<JArray> GetPlaceAutocomplete(string search) {
            var json = await SendAsync(HttpMethod.Get, $"{Url}PlaceAutocomplete?search={Uri.EscapeDataString(search ?? String.Empty)}");
            return ((dynamic)JsonConvert.DeserializeObject(json)).predictions;
        }

        public async Task<JObject> GetPlaceDetails(string palceId) {
            var json = await SendAsync(HttpMethod.Get, $"{Url}PlaceDetails?placeId={Uri.EscapeDataString(palceId ?? String.Empty)}");
            return ((dynamic)JsonConvert.DeserializeObject(json)).result;
        }

        public Task<GetDistanceResponse> GetDistance(LocationDto location1, LocationDto location2) {
            return GetResponseAsync(async () => JsonConvert.DeserializeObject<GetDistanceResponse>(await SendAsync(HttpMethod.Get,
                String.Format(CultureInfo.InvariantCulture, "{0}Distance?lat1={1}&lng1={2}&lat2={3}&lng2={4}", Url,
                    location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude))));
        }

        /// <summary>
        ///     V1 — WebClient.UploadFileTaskAsync через временный файл; теперь multipart file из памяти </summary>
        public async Task<ExtractLoadFromPdf2Response> ExtractLoadFromPdfAsync(string fileName, byte[] data, DispatchBoardType board) {
            try {
                using (var request = CreateRequest(HttpMethod.Post, $"{Url}ExtractLoadFromPdf?board={(int)board}")) {
                    var content = new MultipartFormDataContent();
                    content.Add(new ByteArrayContent(data), "file", String.IsNullOrEmpty(fileName) ? "file.pdf" : fileName);
                    request.Content = content;
                    using (var response = await Http.SendAsync(request)) {
                        var text = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode == false) {
                            throw new Exception(ReadError(text) ?? $"Server responded {(int)response.StatusCode}");
                        }
                        return new ExtractLoadFromPdf2Response() { Status = ResponseStatus.Success, Data = JsonConvert.DeserializeObject<LoadDto>(text) };
                    }
                }
            } catch (Exception e) {
                Debug.WriteLine(e.ToString());
                return new ExtractLoadFromPdf2Response() { Status = ResponseStatus.Failed, ErrorMessage = e.Message };
            }
        }

        #endregion [ Public Methods ]
    }
}
