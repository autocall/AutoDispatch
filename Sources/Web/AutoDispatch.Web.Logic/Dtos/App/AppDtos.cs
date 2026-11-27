using System.ComponentModel.DataAnnotations;

namespace AutoDispatch.Web.Logic.Dtos.App;

/// <summary>
///     Ответ Api/Account/SignIn: токен и всё, что клиенту нужно для сессии (замена Api/Token WebV1; роли — для бордов App) </summary>
public class SessionDto {
    public string Token { get; set; }
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public List<string> Roles { get; set; }
    /// <summary> Есть DispatchCompany с BnsfLogin (OAuth V1 hasBnsfRailway) </summary>
    public bool HasBnsfRailway { get; set; }
    /// <summary> Пользовательские статусы компании (OAuth V1 customStatuses, там — JSON словаря Id → Name) </summary>
    public List<AutoDispatch.Dtos.Dtos.CompanyCustomStatusDto> CustomStatuses { get; set; }
    /// <summary> Учётка SIP софтфона (V1 сессия MicroSIPLogin/MicroSIPPassword), как в AutoCall V3 </summary>
    public string SipLogin { get; set; }
    public string SipPassword { get; set; }
}

public class ForgotPasswordDto {
    [Required, EmailAddress]
    public string Email { get; set; }
}
