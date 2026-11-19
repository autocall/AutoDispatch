using AutoCall.Common.Attributes;
using AutoCall.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoCall.Common.Extensions {

    public enum RolesOperator {
        DelayRepostAllEnable,
        ShowRepostAll,
        ViewRate,
        EditLayoutPallets,
        BrokerBoards,
        CarrierBoards,
        AccountingBoards,
    }

    public static class RolesExtension {

        private static Dictionary<RoleName, int> roles = 
            Enum.GetValues(typeof(RoleName)).Cast<RoleName>().ToDictionary(e => e, e => DisplayAttribute.GetOrder(e));

        public static bool CheckPermission(this RoleName userRole, RoleName minRole, RoleName[] extraRoles = null) {
            if (roles.ContainsKey(userRole) == false) {
                throw new Exception($"mixed role: {userRole}");
            }
            return roles[userRole] >= roles[minRole] || (extraRoles != null && extraRoles.Contains(userRole));
        }

        public static bool CheckPermission(this RoleName userRole, RolesOperator @operator) {
            switch (@operator) {
                case RolesOperator.DelayRepostAllEnable:
                    return CheckPermission(userRole, RoleName.Admin);
                case RolesOperator.ShowRepostAll:
                    return CheckPermission(userRole, RoleName.Accounting);
                case RolesOperator.ViewRate:
                    return CheckPermission(userRole, RoleName.Accounting, extraRoles: new[] { RoleName.Agent });
                case RolesOperator.EditLayoutPallets:
                    return CheckPermission(userRole, RoleName.Owner);
                case RolesOperator.BrokerBoards:
                    return CheckPermission(userRole, RoleName.Agent);
                case RolesOperator.CarrierBoards:
                    return CheckPermission(userRole, RoleName.Agent);
                case RolesOperator.AccountingBoards:
                    return CheckPermission(userRole, RoleName.Accounting);
                default:
                    throw new Exception($"implement {@operator} permission operator");
            }
        }
    }
}
