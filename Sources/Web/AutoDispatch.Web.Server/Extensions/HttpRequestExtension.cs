using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Server.Extensions;

public static class HttpRequestExtension {

    /// <summary>
    ///     Запрос пришёл от htmx (заголовок HX-Request): отвечать фрагментом, редирект — через HX-Redirect </summary>
    public static bool IsHtmx(this HttpRequest request) {
        return request.Headers.ContainsKey("HX-Request");
    }

    /// <summary>
    ///     App ходит с Bearer-токеном, сайт — с cookie </summary>
    public static LoginSourceEnum GetLoginSource(this HttpRequest request) {
        var authorization = request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? LoginSourceEnum.App : LoginSourceEnum.Web;
    }

    /// <summary>
    ///     IP клиента; за nginx берётся из X-Forwarded-For (UseForwardedHeaders) </summary>
    public static string GetIp(this HttpRequest request) {
        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    public static string GetUserAgent(this HttpRequest request) {
        return request.Headers.UserAgent.ToString().Truncate(256);
    }
}
