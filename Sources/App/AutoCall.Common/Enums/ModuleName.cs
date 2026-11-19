using System.ComponentModel;

namespace AutoCall.Common.Enums {
    /// <summary>
    ///     All possible modules </summary>
    public enum ModuleName {
        /// <summary>
        ///     Admin, all rights </summary>
        [Description("AutoCall")]
        AutoCall,
        /// <summary>
        ///     Dispatcher </summary>
        [Description("AutoDispatch")]
        AutoDispatch,
        /// <summary>
        ///     User </summary>
        [Description("AutoLog")]
        AutoLog,
    }
}