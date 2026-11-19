using System.ComponentModel;

namespace AutoCall.Logic.Enums {
    /// <summary>
    ///     Represents test port states </summary>
    public enum TestPortState {
        [Description("")]
        Default,
        [Description("Starting...")]
        TestStarted,
        [Description("Failed")]
        TestFailed,
        [Description("Success")]
        TestSuccess,
    }
}