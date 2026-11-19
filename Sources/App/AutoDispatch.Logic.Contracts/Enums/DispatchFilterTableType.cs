using System.ComponentModel;

namespace AutoDispatch.Logic.Contracts.Enums {

    public enum DispatchFilterTableType {
        [Description("Carrier Dispatch | Assigned Loads")]
        Carrier_Patterns,
        [Description("Carrier Dispatch | Active Loads")]
        Carrier_Records,
        [Description("Broker Dispatch | Assigned Loads")]
        Broker_Patterns,
        [Description("Broker Dispatch | Active Loads")]
        Broker_Records,
        [Description("Carrier Accounting")]
        Accounting_Carriers,
        [Description("Broker Accounting")]
        Accounting_Brokers,
        [Description("Loads History")]
        Carrier_History,
    }

    public enum DispatchFilterLoadType {
        None,

        Carrier_Patterns,
        Carrier_Records,
        Broker_Patterns,
        Broker_Records,
        Accounting_Carriers,
        Accounting_Brokers,

        Sub_Carrier_Patterns,
        Sub_Carrier_Records,
        Sub_Broker_Patterns,
        Sub_Broker_Records,
        Sub_Accounting_Carriers,
        Sub_Accounting_Brokers,

        History,
        Sub_History,
    }
}

