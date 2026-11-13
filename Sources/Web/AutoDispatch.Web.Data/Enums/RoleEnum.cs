using System.ComponentModel;

namespace AutoDispatch.Web.Data.Enums;

/// <summary>
///     Id роли в IdentityRoles = ToRoleId() (RoleEnumExtension). Роли V1 с модулем AutoDispatch (Permissions.cs V1); остальные роли V1 не заводятся
///     (решено 2026-11-11, AI/wiki/plan.md). Права по ролям — MinAccessAttribute и RolesOperator V1, переносятся на этапах 3–6 </summary>
public enum RoleEnum {
    Unknown = 0,
    [Description("Agent")]
    Agent = 1,
    [Description("Warehouse")]
    Warehouse = 2,
    [Description("Dispatcher")]
    Dispatcher = 3,
    [Description("Sales")]
    Sales = 4,
    [Description("Accounting")]
    Accounting = 5,
    [Description("Owner")]
    Owner = 6,
    [Description("Admin")]
    Admin = 7
}
