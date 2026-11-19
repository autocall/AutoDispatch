using AutoCall.Logic.Services;
using AutoCall.Logic.ViewModels;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MlSampleWindowCS.Services {

    /// <summary>
    ///     Мост между AbtoPhoneCallService (AutoDispatch.Logic) и формой SIPPhoneForm. V1 — синглтон Unity за IAutoPhoneV2CallService;
    ///     теперь синглтон MS DI, форма получает его в ConfigurePhone. Get/SetCallServiceSettings выставляет MainManager:
    ///     V1 ContactsForm брал IMainManager через ServiceLocator, а прямой ссылки на AutoDispatch.Logic у проекта нет </summary>
    public class AutoPhoneV2CallService {

        public Func<SettingsCallServiceViewModel> GetCallServiceSettings { get; set; }
        public Action<SettingsCallServiceViewModel> SetCallServiceSettings { get; set; }


        public Action<string, string, string, string, object> MAKE_CALL_Action { get; set; }
        public Action<string> HANGUP_Action { get; set; }
        public Action<string> HANGUP_ALL_Action { get; set; }
        public Action<string> HOLD_Action { get; set; }
        public Action<string> RESUME_Action { get; set; }
        public Action<string> TRANSFER_Action { get; set; }
        public Action<string, string> ACTION_DTMF_Action { get; set; }
        public Action<string> SET_ACCOUNT_Action { get; set; }

        public Action<string> DisplayNotifyMsg { get; set; }

        private DateTime SET_ACCOUNT_ActionDateTime = DateTime.MinValue;

        public AutoPhoneV2CallService() {        }

        public void StartAction(
                abtophone_action action,
                string phone,
                string header = null,
                string subHeader = null,
                string loadId = null, 
                object model = null) {
            switch (action) {
                case abtophone_action.MAKE_CALL:
                    MAKE_CALL_Action?.Invoke(phone, header, subHeader, loadId, model);
                    break;
                case abtophone_action.HANGUP:
                    HANGUP_Action?.Invoke(phone);
                    break;
                case abtophone_action.HANGUP_ALL:
                    HANGUP_ALL_Action?.Invoke(null);
                    break;
                case abtophone_action.HOLD:
                    HOLD_Action?.Invoke(phone);
                    break;
                case abtophone_action.RESUME:
                    RESUME_Action?.Invoke(phone);
                    break;
                case abtophone_action.TRANSFER:
                    TRANSFER_Action?.Invoke(phone);
                    break;
                case abtophone_action.ACTION_DTMF:
                    ACTION_DTMF_Action?.Invoke(phone, header);
                    break;
                case abtophone_action.SET_ACCOUNT:
                    // пропустить авторизацию если две подряд
                    if (DateTime.UtcNow - SET_ACCOUNT_ActionDateTime > TimeSpan.FromSeconds(1)) {
                        SET_ACCOUNT_Action?.Invoke(phone);
                    }
                    SET_ACCOUNT_ActionDateTime = DateTime.UtcNow;
                    break;
            }
        }

        public Action<abtophone_state /*state*/, string /*phone*/, int? /*line*/, object /*model*/> Notification { get; set; }
    }
}
