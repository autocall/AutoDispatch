using AutoDispatch.Logic.Enums;

namespace AutoDispatch.Logic.Managers.Results {
    /// <summary>
    ///     Represents check token result </summary>
    public class CheckTokenResult : BaseResult {
        /// <summary>
        ///     Gets or sets login state </summary>
        public LoginState State { get; set; }
    }
}