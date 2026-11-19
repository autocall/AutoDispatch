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
        public static void TryClose() {
            try {
                // Closes MicroSIP
                var proc = Process.GetProcesses().FirstOrDefault(e => e.ProcessName == "autophone");
                proc?.CloseMainWindow();
            } catch (Exception) {
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
            } catch (Exception) {
            }
        }

        public static bool StartProcess(SettingsService settings, string number = null) {
            // opens MicroSIP
            if (settings.GetCallServiceSettings().CallService == CallServiceType.MicroSIP &&
                Process.GetProcesses().Any(e => e.ProcessName == "autophone") == false) {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\phone\autophone.exe");
                CallServiceSettings callSettings = settings.GetCallServiceSettings();

                var session = UserSession.Current;
                if (String.IsNullOrEmpty(session.MicroSIPLogin) == false) {
                    Process.Start(path, "settings:" + callSettings.MicroSIPSettings);
                    if (number != null) {
                        Task.Factory.StartNew(async () => {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            Process.Start(path, "tel:" + number);
                        });
                    }
                }
                return true;
            } else {
                return false;
            }
        }
    }
}
