using System.Net;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.WebServices;

/// <summary>
///     Vodia PBX по FTP: CDR-лог звонков за день (CSV, первая строка — заголовок). Копия AutoCall V3 </summary>
public class VodiaWebService : BaseWebService {

    public VodiaWebService(ServicesHost host) { }

    /// <summary>
    ///     Строки CDR за дату без заголовка; файла нет (день без звонков или будущий) — пустой список </summary>
    public async Task<ApiResult<List<string>>> GetCdrAsync(DateTime date) {
        if (AppSettings.VodiaFtpLogin == null) {
            return ApiResult<List<string>>.Failed(HttpStatusCode.ServiceUnavailable, "Vodia FTP is not configured");
        }
        var url = String.Format(AppSettings.VodiaCdrUrl, date);
        try {
            // FtpWebRequest устарел (SYSLIB0014), но замены в .NET нет; сторонний FTP-клиент ради одного файла не тянем
#pragma warning disable SYSLIB0014
            var request = (FtpWebRequest)WebRequest.Create(url);
#pragma warning restore SYSLIB0014
            request.Credentials = new NetworkCredential(AppSettings.VodiaFtpLogin, AppSettings.VodiaFtpPassword);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Timeout = (int)AppSettings.VodiaFtpTimeout.TotalMilliseconds;
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var reader = new StreamReader(response.GetResponseStream());
            var lines = new List<string>();
            await reader.ReadLineAsync();
            while (await reader.ReadLineAsync() is { } line) {
                lines.Add(line);
            }
            return ApiResult<List<string>>.Successed(lines);
        } catch (WebException e) when ((e.Response as FtpWebResponse)?.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable) {
            return ApiResult<List<string>>.Successed([]);
        } catch (Exception e) {
            _l.e($"Vodia CDR {url} failed", e);
            return ApiResult<List<string>>.Failed(e);
        }
    }
}
