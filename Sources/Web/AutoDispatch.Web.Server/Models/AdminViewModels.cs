using System.ComponentModel.DataAnnotations;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Dtos;
using AutoDispatch.Web.Logic.Dtos.Identity;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     Форма компании в модалке: Id = 0 — создание </summary>
public class CompanyFormViewModel {
    public int Id { get; set; }
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; }
}

public class CompaniesIndexViewModel {
    public bool Deleted { get; set; }
    public string Name { get; set; }
    public List<CompanyDto> Companies { get; set; }

    /// <summary>
    ///     Тег Active/Deleted с текущим фильтром по названию </summary>
    public string Link(bool deleted) {
        var name = String.IsNullOrEmpty(Name) ? "" : $"&name={Uri.EscapeDataString(Name)}";
        return $"/Admin/Companies?deleted={deleted.ToString().ToLower()}{name}";
    }
}

/// <summary>
///     Форма пользователя в модалке: Id = 0 — создание, тогда пароль обязателен </summary>
public class UserFormViewModel {
    public int Id { get; set; }
    [Required]
    public int CompanyId { get; set; }
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; }
    [MaxLength(256)]
    public string UserName { get; set; }
    [MaxLength(64)]
    public string TimeZoneId { get; set; }
    [Required]
    public RoleEnum Role { get; set; } = RoleEnum.Dispatcher;
    [DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; }

    public List<CompanyDto> Companies { get; set; }
    public List<string> TimeZones { get; set; }
}

/// <summary>
///     /Admin/Users: теги фильтра — ссылки с текущим фильтром (Link) </summary>
public class UsersIndexViewModel {
    public UserFilterDto Filter { get; set; }
    public List<CompanyDto> Companies { get; set; }
    public List<UserDto> Users { get; set; }

    /// <summary>
    ///     Адрес страницы с текущим фильтром после изменения </summary>
    public string Link(Action<UserFilterDto> change) {
        var filter = Filter.Clone();
        change(filter);
        var query = new List<string>();
        void Add(string name, string value) {
            if (String.IsNullOrEmpty(value) == false) {
                query.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }
        Add("CompanyId", filter.CompanyId?.ToString());
        Add("Deleted", filter.Deleted ? "true" : null);
        Add("Email", filter.Email);
        return query.Count == 0 ? "/Admin/Users" : "/Admin/Users?" + String.Join("&", query);
    }
}

/// <summary>
///     /Admin/Logs: теги — ссылки с текущим фильтром и одним изменённым параметром (Link), подписи Scope — ScopeLabel </summary>
public class LogsIndexViewModel {
    public AutoDispatch.Web.Logic.Dtos.Log.LogFilterDto Filter { get; set; }
    /// <summary>
    ///     Scope за последние 7 дней — теги источников </summary>
    public List<string> Scopes { get; set; }
    public AutoDispatch.Web.Logic.Dtos.Log.LogStatsDto Stats { get; set; }
    public List<AutoDispatch.Web.Data.Entities.Log.LogEntry> Entries { get; set; }

    private static readonly Dictionary<string, string> ScopeLabels = new() {
        ["app.session"] = "Session", ["app.ui"] = "UI", ["auth"] = "Auth", ["admin"] = "Admin", ["map"] = "Map"
    };

    public static string ScopeLabel(string scope) => ScopeLabels.GetValueOrDefault(scope, scope);

    /// <summary>
    ///     Адрес страницы с текущим фильтром после изменения; Before и Live сбрасываются — они про конкретную выборку </summary>
    public string Link(Action<AutoDispatch.Web.Logic.Dtos.Log.LogFilterDto> change, bool keepPage = false) {
        var filter = Filter.Clone();
        if (keepPage == false) {
            filter.Before = null;
            filter.Live = false;
        }
        change(filter);
        var query = new List<string>();
        void Add(string name, string value) {
            if (String.IsNullOrEmpty(value) == false) {
                query.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }
        Add("Level", filter.Level);
        filter.Scope.ForEach(scope => Add("Scope", scope));
        Add("Group", filter.Group);
        Add("UserName", filter.UserName);
        Add("Text", filter.Text);
        Add("Session", filter.Session);
        Add("Period", filter.Period);
        Add("Days", filter.Days);
        Add("Requests", filter.Requests ? "true" : null);
        Add("Before", filter.Before?.ToString());
        Add("Live", filter.Live ? "true" : null);
        return query.Count == 0 ? "/Admin/Logs" : "/Admin/Logs?" + String.Join("&", query);
    }
}

public class MonitorIndexViewModel {
    public string Version { get; set; }
    public string Environment { get; set; }
    public DateTime StartedAt { get; set; }
    public List<AutoDispatch.Web.Logic.Dtos.Log.DbPingDto> Databases { get; set; }
    public AutoDispatch.Web.Logic.Dtos.Log.LogTableSizeDto LogTable { get; set; }
    public List<AutoDispatch.Web.Logic.Dtos.Log.OnlineUserDto> Online { get; set; }
    public AutoDispatch.Web.Logic.Dtos.Log.LogStatsDto LogStats { get; set; }
    public List<AutoDispatch.Web.Data.Entities.Log.UserLogin> RecentLogins { get; set; }
}

/// <summary>
///     /Admin/Metrics: строки уровня Level; Day — выбранный локальный день (часы), Hour — выбранный час UTC (минуты);
///     без них — последние периоды </summary>
public class MetricsIndexViewModel {
    public AutoDispatch.Web.Logic.Monitoring.MetricLevelEnum Level { get; set; }
    public DateTime? Day { get; set; }
    public DateTime? Hour { get; set; }
    public List<AutoDispatch.Web.Logic.Monitoring.MetricRowDto> Rows { get; set; }

    /// <summary>
    ///     Цвет ступени шкалы (MetricConverter.GetStep) — цвета темы daisyUI. Классы целиком строками:
    ///     сборка CSS находит их в этом файле (Client/app.css, @source Models) </summary>
    private static readonly string[] StepClasses = ["bg-success/30", "bg-warning/40", "bg-error/40"];

    /// <summary>
    ///     Фон ячейки по шкале метрики (CPU, задержка geocoding) — по максимуму за период (StatusValues);
    ///     "" — нет данных или шкалы </summary>
    public string StepClass(AutoDispatch.Web.Logic.Monitoring.MetricRowDto row, MetricTypeEnum type) =>
        AutoDispatch.Web.Logic.Monitoring.MetricConverter.GetStep(type, row.StatusValues.TryGetValue(type, out var value) ? value : null)
            is { } step ? StepClasses[step] : "";

    /// <summary>
    ///     Границы шкалы для подсказки заголовка: "70 / 90 %"; format — как показать одну границу </summary>
    public string StepsHint(MetricTypeEnum type, Func<int, string> format, string unit) =>
        AutoDispatch.Web.Logic.Monitoring.MetricConverter.GetSteps(type) is { } steps ? $"{String.Join(" / ", steps.Select(format))} {unit}" : "";

    /// <summary>
    ///     Ячейки пользователей и звонков: зелёная, если есть хоть одно значение (отчёт был, ноль тоже норма) </summary>
    public string ActiveClass(params int?[] values) => values.Any(e => e != null) ? "bg-success/25" : "";

    /// <summary>
    ///     Миллисекунды → секунды с двумя знаками; "" — нет данных </summary>
    public string Seconds(int? milliseconds) =>
        milliseconds is { } ms ? (ms / 1000.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : "";
}
