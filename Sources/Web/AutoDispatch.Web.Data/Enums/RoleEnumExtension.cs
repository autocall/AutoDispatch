namespace AutoDispatch.Web.Data.Enums;

/// <summary>
///     Id роли в IdentityRoles = Guid из значения RoleEnum: 00000000-0000-0000-0000-000000000003 для Dispatcher (ключи Guid, AI/wiki/plan.md) </summary>
public static class RoleEnumExtension {
    public static Guid ToRoleId(this RoleEnum role) => new Guid($"00000000-0000-0000-0000-{(int)role:D12}");

    public static RoleEnum ToRoleEnum(this Guid roleId) {
        var bytes = roleId.ToByteArray();
        return Enum.IsDefined(typeof(RoleEnum), (int)bytes[15]) && roleId == ((RoleEnum)bytes[15]).ToRoleId() ? (RoleEnum)bytes[15] : RoleEnum.Unknown;
    }
}
