namespace AutoDispatch.Web.Common.Exceptions;

/// <summary>
///     Пользователь не имеет права на операцию (V1 AccessDeniedException): роль ниже MinAccess или чужая компания; ответ 403 </summary>
public class AccessDeniedException : Exception {
    public AccessDeniedException() : base("Access denied") { }
    public AccessDeniedException(string message) : base(message) { }
}
