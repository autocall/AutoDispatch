using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Attributes;

/// <summary>
///     Минимальная роль для метода IDispatchService (V1 MinAccessAttribute, RoleName → RoleEnum): проверяется в Api/Dispatch/Request.
///     ExtraRoles — роли, которым метод тоже разрешён помимо иерархии (Agent для ConfirmationSend) </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MinAccessAttribute : Attribute {
    public RoleEnum Role { get; }
    public RoleEnum[] ExtraRoles { get; }
    public MinAccessAttribute(RoleEnum role, RoleEnum[] extraRoles = null) {
        this.Role = role;
        this.ExtraRoles = extraRoles;
    }
}
