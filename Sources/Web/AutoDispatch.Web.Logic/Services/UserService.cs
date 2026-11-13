using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Data.Helpers;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Dtos.Identity;
using AutoDispatch.Web.Logic.Helpers;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Пользователи через UserManager (Identity сам пишет через EF); списки — LinqToDB </summary>
public class UserService : BaseService {

    public UserService(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Список админки: Deleted — только удалённые, иначе только активные; Email — подстрока; версия клиента — из БД логов </summary>
    public async Task<List<UserDto>> GetAllAsync(UserFilterDto filter) {
        var db = UnitOfWork.Context;
        var email = String.IsNullOrWhiteSpace(filter.Email) ? null : filter.Email.Trim();
        var query =
            from u in db.GetTable<AdUser>()
            join c in db.GetTable<Company>() on u.CompanyId equals c.Id
            join ur in db.GetTable<AdUserRole>() on u.Id equals ur.UserId into urs
            from ur in urs.DefaultIfEmpty()
            where u.IsDeleted == filter.Deleted && (filter.CompanyId == null || u.CompanyId == filter.CompanyId)
                && (email == null || u.Email.Contains(email))
            orderby c.Name, u.Email
            select new {
                RoleId = ur == null ? Guid.Empty : ur.RoleId,
                Dto = new UserDto {
                    Id = u.Id,
                    CompanyId = u.CompanyId,
                    CompanyName = c.Name,
                    UserName = u.UserName,
                    Email = u.Email,
                    TimeZoneId = u.TimeZoneId,
                    CreatedDateTime = u.CreatedDateTime,
                    IsDeleted = u.IsDeleted
                }
            };
        // RoleId -> RoleEnum (RoleEnumExtension) в SQL не переводится — после выборки
        var users = (await query.ToListAsync()).Select(e => { e.Dto.Role = e.RoleId.ToRoleEnum(); return e.Dto; }).ToList();

        var versions = await Host.GetService<LoginService>().GetAppVersionsAsync();
        foreach (var user in users) {
            user.ClientVersion = versions.GetValueOrDefault(user.Id);
        }
        return users;
    }

    public async Task<UserDto> GetAsync(Guid id) {
        var user = await Host.UserManager.FindByIdAsync(id.ToString());
        if (user == null) return null;
        var company = await base.Rep<Company>().GetByIdAsync(user.CompanyId);
        return new UserDto {
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = company?.Name,
            UserName = user.UserName,
            Email = user.Email,
            TimeZoneId = user.TimeZoneId,
            Role = await GetRoleAsync(user),
            CreatedDateTime = user.CreatedDateTime,
            IsDeleted = user.IsDeleted
        };
    }

    /// <summary>
    ///     Ответ входа App: токен плюс всё для сессии клиента </summary>
    public async Task<SessionDto> GetSessionAsync(AdUser user, string token) {
        var roles = await Host.UserManager.GetRolesAsync(user);
        var company = await base.Rep<Company>().GetByIdAsync(user.CompanyId);
        return new SessionDto {
            Token = token,
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = company?.Name,
            UserName = user.UserName,
            Email = user.Email,
            TimeZoneId = user.TimeZoneId,
            Roles = roles.ToList()
        };
    }

    public async Task<IdentityDto> GetIdentityAsync(AdUser user) {
        var roles = await Host.UserManager.GetRolesAsync(user);
        return new IdentityDto {
            Id = user.Id,
            CompanyId = user.CompanyId,
            UserName = user.UserName,
            Email = user.Email,
            TimeZoneId = user.TimeZoneId,
            Roles = roles.ToList()
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, Guid userId) {
        var user = new AdUser {
            Id = Guid.NewGuid(),
            CompanyId = dto.CompanyId,
            Email = Normalize(dto.Email),
            UserName = String.IsNullOrWhiteSpace(dto.UserName) ? Normalize(dto.Email) : dto.UserName.Trim(),
            TimeZoneId = dto.TimeZoneId ?? TimeZoneHelper.UtcId,
            CreatedId = userId,
            ModifiedId = userId,
            CreatedDateTime = DateTime.UtcNow,
            ModifiedDateTime = DateTime.UtcNow,
        };
        Check(await Host.UserManager.CreateAsync(user, dto.Password));
        Check(await Host.UserManager.AddToRoleAsync(user, dto.Role.ToString()));
        return await GetAsync(user.Id);
    }

    public async Task<UserDto> UpdateAsync(UpdateUserDto dto, Guid userId) {
        var user = await Host.UserManager.FindByIdAsync(dto.Id.ToString());
        if (user == null) throw new InfoException("User not found");

        user.CompanyId = dto.CompanyId;
        user.Email = Normalize(dto.Email);
        user.UserName = String.IsNullOrWhiteSpace(dto.UserName) ? user.Email : dto.UserName.Trim();
        user.TimeZoneId = dto.TimeZoneId ?? TimeZoneHelper.UtcId;
        user.ModifiedId = userId;
        user.ModifiedDateTime = DateTime.UtcNow;
        Check(await Host.UserManager.UpdateAsync(user));

        var role = await GetRoleAsync(user);
        if (role != dto.Role) {
            if (role != RoleEnum.Unknown) {
                Check(await Host.UserManager.RemoveFromRoleAsync(user, role.ToString()));
            }
            Check(await Host.UserManager.AddToRoleAsync(user, dto.Role.ToString()));
        }
        if (String.IsNullOrEmpty(dto.Password) == false) {
            var token = await Host.UserManager.GeneratePasswordResetTokenAsync(user);
            Check(await Host.UserManager.ResetPasswordAsync(user, token, dto.Password));
        }
        return await GetAsync(user.Id);
    }

    /// <summary>
    ///     Soft delete: IsDeleted + lockout + новый security stamp, чтобы cookie и токены перестали действовать </summary>
    public async Task DeleteAsync(Guid id, Guid userId) {
        var user = await Host.UserManager.FindByIdAsync(id.ToString());
        if (user == null) throw new InfoException("User not found");
        if (user.Id == userId) throw new InfoException("You cannot delete yourself");
        user.IsDeleted = true;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.ModifiedId = userId;
        user.ModifiedDateTime = DateTime.UtcNow;
        Check(await Host.UserManager.UpdateAsync(user));
        Check(await Host.UserManager.UpdateSecurityStampAsync(user));
    }

    public async Task RestoreAsync(Guid id, Guid userId) {
        var user = await Host.UserManager.FindByIdAsync(id.ToString());
        if (user == null) throw new InfoException("User not found");
        user.IsDeleted = false;
        user.LockoutEnd = null;
        user.ModifiedId = userId;
        user.ModifiedDateTime = DateTime.UtcNow;
        Check(await Host.UserManager.UpdateAsync(user));
    }

    #region [ private ]

    private async Task<RoleEnum> GetRoleAsync(AdUser user) {
        var roles = await Host.UserManager.GetRolesAsync(user);
        var name = roles.FirstOrDefault();
        return name != null && Enum.TryParse<RoleEnum>(name, out var role) ? role : RoleEnum.Unknown;
    }

    private static string Normalize(string email) => email?.Trim().ToLower();

    private static void Check(Microsoft.AspNetCore.Identity.IdentityResult result) {
        if (result.Succeeded == false) {
            throw new InfoException(String.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    #endregion [ private ]
}
