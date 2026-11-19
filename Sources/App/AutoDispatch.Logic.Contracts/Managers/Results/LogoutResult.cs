using AutoDispatch.Logic.Enums;

namespace AutoDispatch.Logic.Managers.Results {
    /// <summary>
    ///     Represents logout result </summary>
    public class LogoutResult : BaseResult {
        /// <summary>
        ///     Gets or sets login state </summary>
        public LoginState State { get; set; }
    }
}