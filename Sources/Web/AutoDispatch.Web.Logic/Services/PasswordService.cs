using System.Net;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Смена и сброс пароля: сброс — ссылкой на страницу сайта (как в V1), письмо через Mailgun </summary>
public class PasswordService : BaseService {

    private MailgunWebService Mailgun => Host.GetService<MailgunWebService>();

    public PasswordService(ServicesHost host) : base(host) { }

    public async Task ChangeAsync(AcUser user, string currentPassword, string newPassword) {
        var result = await Host.UserManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded == false) {
            throw new InfoException(String.Join("; ", result.Errors.Select(e => e.Description)));
        }
        _l.i($"Password changed: {user.Email}");
    }

    /// <summary>
    ///     Письмо со ссылкой сброса. Наличие пользователя не раскрывается: ответ одинаковый </summary>
    public async Task ForgotAsync(string email) {
        email = email?.Trim().ToLower();
        var user = await Host.UserManager.FindByEmailAsync(email);
        if (user == null || user.IsDeleted) {
            _l.w($"ForgotPassword: unknown email {email}");
            return;
        }
        var code = await Host.UserManager.GeneratePasswordResetTokenAsync(user);
        var url = $"{AppSettings.Website}{AppSettings.ResetPasswordPath}?userId={user.Id}&code={WebUtility.UrlEncode(code)}";
        var html = $"""
            <p>Hello {WebUtility.HtmlEncode(user.UserName)},</p>
            <p>To reset your AutoDispatch password, open the link below:</p>
            <p><a href="{url}">{url}</a></p>
            <p>If you did not request a password reset, ignore this message.</p>
            """;
        var sent = await Mailgun.SendAsync(user.Email, "AutoDispatch — reset password", html);
        if (sent.Success == false) {
            throw new InfoException("Could not send the email, try again later");
        }
        _l.i($"ForgotPassword: email sent to {user.Email}");
    }

    public async Task ResetAsync(int userId, string code, string newPassword) {
        var user = await Host.UserManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted) {
            throw new InfoException("Invalid reset link");
        }
        var result = await Host.UserManager.ResetPasswordAsync(user, code, newPassword);
        if (result.Succeeded == false) {
            throw new InfoException(String.Join("; ", result.Errors.Select(e => e.Description)));
        }
        _l.i($"Password reset: {user.Email}");
    }
}
