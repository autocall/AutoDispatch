using AutoCall.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace MlSampleWindowCS.Services {
    /// <summary>
    ///     Регистрация Abto SIPVoipSDK для текущего пользователя и её диагностика (AutoCall V3 AbtoPhoneHelper). В проекте AutoPhoneV2, а не
    ///     в AutoDispatch.Logic: нужна и PhoneForm (CreateAbtoPhone), а Logic сам ссылается на AutoPhoneV2. AbtoPhoneHelper вызывает Register </summary>
    public static class SipVoipSdk {
        // Scope на сервере: app.sip
        private static Serilog.ILogger Logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Sip);

        public static void Register() {
            var originPath = Path.Combine(Environment.CurrentDirectory, "Resources", "phone", "SIPVoipSDK");
            var destinPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIPVoipSDK");
            if (Directory.Exists(destinPath) == false) {
                Directory.CreateDirectory(destinPath);
            }
            CopyFilesRecursively(originPath, destinPath);

            try {
                RegisterComForCurrentUser(Path.Combine(destinPath, "SIPVoipSDK.dll"));
            } catch (Exception e) {
                Logger.Error(e, "RegisterSIPVoipSDK32: failed to register per-user COM override");
            }
        }

        #region [ COM для текущего пользователя (AutoCall V3) ]

        /// <summary>
        ///     CLSID и TypeLib COM-класса CAbtoPhone (Abto VoIP SIP SDK 4.0), одинаковы для любой копии этой версии SIPVoipSDK.dll (V3 Settings) </summary>
        public const string SIPVoipSDKClsid = "{9BCEE7C8-3FE1-49FA-94D5-FC92DB4301F1}";
        public const string SIPVoipSDKTypeLib = "{51E1C96F-F16B-4D5A-A790-4C72E219F9AE}";

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

        private const string TypeLibMarshaler = "{00020424-0000-0000-C000-000000000046}";

        /// <summary> Интерфейс маршалится стандартным typelib-маршалером — иначе вызов из потока AbtoPhone даёт E_NOINTERFACE (AutoCall V3) </summary>
        public static bool IsProxyRegistered(Guid iid) {
            using (var classes = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32))
            using (var proxyStub = classes.OpenSubKey($@"Interface\{iid:B}\ProxyStubClsid32")) {
                return string.Equals(proxyStub?.GetValue("") as string, TypeLibMarshaler, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        ///     COM-регистрация SDK для Details лога (AutoCall V3): по ней видно, нет ли ключа Interface (E_NOINTERFACE) или CLSID указывает
        ///     на DLL другой версии, чем в поставке. Ключи реестра пользователь сам не пришлёт </summary>
        public static string ComDiagnostics(Guid iid) {
            try {
                var lines = new List<string> {
                    $"Process: {(Environment.Is64BitProcess ? "x64" : "x86")}, thread {Thread.CurrentThread.GetApartmentState()}"
                };
                foreach (RegistryHive hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine }) {
                    using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32))
                    using (var classes = baseKey.OpenSubKey(@"SOFTWARE\Classes")) {
                        string name = hive == RegistryHive.CurrentUser ? "HKCU" : "HKLM";
                        lines.Add($"{name} CLSID: {RegistryValue(classes, $@"CLSID\{SIPVoipSDKClsid}\InprocServer32") ?? "<none>"}, " +
                            $"ThreadingModel '{RegistryValue(classes, $@"CLSID\{SIPVoipSDKClsid}\InprocServer32", "ThreadingModel")}'");
                        lines.Add($"{name} Interface: ProxyStub {RegistryValue(classes, $@"Interface\{iid:B}\ProxyStubClsid32") ?? "<none>"}, " +
                            $"TypeLib {RegistryValue(classes, $@"Interface\{iid:B}\TypeLib") ?? "<none>"}");
                        lines.Add($"{name} TypeLib: {RegistryValue(classes, $@"TypeLib\{SIPVoipSDKTypeLib}\1.0\0\win32") ?? "<none>"}");
                    }
                }
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry32)) {
                    lines.Add($"Registered DLL: {DllInfo(RegistryValue(root, $@"CLSID\{SIPVoipSDKClsid}\InprocServer32"))}");
                }
                lines.Add($"Bundled DLL: {DllInfo(Path.Combine(Environment.CurrentDirectory, "Resources", "phone", "SIPVoipSDK", "SIPVoipSDK.dll"))}");
                return string.Join("\n", lines);
            } catch (Exception e) {
                return $"Diagnostics failed: {e.Message}";
            }
        }

        private static string RegistryValue(RegistryKey key, string subKey, string name = "") {
            using (var sub = key?.OpenSubKey(subKey)) {
                return sub?.GetValue(name)?.ToString();
            }
        }

        private static string DllInfo(string path) {
            if (path == null || File.Exists(path) == false) {
                return $"{path ?? "<none>"} not found";
            }
            return $"{path}, version {FileVersionInfo.GetVersionInfo(path).FileVersion}, modified {File.GetLastWriteTimeUtc(path):u}";
        }

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
