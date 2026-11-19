using System;

namespace AutoDispatch.Logic.DataServices.Models {
    /// <summary>
    ///     The session token for this session </summary>
    public class ApiSessionToken {
        /// <summary>
        ///     Gets or sets token </summary>
        public string AccessToken { get; set; }

        /// <summary>
        ///     Gets or sets expiration </summary>
        public DateTime Expires { get; set; }
    }
}