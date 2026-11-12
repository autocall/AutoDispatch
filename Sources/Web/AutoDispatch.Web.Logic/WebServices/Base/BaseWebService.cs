using System.Net;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Logic.WebServices;

/// <summary>
///     Внешние HTTP API (DAT, Truckstop, PBX): исключения ловятся внутри и превращаются в ApiResult.Failed </summary>
public abstract class BaseWebService : IService { }

public sealed class ApiResult<T> {

    public bool Success { get; private set; }
    public T Data { get; private set; }
    public string ErrorMessage { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }

    public static ApiResult<T> Successed(T data) {
        return new ApiResult<T> { Success = true, Data = data, StatusCode = HttpStatusCode.OK };
    }

    public static ApiResult<T> Failed(HttpStatusCode statusCode, string error) {
        return new ApiResult<T> { Success = false, Data = default, StatusCode = statusCode, ErrorMessage = error };
    }

    public static ApiResult<T> Failed(Exception e) {
        return Failed(HttpStatusCode.InternalServerError, e.Message);
    }
}
