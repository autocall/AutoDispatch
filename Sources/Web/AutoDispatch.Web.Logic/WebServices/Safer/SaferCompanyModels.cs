using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Logic.WebServices.Safer;

/// <summary>
///     Карточка компании FMCSA в форме api/v1 Safer (V1 SaferCompanyModel): её читают CreateFields и конвертер в CensusCompanyDto </summary>
public class SaferCompanyModel {
    public string CARSHIP { get; set; }
    public string OperatingStatusHtml { get; set; }
    public string OperatingStatus { get; set; }
    public string CommonAuthority { get; set; }
    public string ContractAuthority { get; set; }
    public string BrokerAuthority { get; set; }
    public int DOT_NUMBER { get; set; }
    public string PREFIX { get; set; }
    public int? DOCKET_NUMBER { get; set; }
    public string NAME { get; set; }
    public string NAME_DBA { get; set; }
    public string PHY_TEL_NUM { get; set; }
    public string PHY_CELL_NUM { get; set; }
    public string PHY_FAX_NUM { get; set; }
    public string EMAIL { get; set; }
    public string COMPANY_REP1 { get; set; }
    public string COMPANY_REP2 { get; set; }
    public string PHY_STR { get; set; }
    public string PHY_CITY { get; set; }
    public string PHY_ST { get; set; }
    public string PHY_ZIP { get; set; }
    public string MAI_STR { get; set; }
    public string MAI_CITY { get; set; }
    public string MAI_ST { get; set; }
    public string MAI_ZIP { get; set; }
    public int? TOT_PWR { get; set; }
    public int? TOT_DRS { get; set; }
    public string MLG150 { get; set; }
    public short? MCS150MILEAGEYEAR { get; set; }
    public DateTime? MCS_150_DATE { get; set; }
    public string DBNUM { get; set; }
    public DateTime? MCSIPDATE { get; set; }
    public bool? Phy_UpsStore { get; set; }
    public bool? Mai_UpsStore { get; set; }
    public bool? Phy_MailBox { get; set; }
    public bool? Mai_MailBox { get; set; }

    /// <summary>
    ///     Словарь полей карточки для setup packet и confirmation (V1 CarrierViewParser.CreateFields) </summary>
    public Dictionary<string, string> CreateFields() {
        var model = this;
        return new Dictionary<string, string>() {
            { "Entity Type:", model.CARSHIP?.ToUpper() },
            { "Operating Status:", model.OperatingStatus },
            { "Out of Service Date:", model.OperatingStatus == "Out Of Service" ? model.MCSIPDATE?.ToShortDateString() : "None" },
            { "Legal Name:", model.NAME },
            { "DBA Name:", model.NAME_DBA },
            { "Physical Address:", $"{model.PHY_STR}\n{model.PHY_CITY}, {model.PHY_ST} {model.PHY_ZIP}" },
            { "Phone:", model.PHY_TEL_NUM },
            { "Mailing Address:", $"{model.MAI_STR}\n{model.MAI_CITY}, {model.MAI_ST} {model.MAI_ZIP}" },
            { "USDOT Number:", model.DOT_NUMBER.ToString() },
            { "MC/MX/FF Number(s):", model.PREFIX + "-" + model.DOCKET_NUMBER?.ToString() },
            { "DUNS Number:", model.DBNUM?.Length >= 8 ? model.DBNUM.Insert(2, "-").Insert(6, "-") : model.DBNUM },
            { "Power Units:", model.TOT_PWR?.ToString() },
            { "Drivers:", model.TOT_DRS?.ToString() },
            { "MCS-150 Form Date:", model.MCS_150_DATE?.ToString("d") },
            { "MCS-150 Mileage (Year):", model.MLG150 != null && model.MCS150MILEAGEYEAR != null ?
                $"{(Int32.TryParse(model.MLG150, out var tmp) ? tmp.ToString("N0") : model.MLG150)} ({model.MCS150MILEAGEYEAR})" : model.MLG150 },
            // EXTRA
            { nameof(PHY_TEL_NUM), model.PHY_TEL_NUM },
            { nameof(PHY_CELL_NUM), model.PHY_CELL_NUM },
            { nameof(PHY_FAX_NUM), model.PHY_FAX_NUM },
            { nameof(EMAIL), model.EMAIL },
            { nameof(COMPANY_REP1), model.COMPANY_REP1 },
            { nameof(COMPANY_REP2), model.COMPANY_REP2 },
            { nameof(Phy_UpsStore), model.Phy_UpsStore?.ToString() },
            { nameof(Mai_UpsStore), model.Mai_UpsStore?.ToString() },
            { nameof(Phy_MailBox), model.Phy_MailBox?.ToString() },
            { nameof(Mai_MailBox), model.Mai_MailBox?.ToString() },
            { nameof(CommonAuthority), model.CommonAuthority },
            { nameof(ContractAuthority), model.ContractAuthority },
            { nameof(BrokerAuthority), model.BrokerAuthority },
        };
    }
}

