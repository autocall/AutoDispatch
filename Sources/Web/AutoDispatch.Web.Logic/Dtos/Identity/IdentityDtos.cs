using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Dtos.Identity;

public class AcUserDto {
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public bool IsDeleted { get; set; }
}

public class AcRoleDto {
    public int Id { get; set; }
    public string Name { get; set; }
}

/// <summary>
///     Ответ Api/Account/Identity и SignIn </summary>
public class IdentityDto {
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public List<string> Roles { get; set; }
}

/// <summary>
///     Строка списка пользователей в админке: одна роль на пользователя </summary>
public class UserDto {
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public RoleEnum Role { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public bool IsDeleted { get; set; }
    /// <summary>
    ///     Версия AutoDispatch App последнего входа из App; null — не входил или клиент ещё не передаёт версию </summary>
    public string ClientVersion { get; set; }
}

/// <summary>
///     Фильтр /Admin/Users </summary>
public class UserFilterDto {
    public int? CompanyId { get; set; }
    public bool Deleted { get; set; }
    public string Email { get; set; }

    public UserFilterDto Clone() => (UserFilterDto)MemberwiseClone();
}

public class CreateUserDto {
    public int CompanyId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public RoleEnum Role { get; set; }
    public string Password { get; set; }
}

public class UpdateUserDto {
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string TimeZoneId { get; set; }
    public RoleEnum Role { get; set; }
    /// <summary>
    ///     Пусто — пароль не меняется </summary>
    public string Password { get; set; }
}
