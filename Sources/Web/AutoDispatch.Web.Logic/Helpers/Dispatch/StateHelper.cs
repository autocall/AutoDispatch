using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Справочники State/Country/Zone V1 статикой (решено 2026-11-13, AI/wiki/plan.md): сняты с боевых таблиц autocall 2026-11-13.
///     Id — byte-ключи в SetupPacket.*StateId/*CountryId и SetupPacketFactoringCompany </summary>
public static class StateHelper {

    public class State {
        public byte Id { get; init; }
        public string Code { get; init; }
        public string Name { get; init; }
        public byte ZoneId { get; init; }
    }

    public class Country {
        public byte Id { get; init; }
        public string Code { get; init; }
        public string Name { get; init; }
    }

    public class Zone {
        public byte Id { get; init; }
        public string Code { get; init; }
        public string Name { get; init; }
    }

    public const byte Unknown = 100;

    public static readonly IReadOnlyList<State> States = new List<State> {
        new() { Id = 0, Code = "AB", Name = "Alberta", ZoneId = 11 },
        new() { Id = 1, Code = "AG", Name = "Aguascalientes", ZoneId = 14 },
        new() { Id = 2, Code = "AK", Name = "Alaska", ZoneId = 9 },
        new() { Id = 3, Code = "AL", Name = "Alabama", ZoneId = 3 },
        new() { Id = 4, Code = "AS", Name = "American Samoa", ZoneId = 100 },
        new() { Id = 5, Code = "AZ", Name = "Arizona", ZoneId = 8 },
        new() { Id = 6, Code = "AR", Name = "Arkansas", ZoneId = 7 },
        new() { Id = 7, Code = "BC", Name = "British Columbia", ZoneId = 11 },
        new() { Id = 8, Code = "BJ", Name = "Baja California", ZoneId = 15 },
        new() { Id = 9, Code = "BS", Name = "Baja California Sur", ZoneId = 15 },
        new() { Id = 10, Code = "CA", Name = "California", ZoneId = 9 },
        new() { Id = 11, Code = "CH", Name = "Chiapas", ZoneId = 14 },
        new() { Id = 12, Code = "CI", Name = "Chihuahua", ZoneId = 14 },
        new() { Id = 13, Code = "CL", Name = "Colima", ZoneId = 15 },
        new() { Id = 14, Code = "CO", Name = "Colorado", ZoneId = 8 },
        new() { Id = 15, Code = "CP", Name = "Campeche", ZoneId = 14 },
        new() { Id = 16, Code = "CT", Name = "Connecticut", ZoneId = 0 },
        new() { Id = 17, Code = "CU", Name = "Coahuila", ZoneId = 15 },
        new() { Id = 18, Code = "DC", Name = "District of Columbia", ZoneId = 2 },
        new() { Id = 19, Code = "DE", Name = "Delaware", ZoneId = 1 },
        new() { Id = 20, Code = "DF", Name = "Distrito Federale", ZoneId = 14 },
        new() { Id = 21, Code = "DG", Name = "Durango", ZoneId = 14 },
        new() { Id = 22, Code = "EM", Name = "Mexico", ZoneId = 14 },
        new() { Id = 23, Code = "FL", Name = "Florida", ZoneId = 3 },
        new() { Id = 24, Code = "GA", Name = "Georgia", ZoneId = 3 },
        new() { Id = 25, Code = "GJ", Name = "Guanajuato", ZoneId = 14 },
        new() { Id = 26, Code = "GR", Name = "Guerrero", ZoneId = 14 },
        new() { Id = 27, Code = "GU", Name = "GUAM", ZoneId = 100 },
        new() { Id = 28, Code = "HG", Name = "Hidalgo", ZoneId = 14 },
        new() { Id = 29, Code = "HI", Name = "Hawaii", ZoneId = 9 },
        new() { Id = 30, Code = "IA", Name = "Iowa", ZoneId = 5 },
        new() { Id = 31, Code = "ID", Name = "Idaho", ZoneId = 8 },
        new() { Id = 32, Code = "IL", Name = "Illinois", ZoneId = 6 },
        new() { Id = 33, Code = "IN", Name = "Indiana", ZoneId = 4 },
        new() { Id = 34, Code = "JA", Name = "Jalisco", ZoneId = 14 },
        new() { Id = 35, Code = "KS", Name = "Kansas", ZoneId = 6 },
        new() { Id = 36, Code = "KY", Name = "Kentucky", ZoneId = 4 },
        new() { Id = 37, Code = "LA", Name = "Louisiana", ZoneId = 7 },
        new() { Id = 38, Code = "MA", Name = "Massachusetts", ZoneId = 0 },
        new() { Id = 39, Code = "MB", Name = "Manitoba", ZoneId = 11 },
        new() { Id = 40, Code = "MD", Name = "Maryland", ZoneId = 2 },
        new() { Id = 41, Code = "ME", Name = "Maine", ZoneId = 0 },
        new() { Id = 42, Code = "MH", Name = "Michoacan", ZoneId = 14 },
        new() { Id = 43, Code = "MI", Name = "Michigan", ZoneId = 4 },
        new() { Id = 44, Code = "MN", Name = "Minnesota", ZoneId = 5 },
        new() { Id = 45, Code = "MO", Name = "Missouri", ZoneId = 6 },
        new() { Id = 46, Code = "MR", Name = "Morelos", ZoneId = 14 },
        new() { Id = 47, Code = "MS", Name = "Mississippi", ZoneId = 3 },
        new() { Id = 48, Code = "MT", Name = "Montana", ZoneId = 5 },
        new() { Id = 49, Code = "NA", Name = "Nayarit", ZoneId = 14 },
        new() { Id = 50, Code = "NE", Name = "Nebraska", ZoneId = 6 },
        new() { Id = 51, Code = "NL", Name = "Nuevo Leon", ZoneId = 15 },
        new() { Id = 52, Code = "NV", Name = "Nevada", ZoneId = 8 },
        new() { Id = 53, Code = "NB", Name = "New Brunswick", ZoneId = 12 },
        new() { Id = 54, Code = "NH", Name = "New Hampshire", ZoneId = 0 },
        new() { Id = 55, Code = "NJ", Name = "New Jersey", ZoneId = 0 },
        new() { Id = 56, Code = "NM", Name = "New Mexico", ZoneId = 8 },
        new() { Id = 57, Code = "NY", Name = "New York", ZoneId = 1 },
        new() { Id = 58, Code = "NF", Name = "Newfoundland", ZoneId = 12 },
        new() { Id = 59, Code = "NC", Name = "North Carolina", ZoneId = 2 },
        new() { Id = 60, Code = "ND", Name = "North Dakota", ZoneId = 5 },
        new() { Id = 61, Code = "NT", Name = "Northwest Territory", ZoneId = 11 },
        new() { Id = 62, Code = "NS", Name = "Nova Scotia", ZoneId = 12 },
        new() { Id = 63, Code = "NU", Name = "Nunavut", ZoneId = 100 },
        new() { Id = 64, Code = "OA", Name = "Oaxaca", ZoneId = 14 },
        new() { Id = 65, Code = "OH", Name = "Ohio", ZoneId = 4 },
        new() { Id = 66, Code = "OK", Name = "Oklahoma", ZoneId = 7 },
        new() { Id = 67, Code = "ON", Name = "Ontario", ZoneId = 13 },
        new() { Id = 68, Code = "OR", Name = "Oregon", ZoneId = 9 },
        new() { Id = 69, Code = "PA", Name = "Pennsylvania", ZoneId = 1 },
        new() { Id = 70, Code = "PE", Name = "Prince Edward Island", ZoneId = 12 },
        new() { Id = 71, Code = "PQ", Name = "Quebec", ZoneId = 13 },
        new() { Id = 72, Code = "PR", Name = "PUERTO RICO", ZoneId = 100 },
        new() { Id = 73, Code = "PU", Name = "Puebla", ZoneId = 14 },
        new() { Id = 74, Code = "QA", Name = "Queretaro", ZoneId = 14 },
        new() { Id = 75, Code = "QR", Name = "Quintana Roo", ZoneId = 14 },
        new() { Id = 76, Code = "RI", Name = "Rhode Island", ZoneId = 0 },
        new() { Id = 77, Code = "SC", Name = "South Carolina", ZoneId = 2 },
        new() { Id = 78, Code = "SD", Name = "South Dakota", ZoneId = 5 },
        new() { Id = 79, Code = "SI", Name = "Sinaloa", ZoneId = 14 },
        new() { Id = 80, Code = "SK", Name = "Saskatchewan", ZoneId = 100 },
        new() { Id = 81, Code = "SL", Name = "San Luis Potosi", ZoneId = 14 },
        new() { Id = 82, Code = "SO", Name = "Sonora", ZoneId = 15 },
        new() { Id = 83, Code = "TA", Name = "Tabasco", ZoneId = 14 },
        new() { Id = 84, Code = "TL", Name = "Tlaxcala", ZoneId = 14 },
        new() { Id = 85, Code = "TM", Name = "Tamaulipas", ZoneId = 15 },
        new() { Id = 86, Code = "TN", Name = "Tennessee", ZoneId = 3 },
        new() { Id = 87, Code = "TX", Name = "Texas", ZoneId = 7 },
        new() { Id = 88, Code = "UT", Name = "Utah", ZoneId = 8 },
        new() { Id = 89, Code = "VA", Name = "Virginia", ZoneId = 2 },
        new() { Id = 90, Code = "VI", Name = "U.S. VIRGIN ISLANDS", ZoneId = 100 },
        new() { Id = 91, Code = "VL", Name = "Navassa Island", ZoneId = 100 },
        new() { Id = 92, Code = "VT", Name = "Vermont", ZoneId = 0 },
        new() { Id = 93, Code = "WA", Name = "Washington", ZoneId = 9 },
        new() { Id = 94, Code = "WV", Name = "West Virginia", ZoneId = 2 },
        new() { Id = 95, Code = "WI", Name = "Wisconsin", ZoneId = 5 },
        new() { Id = 96, Code = "WY", Name = "Wyoming", ZoneId = 8 },
        new() { Id = 97, Code = "YC", Name = "Yucatan", ZoneId = 14 },
        new() { Id = 98, Code = "YT", Name = "Yukon Territory", ZoneId = 11 },
        new() { Id = 99, Code = "ZT", Name = "Zacatecas", ZoneId = 14 },
        new() { Id = 100, Code = "UN", Name = "UNKNOWN", ZoneId = 100 },
    };

