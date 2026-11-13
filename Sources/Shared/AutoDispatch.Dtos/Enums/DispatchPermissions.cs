using System.ComponentModel;

namespace AutoDispatch.Dtos.Enums {
    public enum DispatchPermissions {
        [Description("Carrier Dispatch")]
        Carrier = 0,
        [Description("Broker Dispatch")]
        Broker = 1,
        [Description("Carrier Accounting")]
        CarrierAccounting = 2,
        [Description("Broker Accounting")]
        BrokerAccounting = 3,
        [Description("Editing")]
        Editing = 4,
        [Description("Deleting")]
        Delete = 5
    }
}
