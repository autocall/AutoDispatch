using AutoCall.Common;
using AutoDispatch.Data;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoCall.Logic.Helpers {
    /// <summary>
    ///     AutoPhone (MicroSIP, внешний Resourcesphoneutophone.exe), V1 модуля AutoCall. Настройки — параметром (V1 — ServiceLocator) </summary>
    public static class MicroSIPHelper {
        // Scope на сервере: app.sip (как AutoCall V3)
        private static Serilog.ILogger Logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Sip);

        public static void TryClose() {
            try {
                // Closes MicroSIP
                var proc = Process.GetProcesses().FirstOrDefault(e => e.ProcessName == "autophone");
                proc?.CloseMainWindow();
            } catch (Exception e) {
                Logger.Error(e, "MicroSIPHelper.TryClose failed");
            }
        }

        public static async Task TryRestart(SettingsService settings) {
            try {
                // restarts MicroSIP
                if (settings.GetCallServiceSettings().CallService == CallServiceType.MicroSIP) {
                    Process.GetProcesses().FirstOrDefault(e => e.ProcessName == "autophone")?.CloseMainWindow();
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    StartProcess(settings);
                }
            } catch (Exception e) {
                Logger.Error(e, "MicroSIPHelper.TryRestart failed");
            }
        }

        public static bool StartProcess(SettingsService settings, string number = null) {
            // opens MicroSIP
            if (settings.GetCallServiceSettings().CallService == CallServiceType.MicroSIP &&
                Process.GetProcesses().Any(e => e.ProcessName == "autophone") == false) {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\phone\autophone.exe");
                CallServiceSettings callSettings = settings.GetCallServiceSettings();

                var session = UserSession.Current;
                if (String.IsNullOrEmpty(session?.MicroSIPLogin) == false) {
                    try {
                        Logger.Information("MicroSIP: starting {Path}", path);
                        Process.Start(path, "settings:" + callSettings.MicroSIPSettings);
                        if (number != null) {
                            Task.Factory.StartNew(async () => {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                Process.Start(path, "tel:" + number);
                            });
                        }
                    } catch (Exception e) {
                        Logger.Error(e, "MicroSIP: failed to start {Path}", path);
                    }
                } else {
                    Logger.Warning("MicroSIP: UserSession.SipLogin is empty, autophone.exe not started");
                }
                return true;
            } else {
                return false;
            }
        }
    }
}
