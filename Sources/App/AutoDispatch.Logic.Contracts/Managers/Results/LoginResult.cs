using System;
using AutoDispatch.Logic.Enums;

namespace AutoDispatch.Logic.Managers.Results {
    /// <summary>
    ///     Represents login result </summary>
    public class LoginResult : BaseResult {
        /// <summary>
        ///     Gets or sets login state </summary>
        public LoginState State { get; set; }
        /// <summary>
        ///     Gets or sets User ID </summary>
        public Guid UserId { get; set; }
        /// <summary>
        ///     Gets or sets UserName </summary>
        public string UserName { get; set; }
        /// <summary>
        ///     Token </summary>
        public string Token { get; set; }
    }
}