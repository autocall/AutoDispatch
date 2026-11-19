using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices.Requests {
    /// <summary>
    ///     Represents AutoCall API request to get login result </summary>
    public class ApiLoginRequest : BaseRequest {
        [JsonProperty("login")]
        public override string UserName { get; set; }
        [JsonProperty("password")]
        public string UserPassword { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}