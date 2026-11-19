using System;
using System.Collections.Generic;

namespace AutoDispatch.Data {
    /// <summary>
    ///     User Session — из ответа Api/Account/SignIn (SessionDto). В V1 их было две: AutoDispatch.Data.IUserSession (вход в dispatch)
    ///     и AutoCall.Data.IUserSession (вход AutoCall: SIP-учётка, модули); теперь одна, статический UserSession.Current, как в AutoCall V3 </summary>
    public class UserSession {

        public static UserSession Current { get; private set; }

        public static void SetCurrent(UserSession session) {
            Current = session;
        }

        /// <summary>
        ///     Current User Name </summary>
        public string UserName { get; set; }
        /// <summary>
        ///     Current Display Name </summary>
        public string Name { get; set; }
        /// <summary>
        ///     Current User ID </summary>
        public Guid UserId { get; set; }
        /// <summary>
        ///     Company Name </summary>
        public string CompanyName { get; set; }
        /// <summary>
        ///     Company Id </summary>
        public Guid CompanyId { get; set; }
        /// <summary>
        ///     Assembly version </summary>
        public string AssemblyVersion { get; set; }
        /// <summary>
        ///     JWT сервера: Authorization: Bearer на всех Api/* (BaseApi.CreateRequest) </summary>
        public string Token { get; set; }
        /// <summary>
        ///     Имя как у V3 (BaseApi) </summary>
        public string AccessToken => Token;
        /// <summary>
        ///     Token Expires </summary>
        public DateTime TokenExpires { get; set; }
        /// <summary>
        ///     Роли — битовая сумма RoleName, как в V1: сервер отдаёт имена (SessionDto.Roles), сумму считает клиент (LoginManager) </summary>
        public int Roles { get; set; }
        /// <summary>
        ///     Actual Company has BNSF account </summary>
        public bool HasBnsfRailway { get; set; }
        /// <summary>
        ///    Company Custom Statuses </summary>
        public Dictionary<Guid, string> CustomStatuses { get; set; }
        /// <summary>
        ///     Учётка SIP софтфона (V1 AutoCall.Data.UserSession.MicroSIPLogin/Password; SessionDto.SipLogin/SipPassword) </summary>
        public string MicroSIPLogin { get; set; }
        public string MicroSIPPassword { get; set; }
    }
}
