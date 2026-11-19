using AutoCall.Common;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Settings;
using Microsoft.Win32;
using MlSampleWindowCS.Services;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;

namespace AutoCall.Logic.Helpers {
    /// <summary>
    ///     AutoPhone v.2 (проект AutoPhoneV2, Abto SIPVoipSDK) — V1 модуля AutoCall. Под .NET 10 — правки AutoCall V3: Thread.Abort не
    ///     поддерживается (поток формы закрывается Application.ExitThread, форма — Application.Run), COM SIPVoipSDK регистрируется для
    ///     текущего пользователя в HKCU (RegisterComForCurrentUser). Настройки — параметром (V1 — ServiceLocator) </summary>
    public static class AbtoPhoneHelper {
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
                _webl.e("AbtoPhoneHelper.TryClose", e);
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
                _webl.e(e.Message);
                throw;
            }
        }

        private static Thread thread;

        public static bool StartProcess(SettingsService settings, AutoPhoneV2CallService callService, bool flag = false) {
            var callSettings = settings.GetCallServiceSettings();
            if (callSettings.CallService == CallServiceType.AbtoPhone && AutoDispatch.Data.UserSession.Current?.MicroSIPLogin != null) {
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
                                _webl.e("AutoPhoneV2", e);
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

        public static void RegisterSIPVoipSDK32() {
            var originPath = Path.Combine(Environment.CurrentDirectory, "Resources", "phone", "SIPVoipSDK");
            var destinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIPVoipSDK");
            if (Directory.Exists(destinPath) == false) {
                Directory.CreateDirectory(destinPath);
            }
            CopyFilesRecursively(originPath, destinPath);

            try {
                RegisterComForCurrentUser(Path.Combine(destinPath, "SIPVoipSDK.dll"));
            } catch (Exception e) {
                _webl.e("RegisterSIPVoipSDK32: failed to register per-user COM override", e);
            }
        }

        #region [ COM для текущего пользователя (AutoCall V3) ]

        /// <summary>
        ///     CLSID и TypeLib COM-класса CAbtoPhone (Abto VoIP SIP SDK 4.0), одинаковы для любой копии этой версии SIPVoipSDK.dll (V3 Settings) </summary>
        private const string SIPVoipSDKClsid = "{9BCEE7C8-3FE1-49FA-94D5-FC92DB4301F1}";
        private const string SIPVoipSDKTypeLib = "{51E1C96F-F16B-4D5A-A790-4C72E219F9AE}";

        // Регистрация CAbtoPhone в HKCU\SOFTWARE\Classes (COM даёт ей приоритет перед HKLM): активация работает без прав администратора,
        // даже если компонент не зарегистрирован для пользователя или регистрация указывает на недоступную DLL. Копия AutoCall V3
        private static void RegisterComForCurrentUser(string dllPath) {
            using (var classes = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes")) {
                using (var inprocServer32 = classes.CreateSubKey($@"CLSID\{SIPVoipSDKClsid}\InprocServer32")) {
                    inprocServer32.SetValue("", dllPath);
                    inprocServer32.SetValue("ThreadingModel", "");
                }
                using (var progId = classes.CreateSubKey($@"CLSID\{SIPVoipSDKClsid}\ProgID")) {
                    progId.SetValue("", "SIPVoipSDK.AbtoPhone.1");
                }
                using (var verIndProgId = classes.CreateSubKey($@"CLSID\{SIPVoipSDKClsid}\VersionIndependentProgID")) {
                    verIndProgId.SetValue("", "SIPVoipSDK.AbtoPhone");
                }
                using (var typeLib = classes.CreateSubKey($@"CLSID\{SIPVoipSDKClsid}\TypeLib")) {
                    typeLib.SetValue("", SIPVoipSDKTypeLib);
                }
                using (var typeLibVersion = classes.CreateSubKey($@"TypeLib\{SIPVoipSDKTypeLib}\1.0")) {
                    typeLibVersion.SetValue("", "ABTO Voip Video SIP SDK 4.0 Type Library");
                    typeLibVersion.SetValue("FLAGS", "0");
                    typeLibVersion.SetValue("HELPDIR", "");
                }
                using (var typeLibWin32 = classes.CreateSubKey($@"TypeLib\{SIPVoipSDKTypeLib}\1.0\0\win32")) {
                    typeLibWin32.SetValue("", dllPath);
                }
            }

            // Ключи Interface\{IID} для всех интерфейсов SDK: без них proxy между потоками не создаётся (E_NOINTERFACE)
            RegisterTypeLibForUser(LoadTypeLibEx(dllPath, RegKindNone), dllPath, null);
        }

        private const int RegKindNone = 2;

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern ITypeLib LoadTypeLibEx(string file, int regKind);

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void RegisterTypeLibForUser(ITypeLib typeLib, string fullPath, string helpDir);

        #endregion [ COM для текущего пользователя (AutoCall V3) ]

        private static void CopyFilesRecursively(string sourcePath, string targetPath) {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
                if (File.Exists(newPath.Replace(sourcePath, targetPath)) == false) {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
            }
        }
    }
}
