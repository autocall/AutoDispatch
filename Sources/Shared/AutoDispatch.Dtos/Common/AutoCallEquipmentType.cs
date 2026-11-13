using System.ComponentModel;

namespace AutoCall.Common.Enums {
    /// <summary>
    ///     Types such as DatPower EquipmentTypes </summary>
    public enum AutoCallEquipmentType {
        None = -1,

        /// <remarks/>
        [Description("Auto Carrier")]
        AutoCarrier,

        /// <remarks/>
        Container,

        /// <remarks/>
        [Description("Container, Insulated")]
        ContainerInsulated,

        /// <remarks/>
        [Description("Container, Refrigerated")]
        ContainerRefrigerated,

        /// <remarks/>
        [Description("Double Drop")]
        DoubleDrop,

        /// <remarks/>
        [Description("Drop Deck, Landoll")]
        DropDeckLandoll,

        /// <remarks/>
        [Description("Dump Trailer")]
        DumpTrailer,

        /// <remarks/>
        Flatbed,

        /// <remarks/>
        [Description("Flatbed, Air-Ride")]
        FlatbedAirRide,

        /// <remarks/>
        [Description("Flatbed, B-Train")]
        FlatbedBTrain,

        /// <remarks/>
        [Description("Flatbed, Double")]
        FlatbedDouble,

        /// <remarks/>
        [Description("Flatbed, HazMat")]
        FlatbedHazMat,

        /// <remarks/>
        [Description("Flatbed, Hotshot")]
        FlatbedHotshot,

        /// <remarks/>
        [Description("Flatbed, Maxi")]
        FlatbedMaxi,

        /// <remarks/>
        [Description("Flatbed w/Sides")]
        FlatbedwSides,

        /// <remarks/>
        [Description("Flatbed w/Tarps")]
        FlatbedwTarps,

        /// <remarks/>
        [Description("Flatbed w/Team")]
        FlatbedwTeam,

        /// <remarks/>
        [Description("Flatbed or Step Deck")]
        FlatbedorStepDeck,

        /// <remarks/>
        [Description("Flatbed/Van/Reefer")]
        FlatbedVanReefer,

        /// <remarks/>
        [Description("Hopper Bottom")]
        HopperBottom,

        /// <remarks/>
        Lowboy,

        /// <remarks/>
        [Description("Moving Van")]
        MovingVan,

        /// <remarks/>
        Pneumatic,

        /// <remarks/>
        [Description("Power Only")]
        PowerOnly,

        /// <remarks/>
        Reefer,

        /// <remarks/>
        [Description("Reefer, Air-Ride")]
        ReeferAirRide,

        /// <remarks/>
        [Description("Reefer, Double")]
        ReeferDouble,

        /// <remarks/>
        [Description("Reefer, HazMat")]
        ReeferHazMat,

        /// <remarks/>
        [Description("Reefer, Intermodal")]
        ReeferIntermodal,

        /// <remarks/>
        [Description("Reefer, Logistics")]
        ReeferLogistics,

        /// <remarks/>
        [Description("Reefer w/Team")]
        ReeferwTeam,

        /// <remarks/>
        [Description("Removable Gooseneck")]
        RemovableGooseneck,

        /// <remarks/>
        [Description("Step Deck")]
        StepDeck,

        /// <remarks/>
        [Description("Stretch Trailer")]
        StretchTrailer,

        /// <remarks/>
        [Description("Tanker, Aluminum")]
        TankerAluminum,

        /// <remarks/>
        [Description("Tanker, Intermodal")]
        TankerIntermodal,

        /// <remarks/>
        [Description("Tanker, Steel")]
        TankerSteel,

        /// <remarks/>
        [Description("Truck and Trailer")]
        TruckandTrailer,

        /// <remarks/>
        Van,

        /// <remarks/>
        [Description("Van, Air-Ride")]
        VanAirRide,

        /// <remarks/>
        [Description("Van, Conestoga")]
        VanConestoga,

        /// <remarks/>
        [Description("Van, Curtain")]
        VanCurtain,

        /// <remarks/>
        [Description("Van, Double")]
        VanDouble,

        /// <remarks/>
        [Description("Van, HazMat")]
        VanHazMat,

        /// <remarks/>
        [Description("Van, Hotshot")]
        VanHotshot,

        /// <remarks/>
        [Description("Van, Insulated")]
        VanInsulated,

        /// <remarks/>
        [Description("Van, Intermodal")]
        VanIntermodal,

        /// <remarks/>
        [Description("Van, Lift-Gate")]
        VanLiftGate,

        /// <remarks/>
        [Description("Van, Logistics")]
        VanLogistics,

        /// <remarks/>
        [Description("Van, Open-Top")]
        VanOpenTop,

        /// <remarks/>
        [Description("Van, Roller Bed")]
        VanRollerBed,

        /// <remarks/>
        [Description("Van, Triple")]
        VanTriple,

        /// <remarks/>
        [Description("Van, Vented")]
        VanVented,

        /// <remarks/>
        [Description("Van w/Team")]
        VanwTeam,

        /// <remarks/>
        [Description("Van or Flatbed w/Tarps")]
        VanorFlatbedwTarps,

        /// <remarks/>
        [Description("Van or Flatbed")]
        VanorFlatbed,

        /// <remarks/>
        [Description("Van or Reefer")]
        VanorReefer,

        /// <remarks/>
        [Description("Insulated Van or Reefer")]
        InsulatedVanorReefer,

        /// <remarks/>
        [Description("Reefer or Vented Van")]
        ReeferorVentedVan,

        /// <remarks/>
        [Description("Flatbed, w/Chains")]
        FlatbedwChains,

        /// <remarks/>
        [Description("Reefer, w/Pallet Exchange")]
        ReeferwPalletExchange,

        /// <remarks/>
        [Description("Van, w/Blanket Wrap")]
        VanwBlanketWrap,

        /// <remarks/>
        [Description("Lowboy or RGN")]
        LowboyorRGN,

        /// <remarks/>
        [Description("Van, w/Pallet Exchange")]
        VanwPalletExchange,

        /// <remarks/>
        [Description("Step Deck or RGN")]
        StepDeckorRGN,

        /// <remarks/>
        Conveyor,

        /// <remarks/>
        [Description("Flatbed, Over Dimension")]
        FlatbedOverDimension,

        /// <remarks/>
        [Description("Lowboy, Over Dimension")]
        LowboyOverDimension,

        /// <remarks/>
        Conestoga,

        /// <remarks/>
        [Description("Flatbed Conestoga")]
        FlatbedConestoga,

        /// <remarks/>
        [Description("Stepdeck Conestoga")]
        StepdeckConestoga,

        /// <remarks/>
        [Description("Straight Box Truck")]
        StraightBoxTruck,

        // SP
        [Description("Sprinter Van")]
        SprinterVan,
        // SZ
        [Description("Sprinter Van Hazmat")]
        SprinterVanHazmat,
        // ST
        [Description("Sprinter Van Temp-Controlled")]
        SprinterVanTempControlled,
        // SM
        [Description("Sprinter Van Team")]
        SprinterVanTeam,
        // SR
        [Description("Straight Box Truck - Reefer")]
        StraightBoxTruckReefer,
        // SBZ
        [Description("Straight Box Truck Hazmat")]
        StraightBoxTruckHazmat,
        // PT
        [Description("Power Only Towaway")]
        PowerOnlyTowaway,
        // PL
        [Description("Power Only Load Out")]
        PowerOnlyLoadOut,
    }
}
