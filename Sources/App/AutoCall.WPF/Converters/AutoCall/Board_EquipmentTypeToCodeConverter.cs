using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using AutoCall.Common.Enums;

namespace AutoCall.WPF.Converters.AutoCall {
    /// <summary>
    ///     Код оборудования DAT в колонке Equipment бордов. V1: AutoCallEquipmentType → TfmiFreightMatchingService.EquipmentType (AutoMapper)
    ///     → IDatPowerService.GetTypesByCodes → справочник EquipmentType WebV1 (WCF, модуль AutoCall). Справочник неизменный — таблица
    ///     снята с autocall.dbo.EquipmentType 2026-11-19 и сопоставлена с конвертером V1 Equipment_WebModelToModel (None → AutoCarrier, как в V1) </summary>
    public class Board_EquipmentTypeToCodeConverter : IValueConverter {

        private static readonly Dictionary<AutoCallEquipmentType, string> Codes = new Dictionary<AutoCallEquipmentType, string>() {
            { AutoCallEquipmentType.AutoCarrier, "AC" },
            { AutoCallEquipmentType.Container, "C" },
            { AutoCallEquipmentType.ContainerInsulated, "CI" },
            { AutoCallEquipmentType.ContainerRefrigerated, "CR" },
            { AutoCallEquipmentType.DoubleDrop, "DD" },
            { AutoCallEquipmentType.DropDeckLandoll, "LA" },
            { AutoCallEquipmentType.DumpTrailer, "DT" },
            { AutoCallEquipmentType.Flatbed, "F" },
            { AutoCallEquipmentType.FlatbedAirRide, "FA" },
            { AutoCallEquipmentType.FlatbedBTrain, "BT" },
            { AutoCallEquipmentType.FlatbedDouble, "F2" },
            { AutoCallEquipmentType.FlatbedHazMat, "FZ" },
            { AutoCallEquipmentType.FlatbedHotshot, "FH" },
            { AutoCallEquipmentType.FlatbedMaxi, "MX" },
            { AutoCallEquipmentType.FlatbedwSides, "FS" },
            { AutoCallEquipmentType.FlatbedwTarps, "FT" },
            { AutoCallEquipmentType.FlatbedwTeam, "FM" },
            { AutoCallEquipmentType.FlatbedorStepDeck, "FD" },
            { AutoCallEquipmentType.FlatbedVanReefer, "FVR" },
            { AutoCallEquipmentType.HopperBottom, "HB" },
            { AutoCallEquipmentType.Lowboy, "LB" },
            { AutoCallEquipmentType.MovingVan, "MV" },
            { AutoCallEquipmentType.Pneumatic, "NU" },
            { AutoCallEquipmentType.PowerOnly, "PO" },
            { AutoCallEquipmentType.Reefer, "R" },
            { AutoCallEquipmentType.ReeferAirRide, "RA" },
            { AutoCallEquipmentType.ReeferDouble, "R2" },
            { AutoCallEquipmentType.ReeferHazMat, "RZ" },
            { AutoCallEquipmentType.ReeferIntermodal, "RN" },
            { AutoCallEquipmentType.ReeferLogistics, "RL" },
            { AutoCallEquipmentType.ReeferwTeam, "RM" },
            { AutoCallEquipmentType.RemovableGooseneck, "RG" },
            { AutoCallEquipmentType.StepDeck, "SD" },
            { AutoCallEquipmentType.StretchTrailer, "ST" },
            { AutoCallEquipmentType.TankerAluminum, "TA" },
            { AutoCallEquipmentType.TankerIntermodal, "TN" },
            { AutoCallEquipmentType.TankerSteel, "TS" },
            { AutoCallEquipmentType.TruckandTrailer, "TT" },
            { AutoCallEquipmentType.Van, "V" },
            { AutoCallEquipmentType.VanAirRide, "VA" },
            { AutoCallEquipmentType.VanConestoga, "VS" },
            { AutoCallEquipmentType.VanCurtain, "VC" },
            { AutoCallEquipmentType.VanDouble, "V2" },
            { AutoCallEquipmentType.VanHazMat, "VZ" },
            { AutoCallEquipmentType.VanHotshot, "VH" },
            { AutoCallEquipmentType.VanInsulated, "VI" },
            { AutoCallEquipmentType.VanIntermodal, "VN" },
            { AutoCallEquipmentType.VanLiftGate, "VG" },
            { AutoCallEquipmentType.VanLogistics, "VL" },
            { AutoCallEquipmentType.VanOpenTop, "OT" },
            { AutoCallEquipmentType.VanRollerBed, "VB" },
            { AutoCallEquipmentType.VanTriple, "V3" },
            { AutoCallEquipmentType.VanVented, "VV" },
            { AutoCallEquipmentType.VanwTeam, "VM" },
            { AutoCallEquipmentType.VanorFlatbedwTarps, "VT" },
            { AutoCallEquipmentType.VanorFlatbed, "VF" },
            { AutoCallEquipmentType.VanorReefer, "VR" },
            { AutoCallEquipmentType.InsulatedVanorReefer, "IR" },
            { AutoCallEquipmentType.ReeferorVentedVan, "RV" },
            { AutoCallEquipmentType.FlatbedwChains, "FC" },
            { AutoCallEquipmentType.ReeferwPalletExchange, "RP" },
            { AutoCallEquipmentType.VanwBlanketWrap, "VW" },
            { AutoCallEquipmentType.LowboyorRGN, "LR" },
            { AutoCallEquipmentType.VanwPalletExchange, "VP" },
            { AutoCallEquipmentType.StepDeckorRGN, "SR" },
            { AutoCallEquipmentType.Conveyor, "CV" },
            { AutoCallEquipmentType.FlatbedOverDimension, "FO" },
            { AutoCallEquipmentType.LowboyOverDimension, "LO" },
            { AutoCallEquipmentType.Conestoga, "CN" },
            { AutoCallEquipmentType.FlatbedConestoga, "FN" },
            { AutoCallEquipmentType.StraightBoxTruck, "SB" },
            { AutoCallEquipmentType.StepdeckConestoga, "SN" },
            { AutoCallEquipmentType.SprinterVan, "SP" },
            { AutoCallEquipmentType.SprinterVanHazmat, "SZ" },
            { AutoCallEquipmentType.SprinterVanTempControlled, "ST" },
            { AutoCallEquipmentType.SprinterVanTeam, "SM" },
            { AutoCallEquipmentType.StraightBoxTruckReefer, "SR" },
            { AutoCallEquipmentType.StraightBoxTruckHazmat, "SBZ" },
            { AutoCallEquipmentType.PowerOnlyTowaway, "PT" },
            { AutoCallEquipmentType.PowerOnlyLoadOut, "PL" },
            { AutoCallEquipmentType.None, "AC" },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is AutoCallEquipmentType equipment && Codes.TryGetValue(equipment, out var code)) {
                return code;
            }
            // V1: Equipment_WebModelToModel бросал «Undefined type»
            throw new Exception("Undefined type: " + value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
