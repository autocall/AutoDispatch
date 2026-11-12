using System.ComponentModel.DataAnnotations;

namespace AutoDispatch.Web.Logic.Dtos.App;

/// <summary>
///     Ответ Api/Account/SignIn: токен и всё, что клиенту нужно для сессии (замена Api/Token WebV1; роли — для бордов App) </summary>
public class SessionDto {
    public string Token { get; set; }
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public List<string> Roles { get; set; }
}

public class ChangePasswordDto {
    [Required]
    public string CurrentPassword { get; set; }
    [Required, MinLength(6)]
    public string NewPassword { get; set; }
}

public class ForgotPasswordDto {
    [Required, EmailAddress]
    public string Email { get; set; }
}
