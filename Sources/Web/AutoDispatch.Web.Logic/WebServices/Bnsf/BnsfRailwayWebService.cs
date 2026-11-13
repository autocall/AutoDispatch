using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.WebServices.Bnsf;

/// <summary>
///     Отчёт по контейнерам из BNSF Rail Central (V1 BnsfRailwayDataSource): вход через idp.bnsf.com (SAML), отчёт «AutoCall»,
///     координаты через MapView, ePay — суммы к оплате. Cookies сессии хранятся в DispatchCompany.BnsfCookies (JSON) между запусками.
///     HttpWebRequest V1 → HttpClient с CookieContainer на запрос; cookies переносятся вручную, как в V1 </summary>
public class BnsfRailwayWebService : BaseWebService {

    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0";

    public BnsfRailwayWebService(ServicesHost host) { }

    public class Report {
        public List<BnsfRailwayTrailerDto> Trailers { get; set; }
        public string Message { get; set; }
        /// <summary>
        ///     Cookies сессии для сохранения в компании (JSON); null — сессия сброшена </summary>
        public string Cookies { get; set; }
    }

    /// <summary>
    ///     company.BnsfCookies — прошлая сессия; при ошибке возвращает Failed, и компании нужно сбросить BnsfCookies </summary>
    public async Task<ApiResult<Report>> GetReportAsync(string login, string password, string savedCookies) {
        try {
            if (String.IsNullOrEmpty(login)) {
                throw new Exception("Login failed");
            }
            var message = String.Empty;
            List<Cookie> cookies;
            var newSession = false;

            #region [ auth ]
            if (savedCookies == null) {
                newSession = true;
                var result1 = await RequestGetLoginPage();
                cookies = ToList(result1.Cookies);

                var result2 = await RequestPostIdpDiscovery(login, ToCookieCollection(cookies));
                cookies = MergeCookies(cookies, result2.Cookies);

                var result3 = await RequestPostLogin(result2.Data, login, password, ToCookieCollection(cookies));
                cookies = MergeCookies(cookies, result3.Cookies);

                var result4 = await RequestPostSaml20Login(result3.Data, ToCookieCollection(cookies));
                cookies = MergeCookies(cookies, result4.Cookies);

                var result5 = await RequestGetRailcentralServlet(ToCookieCollection(cookies));
                cookies = MergeCookies(cookies, result5.Cookies);

                var reportId = await RequestReportList(ToCookieCollection(cookies), "Action=AG_REPRT_LIST&P_ANAME=A_GET_REPRT_LIST");
                await RequestActionServlet(ToCookieCollection(cookies), $"Action=GetRecords&UserID={login}&ReportId={reportId}");
            } else {
                cookies = JsonConvert.DeserializeObject<List<CookieDto>>(savedCookies).Select(CookieToModel).ToList();
            }
            #endregion [ auth ]

            #region [ trailers ]
            var final3 = await RequestActionServlet(ToCookieCollection(cookies),
                "Action=getDataViewDataBySort&start=0&limit=2000&sortInfo=3*ASC%241*ASC%24&sort=&dir=&groupBy=3&groupDir=ASC");
            List<BnsfRailwayTrailerDto> trailers = JsonToResult(final3);
            #endregion [ trailers ]

            #region [ Latitude,Longitude ]
            var mapArgs = new Dictionary<string, string>();
            foreach (var item in ((dynamic)JsonConvert.DeserializeObject(final3)).DATA_VIEW_MAIN_DATA) {
                mapArgs[item["1"].Value] = ((string)item["4"].Value).Replace(" ", "").Replace(",", ";");
            }
            try {
                var final4 = await RequestActionServlet(ToCookieCollection(cookies),
                    $"Action=MapView&CARS={Uri.EscapeDataString(String.Join("|", mapArgs.Select(e => $"{e.Key};{e.Value}")))}&subFlag=");
                JsonSetLocations(trailers, final4);
            } catch (Exception e) {
                _l.w($"Bnsf MapView: {e.Message}");
                message += "no location\n";
            }
            #endregion [ Latitude,Longitude ]

            #region [ Accounting ]
            try {
                var epay1 = await RequestEPayController1(ToCookieCollection(cookies));
                cookies = MergeCookies(cookies, epay1.Cookies);
                var dates = await RequestEPayController2(epay1.Data, ToCookieCollection(cookies));
                var html = await RequestEPayController3(dates, ToCookieCollection(cookies));
                SetAccounting(trailers, html);
            } catch (Exception e) {
                _l.w($"Bnsf ePay: {e.Message}");
                message += "no accounting\n";
            }
            #endregion [ Accounting ]

            return ApiResult<Report>.Successed(new Report {
                Trailers = trailers,
                Message = message,
                Cookies = newSession ? JsonConvert.SerializeObject(cookies.Select(CookieToDto).ToList()) : savedCookies
            });
        } catch (Exception e) {
            _l.e("Bnsf report failed", e);
            return ApiResult<Report>.Failed(e);
        }
    }

