using AutoCall.Common;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Settings;
using MlSampleWindowCS.Services;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AutoCall.Logic.Helpers {
    /// <summary>
    ///     AutoPhone v.2 (проект AutoPhoneV2, Abto SIPVoipSDK) — V1 модуля AutoCall. Под .NET 10 — правки AutoCall V3: Thread.Abort не
    ///     поддерживается (поток формы закрывается Application.ExitThread, форма — Application.Run), COM SIPVoipSDK регистрируется для
    ///     текущего пользователя в HKCU (RegisterComForCurrentUser). Настройки — параметром (V1 — ServiceLocator) </summary>
    public static class AbtoPhoneHelper {
        // Scope на сервере: app.sip (как AutoCall V3)
        private static Serilog.ILogger Logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Sip);

        public static void TryClose() {
            try {
                if (thread != null && thread.IsAlive) {
                    if (AutoPhoneV2Form != null && AutoPhoneV2Form.IsDisposed == false) {
                        AutoPhoneV2Form.Invoke((Action)(() => {
                            AutoPhoneV2Form.Tag = "Close";
                            ((dynamic)AutoPhoneV2Form).TryClose();
                            AutoPhoneV2Form = null;
                            Application.ExitThread();
                        }));
                    }
                }
            } catch (Exception e) {
                Logger.Error(e, "AbtoPhoneHelper.TryClose failed");
            }
        }

        public static void Debug() {
            try {
                AutoPhoneV2Form.Invoke((Action)(() => {
                    ((dynamic)AutoPhoneV2Form).Debug();
                }));
            } catch (Exception e) {

                throw;
            }
        }

        private static Form AutoPhoneV2Form = null;

        public static void TryRestart(SettingsService settings, AutoPhoneV2CallService callService) {
            try {
                TryClose();
                StartProcess(settings, callService);
            } catch (Exception e) {
                Logger.Error(e, "AbtoPhoneHelper.TryRestart failed");
                throw;
            }
        }

        private static Thread thread;

        public static bool StartProcess(SettingsService settings, AutoPhoneV2CallService callService, bool flag = false) {
            var callSettings = settings.GetCallServiceSettings();
            if (callSettings.CallService == CallServiceType.AbtoPhone && AutoDispatch.Data.UserSession.Current?.MicroSIPLogin == null) {
                // как MicroSIP в AutoCall V3: без SIP-логина софтфон не стартует — раньше молча
                Logger.Warning("AutoPhoneV2: UserSession.SipLogin is empty, softphone not started");
            }
            if (callSettings.CallService == CallServiceType.AbtoPhone && AutoDispatch.Data.UserSession.Current?.MicroSIPLogin != null) {
                Logger.Information("AutoPhoneV2: starting");
                RegisterSIPVoipSDK32();
                var assembly = Assembly.Load("AutoPhoneV2");
                if (AutoPhoneV2Form == null || AutoPhoneV2Form.IsDisposed == true) {
                    thread = new Thread(() => {
                        AutoPhoneV2Form = (Form)assembly.CreateInstance("AutoPhoneV2.SIPPhoneForm");
                        if (((dynamic)AutoPhoneV2Form).ConfigurePhone(callService) == true) {
                            try {
                                // перед .Show() необходимо присвоить Action CallService.Notification в StartNotification()
                                AutoPhoneV2Form.Show();
                                Application.Run(AutoPhoneV2Form);
                            } catch (ThreadAbortException e) {
                            } catch (Exception e) {
                                Logger.Error(e, "AutoPhoneV2");
                                TryClose();
                            }
                        } else {
                            AutoPhoneV2Form = null;
                            if (flag == false) {
                                StartProcess(settings, callService, true);
                            }
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start();

                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        /// <summary> Копия SDK в профиль и COM-регистрация для текущего пользователя — SipVoipSdk (AutoPhoneV2) </summary>
        public static void RegisterSIPVoipSDK32() => SipVoipSdk.Register();
    }
}
