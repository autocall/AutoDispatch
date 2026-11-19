using System;
using System.Reflection;

namespace AutoCall.WPF.Helpers {
    /// <summary>
    ///     Версия приложения для показа пользователю (заголовки окон), копия AutoCall V3. Версия публикации ClickOnce лаунчер отдаёт
    ///     процессу через env-переменные (System.Deployment.Application V1 на net10.0 нет); вне ClickOnce — версия сборки </summary>
    public static class VersionHelper {

        public static string GetDisplayVersion() {
            string version = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion");
            if (String.IsNullOrEmpty(version)) {
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
            }
            return version;
        }

        /// <summary>
        ///     V1 LoginDialog: ApplicationDeployment.CurrentDeployment.UpdateLocation содержит "/Beta/" </summary>
        public static bool IsBeta() {
            return Environment.GetEnvironmentVariable("ClickOnce_UpdateLocation")?.Contains("/Beta/") == true;
        }
    }
}
