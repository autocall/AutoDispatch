using Newtonsoft.Json;

namespace AutoDispatch.Logic.DataServices.Models {
    /// <summary>
    ///     Represents errors model for AutoCall API </summary>
    public class ApiErrors {
        /// <summary>
        ///     Gets or sets error description </summary>
        [JsonProperty("error_description")]
        public string Desctiption { get; set; }

        /// <summary>
        ///     Gets or sets error </summary>
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}