/// <summary>
///     V1 Converters/Dispatch/Safer/WebModelToDto </summary>
public class SaferCompany_ModelToDto : TypeConverter<SaferCompanyModel, CensusCompanyDto> {
    protected override CensusCompanyDto ConvertCore(SaferCompanyModel source) {
        var destination = new CensusCompanyDto();
        destination.CARSHIP = source.CARSHIP;
        destination.DOT_NUMBER = source.DOT_NUMBER;
        destination.ICC_DOCKET_1_PREFIX = source.PREFIX;
        destination.ICC1 = source.DOCKET_NUMBER;
        destination.NAME = source.NAME;
        destination.NAME_DBA = source.NAME_DBA;
        destination.TEL_NUM = source.PHY_TEL_NUM;
        destination.CELL_NUM = source.PHY_CELL_NUM;
        destination.FAX_NUM = source.PHY_FAX_NUM;
        destination.EMAILADDRESS = source.EMAIL;
        destination.COMPANY_REP1 = source.COMPANY_REP1;
        destination.COMPANY_REP2 = source.COMPANY_REP2;
        destination.PHY_STR = source.PHY_STR;
        destination.PHY_CITY = source.PHY_CITY;
        destination.PHY_ST = source.PHY_ST;
        destination.PHY_ZIP = source.PHY_ZIP;
        destination.MAI_STR = source.MAI_STR;
        destination.MAI_CITY = source.MAI_CITY;
        destination.MAI_ST = source.MAI_ST;
        destination.MAI_ZIP = source.MAI_ZIP;
        destination.TOT_PWR = source.TOT_PWR ?? 0;
        destination.TOT_DRS = source.TOT_DRS;
        destination.MLG150 = source.MLG150;
        destination.MCS150MILEAGEYEAR = source.MCS150MILEAGEYEAR;
        destination.MCS_150_DATE = source.MCS_150_DATE;
        destination.CommonAuthority = source.CommonAuthority;
        destination.ContractAuthority = source.ContractAuthority;
        destination.BrokerAuthority = source.BrokerAuthority;
        return destination;
    }
}

#region [ api/v3 Safer ]

/// <summary>
///     Ответ brokersnapshot.com/api/v3/company?dot=…&include=3 — только поля, которые читает AutoDispatch (V1 SaferCompanyV3Model).
///     Enum'ы приходят числами, поля со значением по умолчанию в JSON не пишутся </summary>
public class SaferCompanyV3Model {
    public int DotNumber { get; set; }
    public SaferCompanyV3Name Name { get; set; }
    public List<SaferCompanyV3Location> Locations { get; set; }
    public List<SaferCompanyV3Phone> Phones { get; set; }
    public List<SaferCompanyV3Email> Emails { get; set; }
    public List<SaferCompanyV3Officer> Officers { get; set; }
    /// <summary> Авторизации из Motus; если DOT в Motus нет — вместо них CensusAuths </summary>
    public List<SaferCompanyV3Auth> Auths { get; set; }
    public List<SaferCompanyV3CensusAuth> CensusAuths { get; set; }
    public SaferCompanyV3Mcs150 Mcs150 { get; set; }
    public SaferCompanyV3Census Census { get; set; }
    public SaferCompanyV3Api Api { get; set; }
}

public class SaferCompanyV3Name {
    public string LegalName { get; set; }
    public string DbaName { get; set; }
}

