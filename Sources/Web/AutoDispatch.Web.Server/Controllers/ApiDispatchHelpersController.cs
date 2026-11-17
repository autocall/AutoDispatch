using Microsoft.AspNetCore.Mvc;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Helpers.Dispatch;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.Controllers;

/// <summary>
///     Вспомогательные запросы окна груза App (V1 Dispatch/SearchLoads|SearchCustomers|GetPlaceAutocomplete|GetPlaceDetails|GetDistance|
///     ExtractLoadFromPdf). Маршруты V3 (решено 2026-11-17): ответ — данные как есть (JsonSuccess), ошибка — ExceptionHandler 400 {error} </summary>
[Route("Api/Dispatch/Helpers/[action]")]
public class ApiDispatchHelpersController : ApiBaseController {

    private DispatchService DispatchService => Host.GetService<DispatchService>();
    private CustomerCompanyService CustomerCompanies => Host.GetService<CustomerCompanyService>();
    private GeocodeService Geocode => Host.GetService<GeocodeService>();
    private OpenAIService OpenAI => Host.GetService<OpenAIService>();
    private ConfirmationService Confirmations => Host.GetService<ConfirmationService>();

    public ApiDispatchHelpersController(ServicesHost host) : base(host) { }

    /// <summary>
    ///     Автокомплит «город / компания»: {"City": "Dal"} или {"Company": "Wal"} → QueryLoadDto[] (top 5) </summary>
    [HttpPost]
    public async Task<ActionResult> SearchLoads([FromBody] Dictionary<string, string> searchTexts) {
        return JsonSuccess(await DispatchService.SearchLoadsAsync(GetUserId(), searchTexts ?? new Dictionary<string, string>()));
    }

    /// <summary>
    ///     Заказчики своей компании по части имени → CustomerCompanyDto[] </summary>
    [HttpPost]
    public async Task<ActionResult> SearchCustomers([FromBody] string text) {
        return JsonSuccess(await CustomerCompanies.SearchCustomersAsync(GetUserId(), text ?? ""));
    }

    /// <summary>
    ///     Подсказки адреса в форме legacy Google Places {predictions, status} (GeocodeService через Safer); V1 отдавал gzip — здесь JSON </summary>
    [HttpGet]
    public async Task<ActionResult> PlaceAutocomplete(string search) {
        return Content(ZipExtension.DeserializeObject<string>(await Geocode.GetPlaceAutocompleteAsync(search)), "application/json");
    }

    /// <summary>
    ///     Детали места {result, status} по place_id из PlaceAutocomplete </summary>
    [HttpGet]
    public async Task<ActionResult> PlaceDetails(string placeId) {
        return Content(ZipExtension.DeserializeObject<string>(await Geocode.GetPlaceDetailsAsync(placeId)), "application/json");
    }

    /// <summary>
    ///     Google Distance Matrix: {distance} метры, {duration} секунды </summary>
    [HttpGet]
    public async Task<ActionResult> Distance(float lat1, float lng1, float lat2, float lng2) {
        var result = await Geocode.GeDistanceAsync(lat1, lng1, lat2, lng2);
        return JsonSuccess(new { Distance = result.Item1, Duration = result.Item2 });
    }

    /// <summary>
    ///     Разбор rate confirmation через OpenAI: PDF файлом (multipart "file") → LoadDto </summary>
    [HttpPost, DisableRequestSizeLimit]
    public async Task<ActionResult> ExtractLoadFromPdf(IFormFile file, [FromQuery] DispatchBoardType board) {
        if (file == null || file.Length == 0) {
            throw new InfoException("File is empty");
        }
        await using var stream = file.OpenReadStream();
        return JsonSuccess(await ExtractAsync(stream, board));
    }

    /// <summary>
    ///     То же по сохранённому confirmation груза </summary>
    [HttpGet]
    public async Task<ActionResult> ExtractLoadFromPdf2(Guid loadId, DispatchBoardType board) {
        var load = await Confirmations.GetLoadByIdAsync(loadId);
        if (load == null || String.IsNullOrEmpty(load.ConfirmationFile)) {
            throw new InfoException("Load has no confirmation file");
        }
        await using var stream = new FileStream(DispatchFileHelper.FileGetFullPath(load.EntityId, load.ConfirmationFile), FileMode.Open, FileAccess.Read);
        return JsonSuccess(await ExtractAsync(stream, board));
    }

    /// <summary>
    ///     Страницы PDF → PNG в TempPath, OpenAI получает их по URL {Website}/Temp/{name} (V1 https://{Domain}/Temp/) </summary>
    private async Task<AutoDispatch.Dtos.Dtos.LoadDto> ExtractAsync(Stream pdf, DispatchBoardType board) {
        var companyId = GetCompanyId();
        var images = await OpenAI.PdfToPngAsync(pdf);
        var names = await OpenAI.SaveImagesToTempAsync(images);
        var urls = names.Select(name => $"{AppSettings.Website}{AppSettings.TempRequestPath}/{name}").ToList();
        return await OpenAI.RequestAsync(urls, board, companyId);
    }
}
