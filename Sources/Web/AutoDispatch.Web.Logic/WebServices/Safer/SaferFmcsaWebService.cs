using System.Text.RegularExpressions;
using HtmlAgilityPack;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Logic.WebServices.Safer;

/// <summary>
///     Запасной источник карточки карриера — HTML safer.fmcsa.dot.gov/query.asp (V1 SaferFmcsaDataService + SaferFmcsaParser),
///     когда brokersnapshot недоступен </summary>
public class SaferFmcsaWebService : BaseWebService {

    private const string Url = "https://safer.fmcsa.dot.gov/query.asp";

    private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    public SaferFmcsaWebService(ServicesHost host) { }

    /// <summary>
    ///     queryParam: MC_MX или USDOT </summary>
    public async Task<ApiResult<Dictionary<string, string>>> GetFieldsAsync(int number, bool byMc) {
        var queryParam = byMc ? "MC_MX" : "USDOT";
        try {
            using var request = new HttpRequestMessage(HttpMethod.Post, Url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.84 Safari/537.36");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["searchtype"] = "ANY",
                ["query_type"] = "queryCarrierSnapshot",
                ["query_param"] = queryParam,
                ["query_string"] = number.ToString()
            });
            using var response = await Http.SendAsync(request);
            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            if (doc.DocumentNode.SelectSingleNode("//th[@class='querylabelbkg']") == null) {
                _l.w($"SaferFmcsa unrecognized page: {html.Truncate(1024)}");
                return ApiResult<Dictionary<string, string>>.Failed(System.Net.HttpStatusCode.NotFound, $"Company was not found: {queryParam}={number}");
            }
            return ApiResult<Dictionary<string, string>>.Successed(ParseFields(doc));
        } catch (Exception e) {
            _l.e("SaferFmcsa failed", e);
            return ApiResult<Dictionary<string, string>>.Failed(e);
        }
    }

    private static Dictionary<string, string> ParseFields(HtmlDocument doc) {
        var result = new Dictionary<string, string>();
        var regex = new Regex("[ ]{2,}");
        var thList = doc.DocumentNode.SelectNodes("//th[@class='querylabelbkg']");
        foreach (HtmlNode node in thList) {
            var td = node.SelectSingleNode("following-sibling::td[1]");
            if (td != null) {
                var key = regex.Replace(node.InnerText.Replace("&nbsp;", "").Trim(), " ");
                if (key == "Operating Authority Status:") {
                    result["Operating Status:"] = td.FirstChild.InnerText.Trim();
                }
                var value = regex.Replace(td.InnerText.Replace("&nbsp;", "").Trim(), " ");
                result[key] = value;
            }
        }
        return result;
    }
}
