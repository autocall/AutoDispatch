using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Common.Enums;
using AutoDispatch.Data;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Managers.Results;

namespace AutoDispatch.Logic.Managers {
    /// <summary>
    ///     Вход в App. В V1 вход был двойной — AutoCall (IApiManager: сессия, модули, SIP-учётка) и dispatch (этот менеджер, OAuth Api/Token);
    ///     теперь один Api/Account/SignIn, сессия — UserSession.Current </summary>
    public class LoginManager {
        #region [ Private Fields ]

        private LoginDataService _dataService;

        #endregion [ Private Fields ]

        #region [ .ctor ]

        public LoginManager(LoginDataService dataService) {
            _dataService = dataService;
        }

        #endregion [ .ctor ]

        #region [ Public Methods ]

        public string GetWindowTitle(string title) {
            var session = UserSession.Current;
            return $"{title} ({session.CompanyName} - {session.UserName}) v.{session.AssemblyVersion}";
        }

        public async Task<LoginResult> LoginAsync(string userName, string password, string version) {
            try {
                ApiLoginResponse result = await _dataService.LoginAsync(new ApiLoginRequest() {
                    UserName = userName,
                    UserPassword = password,
                    Version = version
                });
                //Create session
                var session = new UserSession {
                    UserName = result.Email,
                    Name = result.UserName,
                    UserId = result.Id,
                    CompanyName = result.CompanyName,
                    CompanyId = result.CompanyId,
                    AssemblyVersion = version,
                    Token = result.Token,
                    Roles = ToRoles(result.Roles),
                    HasBnsfRailway = result.HasBnsfRailway,
                    MicroSIPLogin = result.SipLogin,
                    MicroSIPPassword = result.SipPassword,
                };
                try {
                    session.CustomStatuses = result.CustomStatuses?.ToDictionary(e => e.EntityId, e => e.Name) ?? new Dictionary<Guid, string>();
                } catch (Exception e) {
                    _webl.e("Login", e);
                }
                UserSession.SetCurrent(session);

                return new LoginResult() {
                    State = LoginState.LoggedIn,
                    UserName = session.UserName,
                    UserId = session.UserId,
                    Token = session.Token,
                };
            } catch (Exception exception) {
                var result = new LoginResult() {
                    State = LoginState.LoginFailed,
                };
                result.ErrorResult(exception);
                return result;
            }
        }

        public async Task<LoginResult> CheckTokenAsync(string token) {
            // Do request
            try {
                var result = await _dataService.CheckTokenAsync(new ApiCheckTokenRequest() { Token = token });
                if (result.IsSuccessed) {
                    return new LoginResult() {
                        //todo return user name
                        State = LoginState.LoggedIn
                    };
                } else {
                    return new LoginResult() {
                        State = LoginState.LoggedOut
                    };
                }
            } catch (Exception exception) {
                var result = new LoginResult() {
                    State = LoginState.LoginFailed
                };
                result.ErrorResult(exception);
                return result;
            }
        }

        public async Task<AutoCall.Logic.Managers.Results.ForgotPasswordResult> ForgotPasswordAsync(string email) {
            var result = new AutoCall.Logic.Managers.Results.ForgotPasswordResult();
            try {
                result.Message = await _dataService.ForgotPasswordAsync(email);
            } catch (Exception exception) {
                result.ErrorResult(exception);
            }
            return result;
        }

        /// <summary>
        ///     V1 вызывал Api/Account/Logout; у JWT серверной сессии нет — сессия App просто сбрасывается </summary>
        public LogoutResult Logout() {
            UserSession.SetCurrent(null);
            return new LogoutResult() {
                State = LoginState.LoggedOut
            };
        }

        #endregion [ Public Methods ]

        /// <summary>
        ///     Имена ролей сервера (RoleEnum) → битовая сумма RoleName V1: на ней App проверяет права (RolesOperator, CheckPermission) </summary>
        private static int ToRoles(IEnumerable<string> roles) {
            int result = 0;
            foreach (var role in roles ?? Enumerable.Empty<string>()) {
                if (Enum.TryParse(role, true, out RoleName value)) {
                    result |= (int)value;
                }
            }
            return result;
        }
    }
}
