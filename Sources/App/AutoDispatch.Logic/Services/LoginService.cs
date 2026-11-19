using System.Threading.Tasks;
using AutoCall.Common.Enums;
using AutoDispatch.Data;
using AutoDispatch.Logic.Managers;
using AutoDispatch.Logic.Managers.Results;

namespace AutoDispatch.Logic.Services {
    public class LoginService {

        #region [ _fields ]

        private LoginManager _manager;

        #endregion [ _fields ]

        #region [ .ctor ]

        public LoginService(LoginManager manager) {
            _manager = manager;
        }

        #endregion [ .ctor ]

        #region [ Public ]

        public Task<LoginResult> LoginAsync(string userName, string password, string version) {
            return _manager.LoginAsync(userName, password, version);
        }

        public Task<AutoCall.Logic.Managers.Results.ForgotPasswordResult> ForgotPasswordAsync(string email) {
            return _manager.ForgotPasswordAsync(email);
        }

        public LogoutResult Logout() {
            return _manager.Logout();
        }

        public string GetToken() {
            var session = UserSession.Current;
            return session.Token;
        }
        public RoleName GetRoles() {
            var session = UserSession.Current;
            return (RoleName)session.Roles;
        }

        #endregion [ Public ]
    }
}
