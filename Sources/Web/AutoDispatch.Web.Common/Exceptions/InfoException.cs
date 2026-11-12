namespace AutoDispatch.Web.Common.Exceptions;

/// <summary>
///     Ожидаемая ошибка для пользователя: не логируется, текст уходит в ответ как есть </summary>
public class InfoException : Exception {
    public InfoException(string message) : base(message) { }
}