public class SaferCompanyV3Location {
    /// <summary> 1 - Physical, 2 - Mailing </summary>
    public int Type { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public bool IsUps { get; set; }
    public bool IsMailBox { get; set; }
}

public class SaferCompanyV3Phone {
    /// <summary> 1 - Business, 2 - Cell, 3 - Fax </summary>
    public int Type { get; set; }
    /// <summary> только цифры </summary>
    public string PhoneNumber { get; set; }
}

public class SaferCompanyV3Email {
    public string EmailAddress { get; set; }
}

public class SaferCompanyV3Officer {
    public string FullName { get; set; }
}

public class SaferCompanyV3Auth {
    /// <summary> докет с префиксом, например MC147039 </summary>
    public string DocketNumber { get; set; }
    /// <summary> MotusOpAuthType: 1-7, 12 - Carrier, 8-9 - Broker, 10-11 - Freight Forwarder, 13 - Temporary </summary>
    public int Type { get; set; }
    /// <summary> MotusOpAuthStatus: 1 - Active, 2 - Inactive, 3 - Pending, 4 - Revoked, 5 - Withdrawn, 6 - Suspended </summary>
    public int Status { get; set; }
}

public class SaferCompanyV3CensusAuth {
    public string DocketNumber { get; set; }
    /// <summary> FmcsaAuthorityStatusEnum: 1 - None, 2 - Active, 3 - Inactive </summary>
    public int Common { get; set; }
    public int Contract { get; set; }
    public int Broker { get; set; }
}

public class SaferCompanyV3Mcs150 {
    public DateTime? Mcs150Date { get; set; }
    public long? Mcs150Mileage { get; set; }
    public short? Mcs150MileageYear { get; set; }
}

public class SaferCompanyV3Census {
    /// <summary> FmcsaCarshipEnum, маска: 1 Carrier, 2 Shipper, 4 Broker, 8 Registrant, 16 Freight Forwarder, 32 IEP, 64 Cargo Tank </summary>
    public int Carship { get; set; }
    public SaferCompanyV3CensusDetail Detail { get; set; }
    public SaferCompanyV3CensusDrivers Drivers { get; set; }
    public SaferCompanyV3CensusFleet Fleet { get; set; }
    public SaferCompanyV3CensusMcsip Mcsip { get; set; }
}

public class SaferCompanyV3CensusDetail {
    public int? DunBradstreetNo { get; set; }
}

public class SaferCompanyV3CensusDrivers {
    public int? DriverTotal { get; set; }
}

public class SaferCompanyV3CensusFleet {
    public int? PowerUnits { get; set; }
}

public class SaferCompanyV3CensusMcsip {
    public DateTime? StepDate { get; set; }
}

public class SaferCompanyV3Api {
    public SaferCompanyV3ApiDetail Detail { get; set; }
}

public class SaferCompanyV3ApiDetail {
    public long? DunBradstreetNo { get; set; }
    public int? DriverTotal { get; set; }
    public int? NbrPowerUnit { get; set; }
}

public class SaferModel {
    public bool Success { get; set; }
    public string Error { get; set; }
}

public class SaferModel<T> : SaferModel {
    public T Data { get; set; }
}

/// <summary>
///     Ответ api/v3 → SaferCompanyModel (форма api/v1), чтобы CreateFields и конвертер в CensusCompanyDto не менялись (V1 SaferCompanyV3Converter) </summary>
public static class SaferCompanyV3Converter {

    private class AuthItem {
        public string Docket { get; set; }
        public string Form { get; set; }
        public string Status { get; set; }
    }

    private static readonly string[] AuthStatuses = { null, "Active", "Inactive", "Pending", "Revoked", "Withdrawn", "Suspended" };
    private static readonly string[] CensusAuthStatuses = { null, null, "Active", "Inactive" };
    private static readonly string[] Carships = { "Carrier", "Shipper", "Broker", "Registrant", "Freight Forwarder", "IEP", "Cargo Tank" };

