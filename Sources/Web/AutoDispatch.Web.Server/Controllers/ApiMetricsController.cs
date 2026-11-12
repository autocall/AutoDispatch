using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Отчёт App о своём состоянии раз в 30 с: в минутные метрики, пользователь — из токена </summary>
[Route("Api/Metrics")]
public class ApiMetricsController : ApiBaseController {

    private MetricService Service => Host.GetService<MetricService>();

    public ApiMetricsController(ServicesHost host) : base(host) { }

    [HttpPost]
    public ActionResult Report([FromBody] ClientMetricsDto dto) {
        if (dto == null) {
            return JsonFail(HttpStatusCode.BadRequest, "Empty report");
        }
        Service.AddClientReport(GetUserId(), dto);
        return JsonSuccess();
    }

    /// <summary>
    ///     Скрипт на сервере БД (Deploy/metric_cpu.sh, как Metric/Add в Safer): {"type":2,"value":N},
    ///     заголовок Authorization: Bearer Secrets:Metric:Token — не JWT </summary>
    [HttpPost("Add"), AllowAnonymous]
    public ActionResult Add([FromBody] ServerMetricDto dto) {
        if (Request.Headers.Authorization.ToString() != $"Bearer {AppSettings.MetricToken}") {
            return JsonFail(HttpStatusCode.Unauthorized, "Invalid bearer token");
        }
        if (dto == null || dto.Type == MetricTypeEnum.Unknown || Enum.IsDefined(dto.Type) == false) {
            return JsonFail(HttpStatusCode.BadRequest, "Unknown metric type");
        }
        Service.Add(dto.Type, dto.Value);
        return JsonSuccess();
    }
}
