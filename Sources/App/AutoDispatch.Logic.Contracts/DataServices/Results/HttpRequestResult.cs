namespace AutoDispatch.Logic.DataServices.Results {
    /// <summary>
    ///     Represents result of http request </summary>
    public class HttpRequestResult {
        /// <summary>
        ///     Gets or sets request has been successed </summary>
        public bool Success { get; set; }
        /// <summary>
        ///     Gets or sets response data </summary>
        public string Data { get; set; }
        /// <summary>
        ///     Gets or sets error messages </summary>
        public string Errors { get; set; }
    }
    /// <summary>
    ///     Represents result of http request </summary>
    public class HttpRequestResult<T> {
        /// <summary>
        ///     Gets or sets request has been successed </summary>
        public bool Success { get; set; }
        /// <summary>
        ///     Gets or sets response data </summary>
        public T Data { get; set; }
        /// <summary>
        ///     Gets or sets error messages </summary>
        public string Errors { get; set; }
    }
}