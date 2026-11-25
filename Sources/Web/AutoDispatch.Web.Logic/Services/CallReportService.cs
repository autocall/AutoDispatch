using System.Globalization;
using System.Text.RegularExpressions;
using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic.Dtos.CallReport;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Call Report и Phone channel congestion по CDR Vodia за день (перенос V1 VodiaLogService, копия AutoCall V3: софтфоны App звонят через ту же PBX).
///     Owner видит звонки extension своей компании, Admin — все </summary>
public class CallReportService : BaseService {

    /// <summary>
    ///     Extension в поле remote: «sip:…:0341162@…» </summary>
    private static readonly Regex RemoteSipLogin = new Regex(@":(?<ext>\d+)@", RegexOptions.Compiled);

    private VodiaWebService Vodia => Host.GetService<VodiaWebService>();

    public CallReportService(ServicesHost host) : base(host) { }

    public async Task<CongestionDto> GetCongestionAsync(CallReportFilterDto filter, Guid companyId, bool isAdmin) {
        var day = await LoadDayAsync(filter, companyId, isAdmin);
        if (day.Error != null) return new CongestionDto { Error = day.Error };
        var users = day.Calls.Select(e => e.SipLogin).Distinct().Order()
            .Select(e => new CallReportUserDto { SipLogin = e, UserName = day.UserName(e) })
            .ToList();
        var calls = users.Any(e => e.SipLogin == filter.User) ? day.Calls.Where(e => e.SipLogin == filter.User).ToList() : day.Calls;
        return new CongestionDto {
            Periods = BuildPeriods(calls, AppSettings.CongestionTopPeriods),
            Users = users,
            Companies = day.Companies,
            Calls = calls.Count,
            Duration = (int)calls.Sum(e => (e.End - e.Start).TotalSeconds)
        };
    }

    public async Task<CallReportDto> GetReportAsync(CallReportFilterDto filter, Guid companyId, bool isAdmin) {
        var day = await LoadDayAsync(filter, companyId, isAdmin);
        if (day.Error != null) return new CallReportDto { Error = day.Error };
        return new CallReportDto {
            Rows = day.Calls
                .GroupBy(e => e.SipLogin)
                .Select(g => new CallReportRowDto {
                    SipLogin = g.Key,
                    UserName = day.UserName(g.Key),
                    CompanyName = day.CompanyName(g.Key),
                    Calls = g.Count(),
                    Duration = (int)g.Sum(e => (e.End - e.Start).TotalSeconds)
                })
                .OrderByDescending(e => e.Duration)
                .ToList(),
            Companies = day.Companies
        };
    }

    /// <summary>
    ///     Звонки дня через trunk. Поля CDR: 5 dir (I/O), 7 start, 9 end (yyyyMMddHHmmss), 10 trunk, 11 local (extension), 12 remote.
    ///     visible — extension компании (звонок виден, если она в local или в remote); null — все </summary>
    public static List<CdrCall> ParseCalls(IEnumerable<string> lines, DateTime day, bool onlyOut, HashSet<string> visible) {
        var calls = new List<CdrCall>();
        foreach (var line in lines) {
            // запятая внутри кавычек даёт больше 19 полей — строка пропускается, как в V1
            var arr = line.Split(',');
            if (arr.Length != 19) continue;
            if (visible != null && visible.Contains(arr[11]) == false) {
                var match = RemoteSipLogin.Match(arr[12]);
                if (match.Success == false || visible.Contains(match.Groups["ext"].Value) == false) continue;
            }
            if (String.IsNullOrEmpty(arr[10])) continue;
            if (onlyOut && arr[5] != "O") continue;
            if (TryParseTime(arr[7], out var start) == false || TryParseTime(arr[9], out var end) == false) continue;
            // звонок целиком внутри дня
            if (start < day || end >= day.AddDays(1)) continue;
            calls.Add(new CdrCall { SipLogin = arr[11], Start = start, End = end });
        }
        return calls;
    }

