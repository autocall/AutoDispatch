using AutoCall.Common.Attributes;
using System;
using System.ComponentModel;

namespace AutoCall.Common.Enums {
    /// <summary>
    ///     DataSource types </summary>
    public enum DataSourceType {
        Unknown,
        Api,
        [DescriptionShort("DAT Carrier")]
        [Description("DAT Carrier")]
        DatPower,
        [DescriptionShort("JBHunt")]
        [Description("J.B.Hunt")]
        JBHunt,
        [DescriptionShort("CHR")]
        [Description("C.H.Robinson")]
        CHRWTrucks,
        [DescriptionShort("Knight & Swift")]
        [Description("Knight & Swift")]
        Knight,
        [DescriptionShort("Landstar")]
        [Description("Landstar")]
        Landstar,
        [DescriptionShort("Magellan")]
        [Description("Magellan")]
        Magellan,
        [DescriptionShort("BNSF")]
        [Description("BNSF Logistics")]
        Bnsf,
        [DescriptionShort("Svr Truckstop")]
        [Description("Server Truckstop")]
        Truckstop,
        [DescriptionShort("Client Truckstop")]
        [Description("Client Truckstop")]
        TruckstopClient,
        [DescriptionShort("Coyote")]
        [Description("Coyote")]
        Coyote,
        [DescriptionShort("Schneider")]
        [Description("Schneider")]
        Schneider,
        [DescriptionShort("XPO")]
        [Description("XPOLogistics")]
        XPOLogistics,
        [DescriptionShort("ArcBest")]
        [Description("ArcBest")]
        ArcBest,
        [DescriptionShort("TruckPath")]
        [Description("Truckloads")]
        Truckloads,
        [DescriptionShort("DAT Broker")]
        [Description("DAT Broker")]
        DatPowerBroker,
        [DescriptionShort("EnglandLogistics")]
        [Description("England Logistics")]
        EnglandLogistics,
    }
}