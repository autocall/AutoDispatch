using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Метрики (как _MetricPart в Safer) по уровням дни → часы → минуты: работает ли система — сервер, сорсы, телефония </summary>
[Authorize(Roles = nameof(RoleEnum.Admin))]
[Route("Admin/Metrics/[action]")]
public class AdminMetricsController : BaseController {

    private MetricService Service => Host.GetService<MetricService>();

    public AdminMetricsController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     level — days | hours | minutes (по умолчанию minutes); day — "yyyy-MM-dd", локальный день сервера (для hours);
    ///     hour — "yyyy-MM-ddTHH" UTC (для minutes). Без day/hour — последние периоды </summary>
    [HttpGet("~/Admin/Metrics")]
    public async Task<ActionResult> Index(string level = null, string day = null, string hour = null) {
        var metricLevel = level?.ToLowerInvariant() switch {
            "days" => MetricLevelEnum.Days,
            "hours" => MetricLevelEnum.Hours,
            _ => MetricLevelEnum.Minutes
        };
        DateTime? selectedDay = metricLevel == MetricLevelEnum.Hours
            && DateTime.TryParseExact(day, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDay)
            ? parsedDay.Date : null;
        DateTime? selectedHour = metricLevel == MetricLevelEnum.Minutes
            && DateTime.TryParseExact(hour, "yyyy-MM-ddTHH", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsedHour)
            ? DateTime.SpecifyKind(parsedHour, DateTimeKind.Utc) : null;

        return View(new MetricsIndexViewModel {
            Level = metricLevel,
            Day = selectedDay,
            Hour = selectedHour,
            Rows = await Service.GetRowsAsync(metricLevel, selectedDay, selectedHour)
        });
    }
}
