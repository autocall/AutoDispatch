using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Requests;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic.DataServices.Wrappers;

namespace AutoDispatch.Logic.DataServices {
    /// <summary>
    ///     Represents data service for AutoDispatch Web API: Api/Dispatch/Request и long-polling Api/Dispatch/Response (AI/wiki/protocol.md).
    ///     Тело — EventRequest в JSON Newtonsoft с $type, Content-Type x-www-form-urlencoded, как V1 (сервер читает поток) </summary>
    /// <remarks>Zip/Json: LoadLoads 10/17sec,LoadSetupPackets 7/9sec/. Вариант с RequestZip/ResponseZip в V1 был закомментирован и не перенесён</remarks>
    public class EngineDataService : BaseDataService {

        #region [ Private Fields ]

        /// <summary>
        ///     Gets request url </summary>
        private string RequestUrl => "Api/Dispatch/Request";
        /// <summary>
        ///     Gets response url </summary>
        private string ResponseUrl => "Api/Dispatch/Response";

        #endregion [ Private Fields ]

        public EngineDataService(HttpClient http) : base(http) { }

        #region [ Public Methods. Serialize as JSON ]

        /// <summary>
        ///     Sends event and try to get response </summary>
        /// <remarks>
        /// Write input data to db
        /// </remarks>
        /// <param name="eventRequest">Request data</param>
        /// <returns>Wrapper on Response Data</returns>
        public async Task<EventResponseWrapper> RequestAsync(EventRequestWrapper eventRequest) {
            try {
                var data = await PostEventAsync(RequestUrl, eventRequest.Request);
                return new EventResponseWrapper(data);
            } catch (Exception e) {
                return new EventResponseWrapper(e);
            }
        }

        /// <summary>
        ///     Trys to get response </summary>
        /// <remarks> Result operation doesn't write to log </remarks>
        /// <param name="eventId">Last event id that occurred</param>
        /// <param name="eventTypes">Type of event</param>
        /// <returns>Wrapper on Response Data</returns>
        public async Task<EventResponseWrapper> ResponseAsync(int? eventId, byte[] eventTypes) {
            var session = UserSession.Current;
            var requestData = new EventNotificationRequestData(session.UserId, eventId, eventTypes);
            var eventRequest = new EventRequest() { Data = requestData };
            try {
                var data = await PostEventAsync(ResponseUrl, eventRequest);
                return new EventResponseWrapper(data);
            } catch (Exception e) {
                return new EventResponseWrapper(e);
            }
        }

        #endregion [ Public Methods ]

        /// <summary>
        ///     V1: HttpWebRequest бросал WebException на ответ не 2xx — здесь то же через EnsureSuccessStatusCode </summary>
        private async Task<EventResponse> PostEventAsync(string url, EventRequest eventRequest) {
            using (var request = CreateRequest(HttpMethod.Post, url)) {
                string strRequest = JsonExtension.SerializeObject(eventRequest);
                request.Content = new StringContent(strRequest, Encoding.UTF8, "application/x-www-form-urlencoded");
                using (var response = await Http.SendAsync(request)) {
                    response.EnsureSuccessStatusCode();
                    string strResult = await response.Content.ReadAsStringAsync();
                    return JsonExtension.DeserializeObject<EventResponse>(strResult);
                }
            }
        }
    }
}
