using System.ComponentModel;

namespace AutoDispatch.Logic.Enums {
    public enum LoadType : byte {
        [Description("Full")] Full,
        [Description("Partial")] Partial
    }
}