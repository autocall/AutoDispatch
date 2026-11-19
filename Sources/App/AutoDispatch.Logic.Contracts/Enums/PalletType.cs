using System.ComponentModel;

namespace AutoDispatch.Logic.Enums {
    public enum PalletType {
        [Description("Standard")]
        Standard,
        [Description("Oversize")]
        Oversize,
        [Description("Floor Loaded")]
        FloorLoaded
    }
}
