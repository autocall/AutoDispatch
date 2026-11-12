using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Server.Infrastructure;

/// <summary>
///     Любое исключение → 400 { error }; InfoException не логируется </summary>
public class ExceptionHandler : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken) {
        if (exception is InfoException == false) {
            _l.e("ExceptionHandler", exception);
        }
        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";
        var result = JsonExtension.Serialize(new { error = exception.GetBaseException().Message });
        await httpContext.Response.WriteAsync(result, cancellationToken);
        return true;
    }
}
