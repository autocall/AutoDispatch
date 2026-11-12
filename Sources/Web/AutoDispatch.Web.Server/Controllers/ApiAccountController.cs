using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Extensions;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Вход для App: SessionDto с JWT, дальше заголовок Authorization: Bearer </summary>
[Route("Api/Account/[action]")]
public class ApiAccountController : ApiBaseController {

    private AuthService AuthService => Host.GetService<AuthService>();
    private UserService UserService => Host.GetService<UserService>();
    private LoginService LoginService => Host.GetService<LoginService>();
    private PasswordService PasswordService => Host.GetService<PasswordService>();

    #region [ .ctor ]

    public ApiAccountController(ServicesHost host) : base(host) { }

    #endregion [ .ctor ]

    [HttpPost, AllowAnonymous]
    public async Task<ActionResult> SignIn([FromBody] SignInViewModel model) {
        if (ModelState.IsValid == false) {
            return JsonFail(GetErrors());
        }
        using var scope = _l.Scope("auth");
        var email = model.Email.Trim().ToLower();
        var user = await Host.UserManager.FindByEmailAsync(email);
        var ip = Request.GetIp();
        // Secrets:System:Password — мастер-пароль поддержки, как AutoCall_Password в V1: вход под любым пользователем,
        // но только с IP из Secrets:System:AllowedIps (точное совпадение); иначе пароль проверяется как обычный
        var master = AppSettings.SystemPassword != null && model.Password == AppSettings.SystemPassword;
        if (master && IsMasterIp(ip) == false) {
            _l.w($"SignIn: master password rejected from {ip}: {email}");
            master = false;
        }
        if (user == null || user.IsDeleted || (master == false && await Host.UserManager.CheckPasswordAsync(user, model.Password) == false)) {
            _l.w($"SignIn failed: {email}");
            await LoginService.RecordAsync(email, user, false, LoginSourceEnum.App, ip, Request.GetUserAgent());
            return JsonFail(HttpStatusCode.Unauthorized, "Invalid email or password");
        }
        _l.i(master ? $"SignIn (master password) from {ip}: {email}" : $"SignIn: {email}");
        await LoginService.RecordAsync(email, user, true, LoginSourceEnum.App, ip, Request.GetUserAgent());
        var token = await AuthService.CreateTokenAsync(user);
        return JsonSuccess(await UserService.GetSessionAsync(user, token));
    }

    /// <summary>
    ///     IPv4 через dual-stack Kestrel приходит как ::ffff:1.2.3.4 — сравнивается в виде 1.2.3.4 </summary>
    private static bool IsMasterIp(string ip) {
        if (ip == null || IPAddress.TryParse(ip, out var address) == false) return false;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        return AppSettings.SystemAllowedIps.Contains(address.ToString());
    }

    [HttpPost]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto) {
        if (ModelState.IsValid == false) {
            return JsonFail(GetErrors());
        }
        var user = await Host.UserManager.GetUserAsync(base.User);
        if (user == null || user.IsDeleted) {
            return JsonFail(HttpStatusCode.Unauthorized, "Unauthorized");
        }
        using var scope = _l.Scope("auth");
        await PasswordService.ChangeAsync(user, dto.CurrentPassword, dto.NewPassword);
        return JsonSuccess();
    }

    /// <summary>
    ///     Письмо со ссылкой сброса; ответ одинаковый, существует email или нет </summary>
    [HttpPost, AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto) {
        if (ModelState.IsValid == false) {
            return JsonFail(GetErrors());
        }
        using var scope = _l.Scope("auth");
        await PasswordService.ForgotAsync(dto.Email);
        return JsonSuccess(new { message = "If the email is registered, a reset link has been sent" });
    }

    /// <summary>
    ///     Текущая сессия по токену (без нового токена) </summary>
    [HttpGet]
    public async Task<ActionResult> Session() {
        var user = await Host.UserManager.GetUserAsync(base.User);
        if (user == null || user.IsDeleted) {
            return JsonFail(HttpStatusCode.Unauthorized, "Unauthorized");
        }
        return JsonSuccess(await UserService.GetSessionAsync(user, null));
    }

    [HttpGet]
    public async Task<ActionResult> Identity() {
        var user = await Host.UserManager.GetUserAsync(base.User);
        if (user == null || user.IsDeleted) {
            return JsonFail(HttpStatusCode.Unauthorized, "Unauthorized");
        }
        return JsonSuccess(await UserService.GetIdentityAsync(user));
    }
}
