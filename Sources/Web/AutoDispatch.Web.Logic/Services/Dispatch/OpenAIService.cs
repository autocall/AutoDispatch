using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using LinqToDB;
using LinqToDB.Async;
using Newtonsoft.Json;
using SkiaSharp;
using UglyToad.PdfPig;
using AutoCall.Common.Enums;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.WebServices;
using AutoDispatch.Web.Logic.WebServices.OpenAI;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Разбор rate confirmation (PDF) в LoadDto через OpenAI chat/completions (V1 OpenAIService).
///     PDF→текст — PdfPig, PDF→PNG — PDFtoImage (pdfium) вместо Telerik/Spire/Magick.NET (AI/wiki/plan.md); логи — AppSettings.OpenAiLogsPath </summary>
public class OpenAIService : DispatchBaseService {

    private OpenAIWebService WebService => Host.GetService<OpenAIWebService>();

    public OpenAIService(ServicesHost host) : base(host) { }

    #region [ PDF ]

    /// <summary>
    ///     Текст PDF со страницами «----------- PageN -----------» (формат Telerik TextFormatProvider V1, на него завязан GetLengthByPage) </summary>
    public string PdfToString(Stream stream) {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        using var document = PdfDocument.Open(ms.ToArray());
        var sb = new StringBuilder();
        foreach (var page in document.GetPages()) {
            sb.AppendLine($"----------- Page{page.Number} -----------");
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }

    public Task<List<byte[]>> PdfToPngAsync(Stream inputStream) {
        return Task.Run(() => {
            var result = new List<byte[]>();
            foreach (SKBitmap bitmap in PDFtoImage.Conversion.ToImages(inputStream, leaveOpen: true)) {
                using (bitmap) {
                    using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                    result.Add(data.ToArray());
                }
            }
            return result;
        });
    }

    /// <summary>
    ///     Сохраняет PNG в AppSettings.TempPath, возвращает имена файлов (для URL TempRequestPath/{name}) </summary>
    public async Task<List<string>> SaveImagesToTempAsync(List<byte[]> binaries) {
        var result = new List<string>();
        Directory.CreateDirectory(AppSettings.TempPath);
        foreach (byte[] binaryData in binaries) {
            var fileName = $"{Guid.NewGuid()}.png";
            await BinaryHelper.WriteAllBytesAsync(Path.Combine(AppSettings.TempPath, fileName), binaryData);
            result.Add(fileName);
        }
        return result;
    }

    #endregion [ PDF ]

    #region [ Request ]

    public async Task<LoadDto> RequestAsync(string fileName, string documentText, DispatchBoardType board, Guid companyId) {
        var log = new List<string>();
        try {
            log.Add($"-- Document --");
            log.Add($"-- FileName: {fileName}");
            log.Add($"-- DateTime: {DateTime.Now:f}");
            log.Add(documentText);
            if (documentText.Count(ch => Char.IsLetterOrDigit(ch)) < 100) {
                throw new Exception("invalid content document");
            }
            log.Add($"-- Pages --");
            var pages = this.GetLengthByPage(documentText);
            log.Add(String.Join("\n", pages.Select(e => $"Page {e.Key}: {e.Value}")));

            documentText = OptimizeDocument(pages, documentText);

            var requestData = ConvertToRequestTextData(documentText);
            return await RequestCoreAsync(requestData, board, companyId, log);
        } catch (Exception e) {
            log.Add($"-- Exception --");
            log.Add(e.ToString());
            throw;
        } finally {
            WriteLog(log);
        }
    }

    public async Task<LoadDto> RequestAsync(List<string> urlFiles, DispatchBoardType board, Guid companyId) {
        var log = new List<string>();
        try {
            if (urlFiles.Count == 0) {
                throw new Exception($"Current pages: {urlFiles.Count}");
            }
            var maxFiles = 5;
            if (urlFiles.Count > 5) {
                throw new Exception($"Max pages: {maxFiles}. Current pages: {urlFiles.Count}");
            }
            log.Add($"-- Document --");
            foreach (var url in urlFiles) {
                log.Add($"-- FileName: {url}");
            }
            log.Add($"-- DateTime: {DateTime.Now:f}");
            log.Add($"-- Pages --");

            RequestImageData requestData = ConvertToRequestImageData(urlFiles);
            return await RequestCoreAsync(requestData, board, companyId, log);
        } catch (Exception e) {
            log.Add($"-- Exception --");
            log.Add(e.ToString());
            throw;
        } finally {
            WriteLog(log);
        }
    }

    /// <summary>
    ///     Общая часть двух RequestAsync V1: запрос, проверки ответа, разбор в MappedLoad и LoadDto </summary>
    private async Task<LoadDto> RequestCoreAsync(RequestData requestData, DispatchBoardType board, Guid companyId, List<string> log) {
        log.Add($"-- Request Data --");
        log.Add(DispatchJson.Serialize(requestData, TypeNameHandling.None));
        ApiResult<ResponseData> response = await WebService.RequestAsync(requestData);
        if (response.Success == false) {
            throw new Exception(response.ErrorMessage);
        }
        if (response.Data == null) {
            throw new Exception("Data is NULL");
        }
        if (response.Data.choices.Length == 0) {
            throw new Exception("Data.choices is empty");
        }
        log.Add($"--Usage: {response.Data.usage.total_tokens}-{response.Data.usage.completion_tokens}={response.Data.usage.prompt_tokens}");
        if (response.Data.choices.Length > 1) {
            log.Add($"-- Warning --");
            log.Add($".choices.Length: {response.Data.choices.Length}");
            log.Add($"-- Response Json --");
            log.Add(response.Data.ContentJson);
        }
        var content = response.Data.choices.First().message.content;
        if (CheckContent(content, out var tmp) == false) {
            log.Add(tmp);
            throw new Exception("document was not recognized [1]");
        }
        log.Add(tmp);
        log.Add($"-- Response Data --");
        log.Add(content);
        MappedLoad mappedLoad = ParseMappedLoad(content);
        if (CheckContent(mappedLoad, out tmp) == false) {
            log.Add(tmp);
            throw new Exception("document was not recognized [2]");
        }
        log.Add($"-- Response Mapped Load --");
        log.Add(ConvertToString(mappedLoad));
        return await ConvertAsync(mappedLoad, board, companyId);
    }

    private bool CheckContent(string content, out string log) {
        log = $"Count '[', ']': {content.Count(ch => ch == '[' || ch == ']')}";
        if (content.Count(ch => ch == '[' || ch == ']') >= 40) {
            return false;
        }
        return true;
    }

    private bool CheckContent(MappedLoad mappedLoad, out string log) {
        var countExists = mappedLoad.GetType()
            .GetProperties()
            .Where(p => p.GetCustomAttributes(false).Any(a => a is DescriptionAttribute))
            .Count(e => CheckExists((string)e.GetValue(mappedLoad)));
        if (countExists >= 15) {
            log = null;
            return true;
        } else {
            log = $"Count Exist Fields: {countExists}";
            return false;
        }
    }

    /// <summary>
    ///     Gets pages </summary>
    /// <returns>Page Number -> Page Length</returns>
    private Dictionary<string, int> GetLengthByPage(string content) {
        Dictionary<string, int> result = new Dictionary<string, int>();
        var matches = Regex.Matches(content, @"----------- Page(?<page>[\d]+) -----------");
        foreach (var match in matches.Cast<Match>().Reverse()) {
            var index = content.IndexOf(match.Value);
            result[match.Groups["page"].Value] = content.Length - index;
            content = content.Substring(0, index);
        }
        return result.OrderBy(e => e.Key).ToDictionary(e => e.Key, e => e.Value);
    }

    /// <summary>
    ///     Page 1: 1671
    ///     Page 2: 1700
    ///     Page 3: 5022 remove
    /// </summary>
    private string OptimizeDocument(Dictionary<string, int> pages, string content) {
        // удаляю с конца все страницы больше 4000 символов
        for (int i = pages.Count - 1; i >= 0; i--) {
            var list = pages.ToList();
            if (list[i].Value > 4000) {
                content = content.Substring(0, content.Length - list[i].Value);
            }
        }
        return content;
    }

    private void WriteLog(List<string> log) {
        try {
            var path = AppSettings.OpenAiLogsPath;
            Directory.CreateDirectory(path);
            File.WriteAllLines(Path.Combine(path, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.log"), log);
        } catch (Exception e) {
            _l.e("OpenAI WriteLog", e);
        }
    }

    private bool CheckExists(string value) {
        if (String.IsNullOrWhiteSpace(value)) {
            return false;
        }
        if (value == "N/A") {
            return false;
        }
        if (value == "-") {
            return false;
        }
        if (value.StartsWith("-")) {
            return false;
        }
        return true;
    }

    #endregion [ Request ]

    #region [ Parse ]

    public bool TryParseDateTime(string str, out DateTime dt) {
        CultureInfo enUS = new CultureInfo("en-US");
        if (DateTime.TryParseExact(str, "g", enUS, DateTimeStyles.None, out dt)) {
            dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, DateTimeKind.Utc);
            return true;
        }
        string datePattern = @"^(?<month>\d{1,2})\/(?<day>\d{1,2})\/(?<year>\d{4}) (?<hour>\d{1,2}):(?<minute>\d{2})";
        var match = Regex.Match(str, datePattern);
        if (match.Success) {
            var year = Int32.Parse(match.Groups["year"].Value);
            var month = Int32.Parse(match.Groups["month"].Value);
            var day = Int32.Parse(match.Groups["day"].Value);
            var hour = Int32.Parse(match.Groups["hour"].Value);
            var minute = Int32.Parse(match.Groups["minute"].Value);
            dt = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
            return true;
        }
        dt = default(DateTime);
        return false;
    }

    /// <summary>
    ///     «City, Texas» → «City, TX» (V1 Context.StatesAbbr → StateHelper.States) </summary>
    public string ToShortState(string cityState) {
        foreach (var state in StateHelper.States) {
            if (cityState.IndexOf(state.Name, StringComparison.CurrentCultureIgnoreCase) != -1) {
                return Regex.Replace(cityState, state.Name, state.Code, RegexOptions.IgnoreCase);
            }
        }
        return cityState;
    }

    public string OptimizeZip(string cityStateZip) {
        var pattern = @"(?<zip>\d{3,7})(?<extrazip>-\d{3,7})$";
        var match = Regex.Match(cityStateZip, pattern);
        if (match.Success) {
            return cityStateZip.Replace(match.Groups["extrazip"].Value, String.Empty);
        }
        return cityStateZip;
    }

    /// <summary>
    ///     (800) 227-9558 x4172 to 8002279558 and 4172 </summary>
    public bool TryParsePhone(string contact, out string phone, out string ext) {
        string pattern = @"^(?<phone>(\(\d{3}\) \d{3}-\d{4}))(?<ext>.+\d{1,})?";
        var match = Regex.Match(contact, pattern);
        if (match.Success) {
            phone = match.Groups["phone"].Value.OnlyNumbers();
            ext = match.Groups["ext"].Success ? match.Groups["ext"].Value.OnlyNumbers() : null;
            return true;
        } else {
            phone = null;
            ext = null;
            return false;
        }
    }

    private async Task<LoadDto> ConvertAsync(MappedLoad origin, DispatchBoardType board, Guid companyId) {
        var destination = new LoadDto() {
            BookedDate = DateTime.Now,
            PickupFrom = default(DateTime),
            PickupTo = default(DateTime),
            DeliveryFrom = default(DateTime),
            DeliveryTo = default(DateTime),
            CompanyId = companyId,
            BoardType = board,
            BoardChanged = DateTime.UtcNow
        };
        // LoadId
        if (CheckExists(origin.LoadId)) {
            destination.RecordId = origin.LoadId;
        }
        // OriginStreet
        if (CheckExists(origin.OriginStreet)) {
            destination.OriginAddress = origin.OriginStreet;
        }
        // DestinationStreet
        if (CheckExists(origin.DestinationStreet)) {
            destination.DestinationAddress = origin.DestinationStreet;
        }
        // Origin
        if (CheckExists(origin.Origin)) {
            destination.OriginCity = this.OptimizeZip(this.ToShortState(origin.Origin));
        }
        // Destination
        if (CheckExists(origin.Destination)) {
            destination.DestinationCity = this.OptimizeZip(this.ToShortState(origin.Destination));
        }
        // PickupDateFrom
        if (CheckExists(origin.PickupDateFrom)) {
            if (this.TryParseDateTime(origin.PickupDateFrom, out var dateTime)) {
                destination.PickupFrom = dateTime;
            }
        }
        // PickupDateTo
        if (CheckExists(origin.PickupDateTo)) {
            if (this.TryParseDateTime(origin.PickupDateTo, out var dateTime)) {
                destination.PickupTo = dateTime;
            }
        }
        // DeliveryDateFrom
        if (CheckExists(origin.DeliveryDateFrom)) {
            if (this.TryParseDateTime(origin.DeliveryDateFrom, out var dateTime)) {
                destination.DeliveryFrom = dateTime;
            }
        }
        // DeliveryDateTo
        if (CheckExists(origin.DeliveryDateTo)) {
            if (this.TryParseDateTime(origin.DeliveryDateTo, out var dateTime)) {
                destination.DeliveryTo = dateTime;
            }
        }
        // PickupFrom -> PickupTo
        if (destination.PickupFrom != default(DateTime) && destination.PickupTo == default(DateTime)) {
            destination.PickupTo = destination.PickupFrom;
        }
        // PickupTo -> PickupFrom
        if (destination.PickupFrom == default(DateTime) && destination.PickupTo != default(DateTime)) {
            destination.PickupFrom = destination.PickupTo;
        }
        // DeliveryFrom -> DeliveryTo
        if (destination.DeliveryFrom != default(DateTime) && destination.DeliveryTo == default(DateTime)) {
            destination.DeliveryTo = destination.DeliveryFrom;
        }
        // DeliveryTo -> DeliveryFrom
        if (destination.DeliveryFrom == default(DateTime) && destination.DeliveryTo != default(DateTime)) {
            destination.DeliveryFrom = destination.DeliveryTo;
        }
        // ShipperName
        if (CheckExists(origin.ShipperName)) {
            destination.ShipperCompany = origin.ShipperName;
        }
        // ReceiverName
        if (CheckExists(origin.ReceiverName)) {
            destination.ReceiverCompany = origin.ReceiverName;
        } else
        if (CheckExists(origin.ConsigneeName)) {
            destination.ReceiverCompany = origin.ConsigneeName;
        }
        // ShipperPhone
        if (CheckExists(origin.ShipperPhone)) {
            if (this.TryParsePhone(origin.ShipperPhone, out var phone, out var ext)) {
                destination.ShipperContacts = DatCompanyContactHelper.CreateAndGetPhone(phone, ext, null);
            }
        }
        // ReceiverPhone
        if (CheckExists(origin.ReceiverPhone)) {
            if (this.TryParsePhone(origin.ReceiverPhone, out var phone, out var ext)) {
                destination.ReceiverContacts = DatCompanyContactHelper.CreateAndGetPhone(phone, ext, null);
            }
        }
        // PickupId
        var pickupIds = new List<string>();
        if (CheckExists(origin.PickupId) && origin.DeliveryId.Length > 2) { // V1: проверка DeliveryId — как в оригинале
            pickupIds.Add(origin.PickupId);
        }
        if (CheckExists(origin.PickupId2) && origin.PickupId2.Length > 2) {
            pickupIds.Add(origin.PickupId2);
        }
        if (CheckExists(origin.PickupId3) && origin.PickupId3.Length > 2) {
            pickupIds.Add(origin.PickupId3);
        }
        if (pickupIds.Any()) {
            destination.PickupNumber = String.Join(", ", pickupIds.Distinct());
        }
        // DeliveryId
        var deliveryIds = new List<string>();
        if (CheckExists(origin.DeliveryId) && origin.DeliveryId.Length > 2) {
            deliveryIds.Add(origin.DeliveryId);
        }
        if (CheckExists(origin.DeliveryId2) && origin.DeliveryId2.Length > 2) {
            deliveryIds.Add(origin.DeliveryId2);
        }
        if (CheckExists(origin.DeliveryId3) && origin.DeliveryId3.Length > 2) {
            deliveryIds.Add(origin.DeliveryId3);
        }
        if (deliveryIds.Any()) {
            destination.DeliveryNumber = String.Join(", ", deliveryIds.Distinct());
        }
        // BrokerName
        if (CheckExists(origin.BrokerName)) {
            destination.DatBrokerCompany = origin.BrokerName;
        }
        // BrokerMc
        if (CheckExists(origin.BrokerMc)) {
            var mcString = origin.BrokerMc.ToLower().Replace("mc", "").Trim();
            if (Int32.TryParse(mcString, out var mc)) {
                if (1 < mc && mc < 99999999) {
                    destination.McNumber = mc;
                }
            }
        }
        // BrokerContactPhone & BrokerContactName
        if (CheckExists(origin.BrokerContactPhone)) {
            if (this.TryParsePhone(origin.BrokerContactPhone, out var phone, out var ext)) {
                destination.Contacts = DatCompanyContactHelper.CreateAndGetPhone(phone, ext, origin.BrokerContactName);
            }
        }
        // Price
        if (CheckExists(origin.Price)) {
            var priceString = origin.Price.OnlyDecimal();
            if (Decimal.TryParse(priceString, out var price)) {
                if (10 < price && price < 999999) {
                    destination.LineHaul = price;
                }
            }
        }
        // Length
        destination.Length = 53;
        if (CheckExists(origin.Length)) {
            var lengthString = origin.Length.OnlyDecimal();
            if (Decimal.TryParse(lengthString, out var length)) {
                if (0 < length && length < 99) {
                    destination.Length = (int)length;
                }
            }
        }
        if (destination.Length == null && CheckExists(origin.Length2)) {
            var lengthString = origin.Length2.OnlyDecimal();
            if (Decimal.TryParse(lengthString, out var length)) {
                if (0 < length && length < 99) {
                    destination.Length = (int)length;
                }
            }
        }
        // Weight
        if (CheckExists(origin.Weight)) {
            var weightString = origin.Weight.OnlyDecimal();
            if (Decimal.TryParse(weightString, out var weight)) {
                if (0 < weight && weight < 99) {
                    destination.Weight = (int)weight;
                }
                if (999 < weight && weight < 99999) {
                    destination.Weight = (int)Math.Round((double)weight / 1000);
                }
            }
        }
        if (destination.Weight == null && CheckExists(origin.Weight2)) {
            var weightString = origin.Weight2.OnlyDecimal();
            if (Decimal.TryParse(weightString, out var weight)) {
                if (0 < weight && weight < 99) {
                    destination.Weight = (int)weight;
                }
                if (999 < weight && weight < 99999) {
                    destination.Weight = (int)Math.Round((double)weight / 1000);
                }
            }
        }
        // Pallets
        if (CheckExists(origin.Pallets)) {
            var palletsString = origin.Pallets.OnlyDecimal();
            if (Int16.TryParse(palletsString, out var pallets)) {
                if (0 < pallets && pallets <= 100) {
                    destination.Pallets = pallets;
                }
            }
        }
        // Pieces
        if (destination.Pallets == null && CheckExists(origin.Pieces)) {
            var piecesString = origin.Pieces.OnlyDecimal();
            if (Int16.TryParse(piecesString, out var pieces)) {
                if (0 < pieces && pieces <= 10000) {
                    destination.Pallets = pieces;
                    destination.PalletType = 2; // PalletType.FloorLoaded
                }
            }
        }
        // Equipment
        if (CheckExists(origin.Equipment)) {
            if (origin.Equipment == "Van") {
                destination.Equipment = (byte)AutoCallEquipmentType.Van;
            }
            if (origin.Equipment == "Refeer") {
                destination.Equipment = (byte)AutoCallEquipmentType.Reefer;
            }
        }
        // ActualCompany
        if (CheckExists(origin.CarrierName)) {
            var dispatchCompanies = await Alive<DispatchCompany>().Where(e => e.CompanyId == companyId && e.BoardType == (byte)board).ToListAsync();
            var found = dispatchCompanies
                .Where(e => e.LegalName.OnlyLetters().IndexOf(origin.CarrierName.OnlyLetters(), StringComparison.OrdinalIgnoreCase) != -1)
                .ToList();
            if (found.Count == 1) {
                destination.ActualCompanyId = found.First().EntityId;
            }
        }
        return destination;
    }

    private MappedLoad ParseMappedLoad(string content) {
        var mappedLoad = new MappedLoad();
        var contentLines = content.Split('\n');
        var contentDictionary = new Dictionary<string, string>();
        foreach (var line in contentLines) {
            var index = line.IndexOf(':');
            if (index != -1) {
                var key = line.Substring(0, index);
                if (index + 2 < line.Length) {
                    var value = line.Substring(index + 2).Trim();
                    contentDictionary[key.Trim(',', '\'', '"', ' ', '*', '-')] = value.Trim(',', '\'', '"', ' ', '*');
                }
            }
        }
        foreach (var propAttr in GetPropertiesAttributies()) {
            foreach (var keyValue in contentDictionary) {
                if (propAttr.Value.Description.Trim('\'').StartsWith(keyValue.Key)) {
                    propAttr.Key.SetValue(mappedLoad, keyValue.Value);
                    contentDictionary.Remove(keyValue.Key);
                    break;
                }
            }
        }
        return mappedLoad;
    }

    private RequestTextData ConvertToRequestTextData(string text) {
        var requestData = new RequestTextData() {
            model = AppSettings.OpenAiModel,
            temperature = AppSettings.OpenAiTemperature,
            messages = new MessageTextData[] {
                new MessageTextData() { role = "user", content = $"Extract fields from carrier rate confirmation in JSON format: {String.Join(", ", GetFields())}. Fill '-' if empty" },
                new MessageTextData() { role = "user", content = text },
            }
        };
        return requestData;
    }

    private RequestImageData ConvertToRequestImageData(List<string> urlFiles) {
        var requestData = new RequestImageData() {
            model = AppSettings.OpenAiModel,
            temperature = AppSettings.OpenAiTemperature,
            messages = new MessageImageData[] {
                new MessageImageData() {
                    role = "user",
                    content = urlFiles.Select(e => new MessageImageContent() {
                        type = "image_url",
                        image_url = new MessageImageImage() { url = e }
                    }).Union(new MessageImageContent[] {
                        new MessageImageContent() {
                            type = "text",
                            text = $"Extract fields from carrier rate confirmation in JSON format: {String.Join(", ", GetFields())}. Fill '-' if empty" }
                    }).ToArray()
                },
            }
        };
        return requestData;
    }

    private string ConvertToString(MappedLoad mappedLoad) {
        var properties = new Dictionary<string, string>();
        foreach (var propAttr in GetPropertiesAttributies()) {
            properties[propAttr.Key.Name] = (string)propAttr.Key.GetValue(mappedLoad);
        }
        return String.Join("\n", properties.Select(e => $"{e.Key}: {e.Value}"));
    }

    private List<string> GetFields() {
        var fields = new List<string>();
        foreach (var propAttr in GetPropertiesAttributies()) {
            fields.Add(propAttr.Value.Description);
        }
        return fields;
    }

    private static Dictionary<PropertyInfo, DescriptionAttribute> GetPropertiesAttributies() {
        var result = new Dictionary<PropertyInfo, DescriptionAttribute>();
        foreach (var property in typeof(MappedLoad).GetProperties()) {
            var attr = (DescriptionAttribute)property.GetCustomAttributes(false).FirstOrDefault(e => e is DescriptionAttribute);
            if (attr != null) {
                result.Add(property, attr);
            }
        }
        return result;
    }

    #endregion [ Parse ]
}

internal class MappedLoad {
    [Description("Load #")]
    public string LoadId { get; set; }
    [Description("Broker MC Number")]
    public string BrokerMc { get; set; }
    [Description("Origin Street")]
    public string OriginStreet { get; set; }
    [Description("'Origin' format: City, ST")]
    public string Origin { get; set; }
    [Description("Destination Street")]
    public string DestinationStreet { get; set; }
    [Description("'Destination' format: City, ST")]
    public string Destination { get; set; }
    [Description("'Pickup Date From' format: mm/dd/yyyy hh:mm")]
    public string PickupDateFrom { get; set; }
    [Description("'Pickup Date To' format: mm/dd/yyyy hh:mm")]
    public string PickupDateTo { get; set; }
    [Description("'Delivery Date From' format: mm/dd/yyyy hh:mm")]
    public string DeliveryDateFrom { get; set; }
    [Description("'Delivery Date To' format: mm/dd/yyyy hh:mm")]
    public string DeliveryDateTo { get; set; }
    [Description("Shipper Name")]
    public string ShipperName { get; set; }
    [Description("Receiver Name")]
    public string ReceiverName { get; set; }
    [Description("Consignee Name")]
    public string ConsigneeName { get; set; }
    [Description("'Shipper Phone Number' format: (xxx) xxx-xxxx")]
    public string ShipperPhone { get; set; }
    [Description("'Receiver Phone Number' format: (xxx) xxx-xxxx")]
    public string ReceiverPhone { get; set; }
    [Description("PU #")]
    public string PickupId { get; set; }
    [Description("Del #")]
    public string DeliveryId { get; set; }
    [Description("Pickup #")]
    public string PickupId2 { get; set; }
    [Description("Dellivery #")]
    public string DeliveryId2 { get; set; }
    [Description("Pickup PO#")]
    public string PickupId3 { get; set; }
    [Description("Delivery PO#")]
    public string DeliveryId3 { get; set; }
    [Description("Carrier Company Name")]
    public string CarrierName { get; set; }
    [Description("Broker Company Name")]
    public string BrokerName { get; set; }
    [Description("Broker Contact Name")]
    public string BrokerContactName { get; set; }
    [Description("'Broker Phone Number' format: (xxx) xxx-xxxx")]
    public string BrokerContactPhone { get; set; }
    [Description("Price")]
    public string Price { get; set; }
    [Description("Length")]
    public string Length { get; set; }
    [Description("EQ Size")]
    public string Length2 { get; set; }
    [Description("Weight")]
    public string Weight { get; set; }
    [Description("Wgt")]
    public string Weight2 { get; set; }
    [Description("Pallets")]
    public string Pallets { get; set; }
    [Description("Pieces")]
    public string Pieces { get; set; }
    [Description("Miles")]
    public string Miles { get; set; }
    [Description("'Equipment' and convert to 'Van' or 'Refeer'")]
    public string Equipment { get; set; }
}
