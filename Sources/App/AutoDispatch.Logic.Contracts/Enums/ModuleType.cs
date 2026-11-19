using System.ComponentModel;

namespace AutoDispatch.Logic.Enums {
    public enum ModuleType {
        [Description("LogBook")]
        LogBook,
        [Description("Chat")]
        Chat,
        [Description("Dispatcher")]
        Dispatcher,
        [Description("Settings")]
        Settings,
        [Description("Unknown")]
        Unknown
    }
}