    #region [ Cookies ]

    private class CookieDto {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Path { get; set; }
    }

    private static CookieDto CookieToDto(Cookie model) => new CookieDto { Name = model.Name, Value = model.Value, Path = model.Path };

    private static Cookie CookieToModel(CookieDto dto) => new Cookie { Name = dto.Name, Value = dto.Value, Path = dto.Path };

    private static List<Cookie> MergeCookies(List<Cookie> cookiesOld, CookieCollection cookiesNew) {
        foreach (Cookie cookie in cookiesNew) {
            cookiesOld.RemoveAll(e => e.Name == cookie.Name);
            cookiesOld.Add(cookie);
        }
        return cookiesOld;
    }

    private static List<Cookie> ToList(CookieCollection cookies) => cookies.Cast<Cookie>().ToList();

    private static CookieCollection ToCookieCollection(List<Cookie> cookies) {
        var result = new CookieCollection();
        foreach (Cookie cookie in cookies) {
            result.Add(cookie);
        }
        return result;
    }

    #endregion [ Cookies ]

    #region [ http ]

    private class HttpResult {
        public string Data { get; set; }
        public CookieCollection Cookies { get; set; }
    }

    /// <summary>
    ///     Запрос с переданными cookies (домен — хост запроса, как в V1); в ответе — тело и все cookies контейнера </summary>
    private static async Task<HttpResult> SendAsync(string url, string form, CookieCollection cookies, Action<HttpRequestMessage> configure = null) {
        var uri = new Uri(url);
        var container = new CookieContainer();
        foreach (Cookie cookie in cookies ?? new CookieCollection()) {
            container.Add(new Cookie(cookie.Name, cookie.Value, String.IsNullOrEmpty(cookie.Path) ? "/" : cookie.Path, uri.Host));
        }
        using var handler = new HttpClientHandler { CookieContainer = container, UseCookies = true, AllowAutoRedirect = true };
        using var http = new HttpClient(handler) { Timeout = AppSettings.ExternalTimeout };
        using var request = new HttpRequestMessage(form == null ? HttpMethod.Get : HttpMethod.Post, uri);
        request.Headers.UserAgent.ParseAdd(UserAgent);
        if (form != null) {
            request.Content = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded");
        }
        configure?.Invoke(request);
        using var response = await http.SendAsync(request);
        var html = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode == false) {
            throw new Exception($"{(int)response.StatusCode} {url}");
        }
        var result = new CookieCollection();
        foreach (Cookie cookie in container.GetAllCookies()) {
            result.Add(cookie);
        }
        return new HttpResult { Data = html, Cookies = result };
    }

    #endregion [ http ]

    #region [ parse ]

    private static List<BnsfRailwayTrailerDto> JsonToResult(string json) {
        var result = new List<BnsfRailwayTrailerDto>();
        var obj = (dynamic)JsonConvert.DeserializeObject(json);
        if (obj.errcode != null && obj.message != null) {
            throw new Exception($"[ trailers ] RequestActionServlet\nerrcode:{obj.errcode}, message:{obj.message}");
        }
        foreach (var item in obj.DATA_VIEW_MAIN_DATA) {
            result.Add(new BnsfRailwayTrailerDto() {
                Owner = (string)item["60"],
                Shipper = (string)item["57"],
                Unit = (string)item["1"],
                LoadEmptyStatus = (string)item["13"],
                CurrentLocation = (string)item["4"],
                EventCode = (string)item["5"],
                EventDate = (string)item["6"],
                ETA_Deramp = (string)item["11"],
                Origin = (string)item["2"],
                Destination = (string)item["3"],
                BeyondGoalHours = (string)item["110"],
                TimeDeramp = (string)item["40"],
                TimeNotify = (string)item["121"],
                FinalTrainArrival = (string)item["75"],
                GoalDate = (string)item["14"],
                Nextevent = (string)item["83"],
                PickUpNumber = (string)item["12"],
                StorageDay = (string)item["29"],
                Train = (string)item["7"],
                Trucker = (string)item["8"],
                WaybillDate = (string)item["80"],
                RateAuthority = (string)item["41"],
            });
        }
        return result;
    }

    private static void SetAccounting(List<BnsfRailwayTrailerDto> trailers, string html) {
        if (String.IsNullOrEmpty(html)) return;
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);
        var table = doc.GetElementbyId("scrollContent");
        var trList = table.SelectNodes("tr/tr")
            .GroupBy(e => e.SelectNodes("td")[3].InnerText)
            .Select(e1 => e1.OrderBy(e2 => DateTime.Parse(e2.SelectNodes("td")[5].InnerText)).Last())
            .ToList();
        foreach (var tr in trList) {
            var tdList = tr.SelectNodes("td");
            foreach (var trailer in trailers) {
                if (CompareByWords(trailer.Unit, tdList[3].InnerText)) {
                    // NOTES: tdList[5] is waybill date
                    trailer.AmountDue = tdList[1].InnerText;
                    trailer.DueDate = tdList[2].InnerText;
                    trailer.WaybillNumber = tdList[10].InnerText;
                    trailer.BillNumber = tdList[11].InnerText;
                    trailer.Adjustments = tdList[16].InnerText;
                    break;
                }
            }
        }
    }

    private static void JsonSetLocations(List<BnsfRailwayTrailerDto> trailers, string json) {
        var mappables = JsonConvert.DeserializeObject(((dynamic)JsonConvert.DeserializeObject(json))
            .mapLayerData.Value.Replace("\\,", ",")).layers[0].dataSeries[0].mappables;
        foreach (var trailer in trailers) {
            /*CICERO,IL*/
            var name1 = new String(trailer.CurrentLocation.Where(Char.IsLetter).ToArray());
            foreach (var mappable in mappables) {
                /*"name" : "CHICAGO  IL&...*/
                var name2 = new String(
                    Regex.Match((string)mappable.name, @"^(?<name>[\w\s]+)&").Groups["name"].Value.Where(Char.IsLetter).ToArray());
                if (name1 == name2) {
                    trailer.Latitude = (float)mappable.latitude;
                    trailer.Longitude = (float)mappable.longitude;
                    break;
                }
            }
        }
    }

    /// <summary>
    ///     BAPQ        119 and BAPQ 119 </summary>
    private static bool CompareByWords(string val1, string val2) {
        var str1 = new String(val1.Where(e => Char.IsDigit(e) || Char.IsLetter(e)).ToArray());
        var str2 = new String(val2.Where(e => Char.IsDigit(e) || Char.IsLetter(e)).ToArray());
        return str1 == str2;
    }

    #endregion [ parse ]

    #region [ Main Door ]

    private static Task<HttpResult> RequestGetLoginPage() {
        return SendAsync("https://idp.bnsf.com/bnsfauth/LoginPage", null, null);
    }

    /// <summary>
    ///     Data — am-url-encoded </summary>
    private static async Task<HttpResult> RequestPostIdpDiscovery(string login, CookieCollection cookies) {
        var result = await SendAsync("https://idp.bnsf.com/bnsfauth/IdpDiscovery",
            $"idpSelect=bnsfcust&userid={login}&isForgotUid=false&isForgotPwd=false&submitButton=", cookies);
        result.Data = Regex.Match(result.Data, @"""am-url-encoded""[\s]+value=""(?<key>[\d\w=]+)""").Groups["key"].Value;
        return result;
    }

    /// <summary>
    ///     Data — SAMLResponse </summary>
    private static async Task<HttpResult> RequestPostLogin(string amUrlEncoded, string login, string password, CookieCollection cookies) {
        var result = await SendAsync("https://custidp.bnsf.com/bnsfauth/Login",
            $"userid={login}&password={Uri.EscapeDataString(password ?? "")}&login=login&am-url-encoded={amUrlEncoded}&detailText=", cookies);
        var match = Regex.Match(result.Data, @"""SAMLResponse""[\s]+value=""(?<key>[\d\w\s+=]+)""");
        if (match.Success == false) {
            throw new Exception("unauthorized");
        }
        result.Data = match.Groups["key"].Value;
        return result;
    }

    private static Task<HttpResult> RequestPostSaml20Login(string samlResponse, CookieCollection cookies) {
        return SendAsync("https://sp.bnsf.com/sps/bnsfsp/saml20/login",
            $"RelayState=https%3A%2F%2Fcustomer.bnsf.com%2F&SAMLResponse={Uri.EscapeDataString(samlResponse)}", cookies);
    }

    private static Task<HttpResult> RequestGetRailcentralServlet(CookieCollection cookies) {
        return SendAsync("https://www.bnsf.com/bnsf.was6/railcentral/RailcentralServlet?mode=clslist", null, cookies);
    }

    /// <summary>
    ///     Id отчёта с именем «AutoCall» в Rail Central </summary>
    private static async Task<string> RequestReportList(CookieCollection cookies, string dataPost) {
        var json = await RequestActionServlet(cookies, dataPost);
        dynamic obj = JsonConvert.DeserializeObject(json);
        if ((int)obj.Count == 0) {
            throw new Exception("Report was not found");
        }
        foreach (var report in obj) {
            if (((string)report.DESCRIPTION).ToLower().Trim() == "autocall") {
                return report.ID.ToString();
            }
        }
        throw new Exception("Please create a report in 'Rail Central' (https://bnsf.com/) with name 'AutoCall'.");
    }

    private static async Task<string> RequestActionServlet(CookieCollection cookies, string dataPost) {
        var result = await SendAsync("https://www.bnsf.com/bnsf.was6/railcentral/ActionServlet", dataPost, cookies);
        var html = result.Data;
        var result1 = Regex.Match(html, @"""appTitle""\>(?<title>[\d\w\s+]+)\<");
        if (result1.Success) {
            throw new Exception(result1.Groups["title"].Value);
        }
        var result2 = Regex.Match(html, @"^{""message"":""(?<message>[\d\w\s+.:]+)""");
        if (result2.Success) {
            throw new Exception(result2.Groups["message"].Value);
        }
        return html;
    }

    #endregion [ Main Door ]

    #region [ accounting ]

    /// <summary>
    ///     Data — Patron </summary>
    private static async Task<HttpResult> RequestEPayController1(CookieCollection cookies) {
        var result = await SendAsync("https://www.bnsf.com/bnsf.was/epay/EPayController", null, cookies, request => {
            request.Headers.Accept.ParseAdd("*/*");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        });
        if (result.Data.Contains("<title>BNSF Login Required</title>")) {
            throw new Exception("BNSF Login Required");
        }
        result.Data = Regex.Match(result.Data, @"NAME=""Patron""[\s]+value=""(?<value>[\d]+)""").Groups["value"].Value;
        return result;
    }

    private static async Task<List<string>> RequestEPayController2(string patron, CookieCollection cookies) {
        var result = await SendAsync("https://www.bnsf.com/bnsf.was/epay/EPayController",
            $"EPayments=True&reqPage=openpayables&Action=openpayables&SelectIndx=&SearchString=&view=AO&Patron={patron}&CoCd=02&searchFor=", cookies);
        var matches = Regex.Matches(result.Data, @"var AccountArray[\d]+ = new Array\(""[\d.]+"",""(?<date>[\d-]+)"",""[\d]+"",""[\d]+""\);");
        return matches.Cast<Match>().Select(e => e.Groups["date"].Value).ToList();
    }

    private static async Task<string> RequestEPayController3(List<string> dates, CookieCollection cookies) {
        var data = $"EPayments=True&reqPage=opendetail&Action=&Mode=&SearchString=&GroupsStringForPDF=&BillsStringForDetail={String.Join("", dates.Select(e => $"{e}{Uri.EscapeDataString("#")}"))}&view=AO&group=BD&page=&sortView=&sortCol=&sort=&sort_Col=&sort_ord=&checkedName={Uri.EscapeDataString("1#2#3#4#")}&partialPay=No&ABS_URL={Uri.EscapeDataString("https://www.bnsf.com/bnsf.was/epay/EPayController")}";
        var result = await SendAsync("https://www.bnsf.com/bnsf.was/epay/EPayController", data, cookies);
        return result.Data;
    }

    #endregion [ accounting ]
}

