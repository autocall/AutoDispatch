using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Логи App пачками в таблицу Log: пользователь — из токена. Сверх квоты — 200 без записи, чтобы App не повторял отправку </summary>
[Route("Api/Logs")]
public class ApiLogsController : ApiBaseController {

    private LogService Service => Host.GetService<LogService>();

    public ApiLogsController(ServicesHost host) : base(host) { }

    [HttpPost, RequestSizeLimit(1024 * 1024)]
    public ActionResult Report([FromBody] ClientLogBatchDto dto) {
        if (dto?.Entries == null) {
            return JsonFail(System.Net.HttpStatusCode.BadRequest, "Empty batch");
        }
        Service.WriteClient(GetUserId(), dto);
        return JsonSuccess();
    }
}
