using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices.Responses {
    public class SearchByMcNumberResponse : BaseResponse {
        [JsonProperty("Fields")]
        public Dictionary<string, string> Fields { get; set; }
    }
}
