using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Dtos.Log;

/// <summary>
///     Фильтр /Admin/Logs; теги страницы — Level, Scope/Group, Period, Requests, Live; поля формы — UserName, Text, Session, From, To </summary>
public class LogFilterDto {
    /// <summary>
    ///     Минимальный уровень (Serilog): Information, Warning, Error … </summary>
    public string Level { get; set; }
    /// <summary>
    ///     Точные Scope, любой из списка; пустой — все </summary>
    public List<string> Scope { get; set; } = [];
    /// <summary>
    ///     Группа, когда Scope пуст: app — записи App (app.*), server — остальные </summary>
    public string Group { get; set; }
    /// <summary>
    ///     Подстрока UserName, пробелы по краям не считаются </summary>
    public string UserName { get; set; }
    /// <summary>
    ///     Подстрока в Message или Details </summary>
    public string Text { get; set; }
    /// <summary>
    ///     Id запуска App, точно </summary>
    public string Session { get; set; }
    /// <summary>
    ///     Быстрый период: 15m, 1h, 6h, 24h, 7d; важнее Day </summary>
    public string Period { get; set; }
    /// <summary>
    ///     Период дней по локальному времени сервера из календаря: «yyyy-MM-dd/yyyy-MM-dd» (Cally calendar-range), оба дня включительно;
    ///     в запрос уходит интервал UTC </summary>
    public string Days { get; set; }

    /// <summary>
    ///     Границы периода Days: первый и последний день; null — не задан или не разобран </summary>
    public (DateTime From, DateTime To)? DayRange() {
        var parts = (Days ?? "").Split('/');
        if (DateTime.TryParseExact(parts[0], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var from) == false) {
            return null;
        }
        var to = parts.Length > 1 && DateTime.TryParseExact(parts[1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsed)
            ? parsed : from;
        return to < from ? (to, from) : (from, to);
    }
    /// <summary>
    ///     Показывать записи request logging (по умолчанию скрыты — основной объём) </summary>
    public bool Requests { get; set; }
    /// <summary>
    ///     Курсор «старее»: Id меньше </summary>
    public int? Before { get; set; }
    /// <summary>
    ///     Автообновление страницы </summary>
    public bool Live { get; set; }

    public static readonly string[] Periods = ["15m", "1h", "6h", "24h", "7d"];

    /// <summary>
    ///     Начало интервала UTC: Period от now, иначе полночь первого дня Days по времени сервера </summary>
    public DateTime? EffectiveFrom(DateTime now) {
        return Period switch {
            "15m" => now.AddMinutes(-15),
            "1h" => now.AddHours(-1),
            "6h" => now.AddHours(-6),
            "24h" => now.AddDays(-1),
            "7d" => now.AddDays(-7),
            _ => DayRange() is { } range ? ToUtc(range.From) : null
        };
    }

    /// <summary>
    ///     Конец интервала UTC: полночь после последнего дня Days по времени сервера; при Period не ограничен </summary>
    public DateTime? EffectiveTo() {
        return Period == null && DayRange() is { } range ? ToUtc(range.To.AddDays(1)) : null;
    }

    private static DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZoneInfo.Local);

    public LogFilterDto Clone() {
        var copy = (LogFilterDto)MemberwiseClone();
        copy.Scope = [.. Scope];
        return copy;
    }
}

public class LogStatsDto {
    /// <summary>
    ///     Уровень → количество за последний час </summary>
    public Dictionary<string, int> LastHour { get; set; } = new();
    /// <summary>
    ///     Уровень → количество за последние сутки </summary>
    public Dictionary<string, int> LastDay { get; set; } = new();
}

public class LogTableSizeDto {
    public long Rows { get; set; }
    public double Megabytes { get; set; }
    /// <summary>
    ///     Прирост: строк за последние 24 часа и в среднем за день за 7 дней </summary>
    public int LastDayRows { get; set; }
    public int DayAverageRows { get; set; }
}

public class DbPingDto {
    public string Name { get; set; }
    public bool Ok { get; set; }
    public double Milliseconds { get; set; }
    public string Error { get; set; }
}

public class OnlineUserDto {
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public string Email { get; set; }
    public LoginSourceEnum Source { get; set; }
    public DateTime LastSeen { get; set; }
}
