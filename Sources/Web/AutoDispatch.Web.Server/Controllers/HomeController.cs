using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.Controllers;

[Authorize]
public class HomeController : BaseController {

    private UserService UserService => Host.GetService<UserService>();

    public HomeController(ServicesHost host) : base(host) { }

    public async Task<ActionResult> Index() {
        var user = await Host.UserManager.GetUserAsync(User);
        return View(await UserService.GetIdentityAsync(user));
    }
}