    public static readonly IReadOnlyList<Country> Countries = new List<Country> {
        new() { Id = 0, Code = "US", Name = "USA" },
        new() { Id = 1, Code = "CA", Name = "Canada" },
        new() { Id = 2, Code = "MX", Name = "Mexico" },
        new() { Id = 100, Code = "UN", Name = "UNKNOWN" },
    };

    public static readonly IReadOnlyList<Zone> Zones = new List<Zone> {
        new() { Id = 0, Code = "Z0", Name = "New England" },
        new() { Id = 1, Code = "Z1", Name = "North East" },
        new() { Id = 2, Code = "Z2", Name = "Mid Atlantic" },
        new() { Id = 3, Code = "Z3", Name = "South East" },
        new() { Id = 4, Code = "Z4", Name = "Mid West" },
        new() { Id = 5, Code = "Z5", Name = "North Central" },
        new() { Id = 6, Code = "Z6", Name = "Central" },
        new() { Id = 7, Code = "Z7", Name = "South" },
        new() { Id = 8, Code = "Z8", Name = "Mountain" },
        new() { Id = 9, Code = "Z9", Name = "West" },
        new() { Id = 10, Code = "ZD", Name = "Canada" },
        new() { Id = 11, Code = "ZW", Name = "Western Canada" },
        new() { Id = 12, Code = "ZE", Name = "Eastern Canada" },
        new() { Id = 13, Code = "ZC", Name = "Central Canada" },
        new() { Id = 14, Code = "ZM", Name = "Mexico" },
        new() { Id = 15, Code = "ZB", Name = "Northern Mexico" },
        new() { Id = 16, Code = "ZU", Name = "Contiguous USA" },
        new() { Id = 100, Code = "UN", Name = "UNKNOWN" },
    };

    private static readonly Dictionary<byte, State> StatesById = States.ToDictionary(e => e.Id);
    private static readonly Dictionary<string, State> StatesByCode = States.ToDictionary(e => e.Code, StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<byte, Country> CountriesById = Countries.ToDictionary(e => e.Id);

    public static State GetState(byte? id) => id != null && StatesById.TryGetValue(id.Value, out var s) ? s : null;

    public static State GetStateByCode(string code) => code != null && StatesByCode.TryGetValue(code.Trim(), out var s) ? s : null;

    public static Country GetCountry(byte? id) => id != null && CountriesById.TryGetValue(id.Value, out var c) ? c : null;

    public static Zone GetZoneByCode(string code) => Zones.FirstOrDefault(e => e.Code == code);

    public static List<SetupPacketStateDto> GetStateDtos() {
        return States.Select(e => new SetupPacketStateDto { EntityId = e.Id, Code = e.Code, Name = e.Name, ZoneId = e.ZoneId }).ToList();
    }

    public static List<SetupPacketCountryDto> GetCountryDtos() {
        return Countries.Select(e => new SetupPacketCountryDto { EntityId = e.Id, Code = e.Code, Name = e.Name }).ToList();
    }
}
