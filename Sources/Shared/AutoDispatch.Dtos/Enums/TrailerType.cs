using System.ComponentModel;

namespace AutoDispatch.Dtos.Enums {
    public enum TrailerType : byte {
        [Description("Van")]
        Van = 0,
        [Description("Refeer")]
        Refeer = 1,
        [Description("Container")]
        Container = 2,
    }
}
