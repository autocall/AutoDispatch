using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Server.Extensions;

namespace AutoDispatch.Web.Server.Controllers;

public abstract class BaseController : Controller {

    protected readonly ServicesHost Host;
    protected IMapper Mapper => Host.Mapper;

    protected BaseController(ServicesHost host) {
        this.Host = host;
    }

    private ActionResult Result(HttpStatusCode statusCode, string json = null) => new ContentResult {
        StatusCode = (int)statusCode,
        Content = json,
        ContentType = "application/json"
    };

    #region [ Json ]

    protected virtual ActionResult JsonSuccess() {
        return Result(HttpStatusCode.OK);
    }

    protected virtual ActionResult JsonSuccess(object data) {
        return Result(HttpStatusCode.OK, JsonExtension.Serialize(data));
    }

    protected virtual ActionResult JsonFail(string error) {
        return JsonFail(HttpStatusCode.InternalServerError, error);
    }

    protected virtual ActionResult JsonFail(HttpStatusCode statusCode, string error) {
        return Result(statusCode, JsonExtension.Serialize(new { error }));
    }

    protected virtual ActionResult JsonFail(Dictionary<string, string> errors) {
        return Result(HttpStatusCode.BadRequest, JsonExtension.Serialize(new { errors }));
    }

    protected virtual ActionResult JsonFail(IEnumerable<IdentityError> errors) {
        return JsonFail(errors.FirstOrDefault()?.Description ?? "IdentityError");
    }

    #endregion [ Json ]

    #region [ Html / htmx ]

    /// <summary>
    ///     htmx не выполняет 302 для фрагмента — редирект передаётся заголовком HX-Redirect </summary>
    protected ActionResult RedirectAfterPost(string url) {
        if (Request.IsHtmx()) {
            Response.Headers["HX-Redirect"] = url;
            return Ok();
        }
        return LocalRedirect(url);
    }

    #endregion [ Html / htmx ]

    /// <summary>
    ///     ModelState → { FieldName: "error\nerror" } </summary>
    protected Dictionary<string, string> GetErrors() {
        return Mapper.Map<Dictionary<string, string>>(ModelState);
    }

    protected Guid GetUserId() => base.User.GetUserId();
    protected Guid GetCompanyId() => base.User.GetCompanyId();
}
