using System.Xml.Linq;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic.Dtos;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Блок Download на Home: ClickOnce AppV3 из AppSettings.DownloadPath (Home/Download WebV1) </summary>
public class DownloadService : BaseService {

    public DownloadService(ServicesHost host) : base(host) { }

    public DownloadDto Get() {
        return new DownloadDto {
            Version = GetVersion()
        };
    }

    #region [ private ]

    /// <summary>
    ///     Версия из манифеста развёртывания — её показывает AppV3 (VersionHelper), а не версия сборки </summary>
    private string GetVersion() {
        var path = Path.Combine(AppSettings.DownloadPath, AppSettings.DownloadApplicationFile);
        if (File.Exists(path) == false) {
            return null;
        }
        var identity = XDocument.Load(path).Root?.Elements().FirstOrDefault(e => e.Name.LocalName == "assemblyIdentity");
        return identity?.Attribute("version")?.Value;
    }

    #endregion [ private ]
}
