using AutoCall.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AutoDispatch.Dtos.Enums {
    [Flags]
    public enum DatCompanyType {
        [Description("BROKER")]
        Broker = 1,
        [Description("CARRIER")]
        Carrier = 2,
        [Description("SHIPPER")]
        Shipper = 4,
        [Description("REGISTRANT")]
        Registrant = 8,
        [Description("FREIGHT FORWARDER")]
        FreightForwarder = 16,
        [Description("CARGO TANK")]
        CargoTank = 32,
        [Description("CFTI")]
        CFTI = 64,
        [Description("IEP")]
        IEP = 128,
        [Description("CTCARRIER")]
        CTCARRIER = 256,
    }

    public static class CarShipType {
        public static List<DatCompanyType> GetDatCompanyTypes(string value) {
            if (String.IsNullOrEmpty(value)) return new List<DatCompanyType>();
            var arr = value.Split('/');
            return arr
                .Where(e => EnumExtensions.TryParseByDesctiption<DatCompanyType>(e, out var _))
                .Select(e => (DatCompanyType)EnumExtensions.ParseByDesctiption<DatCompanyType>(e))
                .ToList();
        }

        public static List<DatCompanyType> GetDatCompanyTypes_OLD(string value) {
            switch (value) {
                case "B":
                    return new List<DatCompanyType>() { DatCompanyType.Broker };
                case "BB":
                    return new List<DatCompanyType>() { DatCompanyType.Broker, DatCompanyType.Broker };
                case "BI":
                    return new List<DatCompanyType>() { DatCompanyType.Broker, DatCompanyType.IEP };
                case "C":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier };
                case "CB":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Broker };
                case "CBI":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper, DatCompanyType.Broker };
                case "CC":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Carrier };
                case "CF":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.FreightForwarder };
                case "CFB":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.FreightForwarder, DatCompanyType.Broker };
                case "CFBI":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.FreightForwarder, DatCompanyType.Broker, DatCompanyType.IEP };
                case "CFI":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.FreightForwarder, DatCompanyType.IEP };
                case "CFTI":
                    return new List<DatCompanyType>() { DatCompanyType.CFTI };
                case "CI":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.IEP };
                case "CR":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Registrant };
                case "CS":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper };
                case "CSB":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper, DatCompanyType.Broker };
                case "CSF":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper, DatCompanyType.FreightForwarder };
                case "CSFB":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper, DatCompanyType.FreightForwarder, DatCompanyType.Broker };
                case "CST":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper, DatCompanyType.CargoTank };
                case "CSTB":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.Shipper, DatCompanyType.CargoTank, DatCompanyType.Broker };
                case "CSTT":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.CargoTank };
                case "CT":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.CargoTank };
                case "CTB":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.CargoTank, DatCompanyType.Broker };
                case "CTI":
                    return new List<DatCompanyType>() { DatCompanyType.CTCARRIER, DatCompanyType.IEP };
                case "CTT":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.CargoTank };
                case "CTTT":
                    return new List<DatCompanyType>() { DatCompanyType.Carrier, DatCompanyType.CargoTank };
                case "F":
                    return new List<DatCompanyType>() { DatCompanyType.FreightForwarder };
                case "FB":
                    return new List<DatCompanyType>() { DatCompanyType.FreightForwarder, DatCompanyType.Broker };
                case "FBI":
                    return new List<DatCompanyType>() { DatCompanyType.FreightForwarder, DatCompanyType.Broker, DatCompanyType.IEP };
                case "FI":
                    return new List<DatCompanyType>() { DatCompanyType.FreightForwarder, DatCompanyType.IEP };
                case "I":
                    return new List<DatCompanyType>() { DatCompanyType.IEP };
                case "R":
                    return new List<DatCompanyType>() { DatCompanyType.Registrant };
                case "S":
                    return new List<DatCompanyType>() { DatCompanyType.Shipper };
                case "SB":
                    return new List<DatCompanyType>() { DatCompanyType.Shipper, DatCompanyType.Broker };
                case "SF":
                    return new List<DatCompanyType>() { DatCompanyType.Shipper, DatCompanyType.FreightForwarder };
                case "ST":
                    return new List<DatCompanyType>() { DatCompanyType.Shipper, DatCompanyType.CargoTank };
                case "T":
                    return new List<DatCompanyType>() { DatCompanyType.CargoTank };
                case "TB":
                    return new List<DatCompanyType>() { DatCompanyType.CargoTank, DatCompanyType.Broker };
                case "TI":
                    return new List<DatCompanyType>() { DatCompanyType.CargoTank, DatCompanyType.IEP };
                default:
                    return new List<DatCompanyType>();
            }
        }

        public static string GetAbbr(string carShip) {
            return new String(carShip.Where(e => char.IsUpper(e)).ToArray());
        }

        public static string GetDescription(string value) {
            var enums = GetDatCompanyTypes(value);
            return enums != null ? string.Join("/", enums.Select(e => e.GetDescription())) : value;
        }
    }
}