using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Logic.WebServices.CarrierView;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Webhook CarrierView (V1 DispatchController.UpdateLocation): тот же путь /Dispatch/UpdateLocation — при переезде в настройках
///     CarrierView меняется только домен. Без логина, как в V1; ответ как в V1 — 200 {Success[, Error]} (AI/wiki/app-testing.md 6.14) </summary>
[AllowAnonymous]
public class CarrierViewController : BaseController {

    private DispatchService DispatchService => Host.GetService<DispatchService>();

    public CarrierViewController(ServicesHost host) : base(host) { }

    [HttpPost("~/Dispatch/UpdateLocation")]
    public async Task<ActionResult> UpdateLocation([FromBody] CarrierViewUpdateLocationRequest model) {
        try {
            await DispatchService.CarrierViewUpdateLocationAsync(model);
            return JsonSuccess(new { Success = true });
        } catch (Exception e) {
            _l.e("UpdateLocation", e);
            return JsonSuccess(new { Success = false, Error = e.Message });
        }
    }
}
