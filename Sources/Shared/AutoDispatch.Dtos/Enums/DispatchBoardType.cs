using System.ComponentModel;

namespace AutoDispatch.Dtos.Enums {
    public enum DispatchBoardType {
        [Description("ASSIGNED LOADS")]
        Carrier,
        [Description("ASSIGNED LOADS")]
        Broker,
        [Description("ACCOUNTING")]
        CarrierAccount,
        [Description("ACCOUNTING")]
        BrokerAccount,
        [Description("HISTORY")]
        CarrierHistory

    }
}
