using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;
using AutoDispatch.Web.Server.Extensions;
using AutoDispatch.Web.Server.Models;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Вход на сайт по cookie (Identity). App входит через ApiAccountController и JWT </summary>
public class AccountController : BaseController {

    private readonly SignInManager<AdUser> _signInManager;
    private LoginService LoginService => Host.GetService<LoginService>();
    private PasswordService PasswordService => Host.GetService<PasswordService>();

    #region [ .ctor ]

    public AccountController(ServicesHost host, SignInManager<AdUser> signInManager) : base(host) {
        this._signInManager = signInManager;
    }

    #endregion [ .ctor ]

    [HttpGet, AllowAnonymous]
    public ActionResult LogIn(string returnUrl = null) {
        if (User.Identity?.IsAuthenticated == true) {
            return LocalRedirect(SafeReturnUrl(returnUrl));
        }
        return View(new LogInViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<ActionResult> LogIn(LogInViewModel model) {
        using var scope = _l.Scope("auth");
        if (ModelState.IsValid) {
            var email = model.Email.Trim().ToLower();
            var user = await Host.UserManager.FindByEmailAsync(email);
            if (user != null && user.IsDeleted == false) {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded) {
                    _l.i($"LogIn: {email}");
                    await LoginService.RecordAsync(email, user, true, LoginSourceEnum.Web, Request.GetIp(), Request.GetUserAgent());
                    return RedirectAfterPost(SafeReturnUrl(model.ReturnUrl));
                }
            }
            _l.w($"LogIn failed: {email}");
            await LoginService.RecordAsync(email, user, false, LoginSourceEnum.Web, Request.GetIp(), Request.GetUserAgent());
            model.Error = "Invalid email or password";
        }
        model.Password = null;
        return Request.IsHtmx() ? PartialView("_LogInFormPart", model) : View(model);
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<ActionResult> LogOut() {
        using var scope = _l.Scope("auth");
        _l.i($"LogOut: {User.Identity?.Name}");
        await _signInManager.SignOutAsync();
        return RedirectAfterPost(AppSettings.LogInPath);
    }

    #region [ password reset ]

    [HttpGet, AllowAnonymous]
    public ActionResult ForgotPassword() {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model) {
        if (ModelState.IsValid) {
            using var scope = _l.Scope("auth");
            try {
                await PasswordService.ForgotAsync(model.Email);
                model.Message = "If the email is registered, a reset link has been sent. Check your inbox.";
            } catch (InfoException e) {
                model.Error = e.Message;
            }
        }
        return Request.IsHtmx() ? PartialView("_ForgotPasswordFormPart", model) : View(model);
    }

    [HttpGet, AllowAnonymous]
    public ActionResult ResetPassword(Guid userId, string code) {
        if (userId == Guid.Empty || String.IsNullOrEmpty(code)) {
            return View(new ResetPasswordViewModel { Error = "Invalid reset link" });
        }
        return View(new ResetPasswordViewModel { UserId = userId, Code = code });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model) {
        if (ModelState.IsValid) {
            using var scope = _l.Scope("auth");
            try {
                await PasswordService.ResetAsync(model.UserId, model.Code, model.Password);
                return RedirectAfterPost(AppSettings.LogInPath);
            } catch (InfoException e) {
                model.Error = e.Message;
            }
        }
        model.Password = null;
        model.ConfirmPassword = null;
        return Request.IsHtmx() ? PartialView("_ResetPasswordFormPart", model) : View(model);
    }

    #endregion [ password reset ]

    /// <summary>
    ///     Модалка с кнопки на Home </summary>
    [HttpGet, Authorize]
    public ActionResult ChangePassword() {
        return PartialView("_ChangePasswordFormPart", new ChangePasswordViewModel());
    }

    /// <summary>
    ///     Успех — сообщение в той же модалке, ошибка — форма остаётся открытой с текстом ошибки </summary>
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model) {
        if (ModelState.IsValid) {
            using var scope = _l.Scope("auth");
            var user = await Host.UserManager.GetUserAsync(User);
            try {
                await PasswordService.ChangeAsync(user, model.CurrentPassword, model.NewPassword);
                // смена пароля меняет SecurityStamp — без обновления cookie сайт разлогинит при следующей проверке
                await _signInManager.RefreshSignInAsync(user);
                model.Message = "Your password has been changed";
            } catch (InfoException e) {
                model.Error = e.Message;
            }
        }
        return PartialView("_ChangePasswordFormPart", model);
    }

    #region [ private ]

    private string SafeReturnUrl(string returnUrl) {
        return String.IsNullOrEmpty(returnUrl) == false && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
    }

    #endregion [ private ]
}
