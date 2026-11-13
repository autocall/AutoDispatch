using System.ComponentModel;

namespace AutoDispatch.Dtos.Enums {
    public enum CurrencyType : byte {
        [Description("$")]
        Dollar = 0,
        [Description("C$")]
        CanadianDollar = 1,
    }
}
