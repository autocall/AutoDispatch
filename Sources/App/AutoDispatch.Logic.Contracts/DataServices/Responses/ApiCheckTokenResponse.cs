using System;
using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices.Responses {
    /// <summary> 
    ///     Represents AutoCall API response to get login result </summary>
    public class ApiCheckTokenResponse : BaseResponse {
        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}