    /// <summary>
    ///     Интервалы с постоянным числом одновременных звонков (соседние с равным числом слиты):
    ///     top самых загруженных (при равенстве — длинных), по времени от поздних к ранним </summary>
    public static List<CongestionPeriodDto> BuildPeriods(List<CdrCall> calls, int top) {
        // в начале звонка +1, в конце −1; между соседними точками число звонков постоянно
        var points = calls
            .Where(e => e.End > e.Start)
            .SelectMany(e => new[] { (Time: e.Start, Delta: 1), (Time: e.End, Delta: -1) })
            .GroupBy(e => e.Time)
            .OrderBy(g => g.Key)
            .Select(g => (Time: g.Key, Delta: g.Sum(e => e.Delta)))
            .ToList();
        var periods = new List<CongestionPeriodDto>();
        var active = 0;
        for (var i = 0; i < points.Count - 1; i++) {
            active += points[i].Delta;
            if (active == 0) continue;
            var last = periods.LastOrDefault();
            if (last != null && last.End == points[i].Time && last.Calls == active) {
                last.End = points[i + 1].Time;
            } else {
                periods.Add(new CongestionPeriodDto { Start = points[i].Time, End = points[i + 1].Time, Calls = active });
            }
        }
        return periods
            .OrderByDescending(e => e.Calls)
            .ThenByDescending(e => e.End - e.Start)
            .Take(top)
            .OrderByDescending(e => e.Start)
            .ToList();
    }

    private static bool TryParseTime(string value, out DateTime time) {
        return DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time);
    }

    /// <summary>
    ///     CDR дня с отбором по компании пользователя и по фильтру Company </summary>
    private async Task<CdrDay> LoadDayAsync(CallReportFilterDto filter, Guid companyId, bool isAdmin) {
        var cdr = await Vodia.GetCdrAsync(filter.Day);
        if (cdr.Success == false) return new CdrDay { Error = cdr.ErrorMessage };

        var db = UnitOfWork.Context;
        var sipUsers = await db.GetTable<AdUser>()
            .Where(e => e.SipLogin != null && e.SipLogin != "")
            .Select(e => new SipUser { SipLogin = e.SipLogin, UserName = e.UserName, CompanyId = e.CompanyId, IsDeleted = e.IsDeleted })
            .ToListAsync();
        var names = (await db.GetTable<Company>().Select(e => new { e.Id, e.Name }).ToListAsync()).ToDictionary(e => e.Id, e => e.Name);
        // extension могли передать новому пользователю — действующий важнее удалённого
        var users = sipUsers.GroupBy(e => e.SipLogin).ToDictionary(g => g.Key, g => g.OrderBy(e => e.IsDeleted).First());
        var visible = isAdmin ? null : sipUsers.Where(e => e.CompanyId == companyId).Select(e => e.SipLogin).ToHashSet();

        var day = new CdrDay { Users = users, CompanyNames = names };
        day.Calls = ParseCalls(cdr.Data, filter.Day, filter.OnlyOut, visible);
        day.Companies = day.Calls
            .Select(e => day.CompanyId(e.SipLogin))
            .OfType<Guid>()
            .Distinct()
            .Select(e => new CallReportCompanyDto { Id = e, Name = names.GetValueOrDefault(e) })
            .OrderBy(e => e.Name)
            .ToList();
        if (day.Companies.Any(e => e.Id == filter.CompanyId)) {
            day.Calls = day.Calls.Where(e => day.CompanyId(e.SipLogin) == filter.CompanyId).ToList();
        }
        return day;
    }

    private class SipUser {
        public string SipLogin { get; set; }
        public string UserName { get; set; }
        public Guid CompanyId { get; set; }
        public bool IsDeleted { get; set; }
    }

    private class CdrDay {
        public List<CdrCall> Calls { get; set; } = [];
        public List<CallReportCompanyDto> Companies { get; set; } = [];
        public Dictionary<string, SipUser> Users { get; set; } = [];
        public Dictionary<Guid, string> CompanyNames { get; set; } = [];
        public string Error { get; set; }

        public Guid? CompanyId(string sipLogin) => this.Users.TryGetValue(sipLogin, out var user) ? user.CompanyId : null;
        public string UserName(string sipLogin) => this.Users.TryGetValue(sipLogin, out var user) ? user.UserName : sipLogin;
        public string CompanyName(string sipLogin) => this.CompanyId(sipLogin) is { } id ? this.CompanyNames.GetValueOrDefault(id) : null;
    }
}