/// <summary>
///     V1 Converters/Dispatch/BnsfTrailer/Converter: перенос полей отчёта BNSF в трейлер </summary>
public static class BnsfTrailerConverter {
    public static bool Set(BnsfRailwayTrailerDto bnsfDto, TrailerDto trailerDto, out bool changePickUpNumber) {
        if (
            trailerDto.BnsfOwner != bnsfDto.Owner ||
            trailerDto.BnsfShipper != bnsfDto.Shipper ||
            trailerDto.BnsfUnit != bnsfDto.Unit ||
            trailerDto.BnsfLoadEmptyStatus != bnsfDto.LoadEmptyStatus ||
            trailerDto.BnsfCurrentLocation != bnsfDto.CurrentLocation ||
            trailerDto.BnsfEventCode != bnsfDto.EventCode ||
            trailerDto.BnsfEventDate != bnsfDto.EventDate ||
            trailerDto.BnsfETA_Deramp != bnsfDto.ETA_Deramp ||
            trailerDto.BnsfOrigin != bnsfDto.Origin ||
            trailerDto.BnsfDestination != bnsfDto.Destination ||
            trailerDto.BnsfBeyondGoalHours != bnsfDto.BeyondGoalHours ||
            trailerDto.BnsfTimeDeramp != bnsfDto.TimeDeramp ||
            trailerDto.BnsfTimeNotify != bnsfDto.TimeNotify ||
            trailerDto.BnsfFinalTrainArrival != bnsfDto.FinalTrainArrival ||
            trailerDto.BnsfGoalDate != bnsfDto.GoalDate ||
            trailerDto.BnsfNextevent != bnsfDto.Nextevent ||
            trailerDto.BnsfPickUpNumber != bnsfDto.PickUpNumber ||
            trailerDto.BnsfStorageDay != bnsfDto.StorageDay ||
            trailerDto.BnsfTrain != bnsfDto.Train ||
            trailerDto.BnsfTrucker != bnsfDto.Trucker ||
            trailerDto.BnsfWaybillDate != bnsfDto.WaybillDate ||
            trailerDto.BnsfRateAuthority != bnsfDto.RateAuthority ||
            trailerDto.BnsfAmountDue != bnsfDto.AmountDue ||
            trailerDto.BnsfDueDate != bnsfDto.DueDate ||
            trailerDto.BnsfWaybillNumber != bnsfDto.WaybillNumber ||
            trailerDto.BnsfBillNumber != bnsfDto.BillNumber ||
            trailerDto.BnsfAdjustments != bnsfDto.Adjustments) {
            changePickUpNumber = trailerDto.BnsfPickUpNumber != bnsfDto.PickUpNumber;

            trailerDto.BnsfOwner = bnsfDto.Owner;
            trailerDto.BnsfShipper = bnsfDto.Shipper;
            trailerDto.BnsfUnit = bnsfDto.Unit;
            trailerDto.BnsfLoadEmptyStatus = bnsfDto.LoadEmptyStatus;
            trailerDto.BnsfCurrentLocation = bnsfDto.CurrentLocation;
            trailerDto.BnsfEventCode = bnsfDto.EventCode;
            trailerDto.BnsfEventDate = bnsfDto.EventDate;
            trailerDto.BnsfETA_Deramp = bnsfDto.ETA_Deramp;
            trailerDto.BnsfOrigin = bnsfDto.Origin;
            trailerDto.BnsfDestination = bnsfDto.Destination;
            trailerDto.BnsfBeyondGoalHours = bnsfDto.BeyondGoalHours;
            trailerDto.BnsfTimeDeramp = bnsfDto.TimeDeramp;
            trailerDto.BnsfTimeNotify = bnsfDto.TimeNotify;
            trailerDto.BnsfFinalTrainArrival = bnsfDto.FinalTrainArrival;
            trailerDto.BnsfGoalDate = bnsfDto.GoalDate;
            trailerDto.BnsfNextevent = bnsfDto.Nextevent;
            trailerDto.BnsfPickUpNumber = bnsfDto.PickUpNumber;
            trailerDto.BnsfStorageDay = bnsfDto.StorageDay;
            trailerDto.BnsfTrain = bnsfDto.Train;
            trailerDto.BnsfTrucker = bnsfDto.Trucker;
            trailerDto.BnsfWaybillDate = bnsfDto.WaybillDate;
            trailerDto.BnsfRateAuthority = bnsfDto.RateAuthority;
            trailerDto.BnsfAmountDue = bnsfDto.AmountDue;
            trailerDto.BnsfDueDate = bnsfDto.DueDate;
            trailerDto.BnsfWaybillNumber = bnsfDto.WaybillNumber;
            trailerDto.BnsfBillNumber = bnsfDto.BillNumber;
            trailerDto.BnsfAdjustments = bnsfDto.Adjustments;
            return true;
        }
        changePickUpNumber = false;
        return false;
    }

