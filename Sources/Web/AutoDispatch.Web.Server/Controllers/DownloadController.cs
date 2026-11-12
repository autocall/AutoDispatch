using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Страница Download: установщик AppV3, для всех ролей </summary>
[Authorize]
[Route("Download")]
public class DownloadController : BaseController {

    private DownloadService Service => Host.GetService<DownloadService>();

    #region [ .ctor ]

    public DownloadController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpGet("")]
    public ActionResult Index() {
        return View(Service.Get());
    }
}
