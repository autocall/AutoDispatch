using System.ComponentModel.DataAnnotations;

namespace AutoDispatch.Web.Server.Models;

public class SignInViewModel {
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}

/// <summary>
///     Форма входа на сайт (cookie) </summary>
public class LogInViewModel {
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required, DataType(DataType.Password)]
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string ReturnUrl { get; set; }
    /// <summary>
    ///     Сообщение об ошибке входа, показывается в форме </summary>
    public string Error { get; set; }
}

public class ForgotPasswordViewModel {
    [Required, EmailAddress]
    public string Email { get; set; }
    /// <summary>
    ///     Текст после отправки: форма заменяется сообщением </summary>
    public string Message { get; set; }
    public string Error { get; set; }
}

/// <summary>
///     Смена пароля на Home (Profile/ChangePassword WebV1) </summary>
public class ChangePasswordViewModel {
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; }
    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; }
    [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    public string Message { get; set; }
    public string Error { get; set; }
}

public class ResetPasswordViewModel {
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Code { get; set; }
    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; }
    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    public string Error { get; set; }
}
