using System.ComponentModel;

namespace AutoCall.Logic.Contracts.Enums {
    /// <summary>
    ///     Represents board types </summary>
    public enum BoardType {
        [Description("Unknown")] None = -1,
        [Description("Post Loads")] DatPowerPostLoads,
        [Description("Search Trucks")] DatPowerSearchTrucks,
        [Description("Post Trucks")] DatPowerPostTrucks,
        [Description("Search Loads")] DatPowerSearchLoads,
        [Description("Carrier Dispatch")] DispatchCarrier,
        [Description("Broker Dispatch")] DispatchBroker,
        [Description("Carrier Accounting")] DispatchCarrierAccount,
        [Description("Broker Accounting")] DispatchBrokerAccount,
        [Description("Loads History")] DispatchCarrierHistory,
    }
}