namespace AutoDispatch.Web.Data.Enums;

/// <summary>
///     Откуда пришёл вход или запрос: сайт (cookie) или приложение (JWT) </summary>
public enum LoginSourceEnum : byte {
    Unknown = 0,
    Web = 1,
    App = 2
}