    /// <summary> mc — запрошенный MC: он и попадает в PREFIX/DOCKET_NUMBER, иначе докет выбранной авторизации </summary>
    public static SaferCompanyModel ToModel(SaferCompanyV3Model source, int? mc) {
        var auths = GetAuths(source);
        var auth = auths.FirstOrDefault(e => e.Status == "Active") ?? auths.FirstOrDefault();
        var phy = source.Locations?.FirstOrDefault(e => e.Type == 1);
        var mai = source.Locations?.FirstOrDefault(e => e.Type == 2);
        var officers = source.Officers ?? new List<SaferCompanyV3Officer>();
        var duns = source.Census?.Detail?.DunBradstreetNo ?? source.Api?.Detail?.DunBradstreetNo;

        var model = new SaferCompanyModel {
            CARSHIP = GetCarship(source, auths),
            OperatingStatus = auth == null ? null : $"{auth.Form}: {auth.Status}",
            CommonAuthority = GetFormStatus(auths, "Carrier"),
            BrokerAuthority = GetFormStatus(auths, "Broker"),
            DOT_NUMBER = source.DotNumber,
            NAME = source.Name?.LegalName,
            NAME_DBA = source.Name?.DbaName,
            PHY_TEL_NUM = GetPhone(source, 1),
            PHY_CELL_NUM = GetPhone(source, 2),
            PHY_FAX_NUM = GetPhone(source, 3),
            EMAIL = source.Emails?.FirstOrDefault()?.EmailAddress,
            COMPANY_REP1 = officers.ElementAtOrDefault(0)?.FullName,
            COMPANY_REP2 = officers.ElementAtOrDefault(1)?.FullName,
            PHY_STR = phy?.Street,
            PHY_CITY = phy?.City,
            PHY_ST = phy?.State,
            PHY_ZIP = phy?.Zip,
            MAI_STR = mai?.Street,
            MAI_CITY = mai?.City,
            MAI_ST = mai?.State,
            MAI_ZIP = mai?.Zip,
            TOT_PWR = source.Census?.Fleet?.PowerUnits ?? source.Api?.Detail?.NbrPowerUnit,
            TOT_DRS = source.Census?.Drivers?.DriverTotal ?? source.Api?.Detail?.DriverTotal,
            MLG150 = source.Mcs150?.Mcs150Mileage?.ToString(),
            MCS150MILEAGEYEAR = source.Mcs150?.Mcs150MileageYear,
            MCS_150_DATE = source.Mcs150?.Mcs150Date,
            DBNUM = duns > 0 ? duns.ToString() : null,
            MCSIPDATE = source.Census?.Mcsip?.StepDate,
            Phy_UpsStore = phy?.IsUps,
            Mai_UpsStore = mai?.IsUps,
            Phy_MailBox = phy?.IsMailBox,
            Mai_MailBox = mai?.IsMailBox,
        };
        if (mc != null) {
            model.PREFIX = "MC";
            model.DOCKET_NUMBER = mc;
        } else if (auth?.Docket != null) {
            var prefix = new String(auth.Docket.TakeWhile(Char.IsLetter).ToArray());
            if (Int32.TryParse(auth.Docket.Substring(prefix.Length), out var docket)) {
                model.PREFIX = prefix;
                model.DOCKET_NUMBER = docket;
            }
        }
        return model;
    }

    private static List<AuthItem> GetAuths(SaferCompanyV3Model source) {
        if (source.Auths != null) {
            return source.Auths.Select(e => new AuthItem {
                Docket = e.DocketNumber,
                Form = e.Type == 8 || e.Type == 9 ? "Broker"
                    : e.Type == 10 || e.Type == 11 ? "Freight Forwarder"
                    : e.Type == 13 ? "Temporary"
                    : "Carrier",
                Status = e.Status < AuthStatuses.Length ? AuthStatuses[e.Status] : null,
            }).Where(e => e.Status != null).ToList();
        }
        var result = new List<AuthItem>();
        foreach (var e in source.CensusAuths ?? new List<SaferCompanyV3CensusAuth>()) {
            foreach (var (form, status) in new[] { ("Carrier", e.Common), ("Carrier", e.Contract), ("Broker", e.Broker) }) {
                if (status < CensusAuthStatuses.Length && CensusAuthStatuses[status] != null) {
                    result.Add(new AuthItem { Docket = e.DocketNumber, Form = form, Status = CensusAuthStatuses[status] });
                }
            }
        }
        return result;
    }

    private static string GetFormStatus(List<AuthItem> auths, string form) {
        var items = auths.Where(e => e.Form == form).ToList();
        return (items.FirstOrDefault(e => e.Status == "Active") ?? items.FirstOrDefault())?.Status;
    }

    private static string GetCarship(SaferCompanyV3Model source, List<AuthItem> auths) {
        var carship = source.Census?.Carship ?? 0;
        var names = carship != 0
            ? Carships.Where((name, bit) => (carship & (1 << bit)) != 0)
            : auths.Select(e => e.Form).Where(e => e != "Temporary").Distinct();
        var result = String.Join("/", names);
        return result.Length > 0 ? result : null;
    }

    private static string GetPhone(SaferCompanyV3Model source, int type) {
        var phone = source.Phones?.FirstOrDefault(e => e.Type == type)?.PhoneNumber;
        return phone?.Length == 10 ? $"({phone.Substring(0, 3)}) {phone.Substring(3, 3)}-{phone.Substring(6)}" : phone;
    }
}

#endregion [ api/v3 Safer ]