    public static bool IsEmpty(TrailerDto trailerDto) {
        return
            String.IsNullOrEmpty(trailerDto.BnsfOwner) &&
            String.IsNullOrEmpty(trailerDto.BnsfShipper) &&
            String.IsNullOrEmpty(trailerDto.BnsfLoadEmptyStatus) &&
            String.IsNullOrEmpty(trailerDto.BnsfCurrentLocation) &&
            String.IsNullOrEmpty(trailerDto.BnsfEventCode) &&
            String.IsNullOrEmpty(trailerDto.BnsfEventDate) &&
            String.IsNullOrEmpty(trailerDto.BnsfETA_Deramp) &&
            String.IsNullOrEmpty(trailerDto.BnsfOrigin) &&
            String.IsNullOrEmpty(trailerDto.BnsfDestination) &&
            String.IsNullOrEmpty(trailerDto.BnsfBeyondGoalHours) &&
            String.IsNullOrEmpty(trailerDto.BnsfTimeDeramp) &&
            String.IsNullOrEmpty(trailerDto.BnsfTimeNotify) &&
            String.IsNullOrEmpty(trailerDto.BnsfFinalTrainArrival) &&
            String.IsNullOrEmpty(trailerDto.BnsfGoalDate) &&
            String.IsNullOrEmpty(trailerDto.BnsfNextevent) &&
            String.IsNullOrEmpty(trailerDto.BnsfPickUpNumber) &&
            String.IsNullOrEmpty(trailerDto.BnsfStorageDay) &&
            String.IsNullOrEmpty(trailerDto.BnsfTrain) &&
            String.IsNullOrEmpty(trailerDto.BnsfTrucker) &&
            String.IsNullOrEmpty(trailerDto.BnsfRateAuthority);
    }

    public static void SetEmpty(TrailerDto trailerDto) {
        trailerDto.BnsfOwner = null;
        trailerDto.BnsfShipper = null;
        trailerDto.BnsfLoadEmptyStatus = null;
        trailerDto.BnsfCurrentLocation = null;
        trailerDto.BnsfEventCode = null;
        trailerDto.BnsfEventDate = null;
        trailerDto.BnsfETA_Deramp = null;
        trailerDto.BnsfOrigin = null;
        trailerDto.BnsfDestination = null;
        trailerDto.BnsfBeyondGoalHours = null;
        trailerDto.BnsfTimeDeramp = null;
        trailerDto.BnsfTimeNotify = null;
        trailerDto.BnsfFinalTrainArrival = null;
        trailerDto.BnsfGoalDate = null;
        trailerDto.BnsfNextevent = null;
        trailerDto.BnsfPickUpNumber = null;
        trailerDto.BnsfStorageDay = null;
        trailerDto.BnsfTrain = null;
        trailerDto.BnsfTrucker = null;
        trailerDto.BnsfRateAuthority = null;
        // пусть всегда отображаются последние: AmountDue, DueDate, WaybillNumber, BillNumber, Adjustments, Unit, WaybillDate
    }
}
