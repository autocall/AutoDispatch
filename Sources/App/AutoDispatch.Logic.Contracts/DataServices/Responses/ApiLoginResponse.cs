using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Logic.DataServices.Responses {
    /// <summary>
    ///     Ответ входа — SessionDto сервера (Api/Account/SignIn, AI/wiki/protocol.md). V1 читал OAuth-ответ Api/Token
    ///     (access_token, .expires, roles — битовая сумма, customStatuses — JSON); роли теперь — имена, сумму считает LoginManager,
    ///     статусы — список CompanyCustomStatusDto (решено 2026-11-17, AI/wiki/plan.md «Логин App») </summary>
    public class ApiLoginResponse : BaseResponse {
        /// <summary> JWT (V1 access_token) </summary>
        public string Token { get; set; }
        /// <summary> Id пользователя (V1 userId) </summary>
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        /// <summary> Отображаемое имя (V1 name) </summary>
        public string UserName { get; set; }
        /// <summary> Логин (V1 userName) </summary>
        public string Email { get; set; }
        public string TimeZoneId { get; set; }
        public List<string> Roles { get; set; }
        public bool HasBnsfRailway { get; set; }
        public List<CompanyCustomStatusDto> CustomStatuses { get; set; }
        /// <summary> Учётка SIP софтфона (V1 — из входа AutoCall: MicroSIPLogin/MicroSIPPassword) </summary>
        public string SipLogin { get; set; }
        public string SipPassword { get; set; }
    }
}
