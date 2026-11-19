using AutoCall.Logic.Enums;

namespace AutoCall.Logic.Managers.Results {
    /// <summary>
    ///     Represents login result </summary>
    public class LoginResult : BaseResult {
        /// <summary>
        ///     Gets or sets login state </summary>
        public LoginState State { get; set; }
        /// <summary>
        ///     Packed token </summary>
        public byte[] BinaryToken { get; set; }

        public bool DefaultPassword { get; set; }
    }
}