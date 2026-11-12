using Newtonsoft.Json.Linq;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Автокомплит локаций для App через Safer (api/v2/geo/google/*, Autocomplete New), кэш на стороне Safer.
///     Замена svc/CityService WebV1 </summary>
public class PlaceService : BaseService {

    private BrokersnapshotWebService Brokersnapshot => Host.GetService<BrokersnapshotWebService>();

    public PlaceService(ServicesHost host) : base(host) { }

    /// <summary>
    ///     citiesOnly — только города (App); false — адреса и города (поиск на карте /Map) </summary>
    public async Task<List<PlaceDto>> SearchAsync(string search, bool citiesOnly = true) {
        search = (search ?? "").Trim();
        if (search.Length < AppSettings.PlaceSearchMinLength) {
            return new List<PlaceDto>();
        }
        if (search.Length > AppSettings.PlaceSearchMaxLength) {
            search = search.Substring(0, AppSettings.PlaceSearchMaxLength);
        }
        var json = await this.RequestAsync(Brokersnapshot.AutocompleteAsync(search, citiesOnly));
        var predictions = JObject.Parse(json)["Data"]?["predictions"] as JArray ?? new JArray();
        return predictions.Select(p => {
            // main_text — имя ("Dallas"), secondary_text — остальное ("TX, USA"); штат — первый двухбуквенный код.
            // terms, из которых штат брался раньше, в Autocomplete (New) нет
            var secondary = ((string)p["secondary_text"] ?? "").Split(',').Select(t => t.Trim()).ToList();
            return new PlaceDto {
                PlaceId = (string)p["place_id"],
                Name = (string)p["main_text"],
                State = secondary.FirstOrDefault(t => t.Length == 2 && t.All(Char.IsUpper)),
                Display = (string)p["description"]
            };
        }).Where(p => String.IsNullOrEmpty(p.PlaceId) == false).ToList();
    }

    /// <summary>
    ///     null — место не найдено </summary>
    public async Task<PlaceDetailsDto> GetAsync(string placeId) {
        placeId = (placeId ?? "").Trim();
        if (placeId.Length == 0 || placeId.Length > AppSettings.PlaceSearchMaxLength) {
            return null;
        }
        var json = await this.RequestAsync(Brokersnapshot.PlaceIdAsync(placeId));
        // незнакомый placeId — не ошибка, а found=false (Safer кэширует его как отрицательный)
        var data = JObject.Parse(json)["Data"];
        if (data == null || (bool?)data["found"] != true) {
            return null;
        }
        return new PlaceDetailsDto {
            PlaceId = placeId,
            Name = (string)data["city"] ?? ((string)data["formatted_address"] ?? "").Split(',')[0],
            State = (string)data["state"],
            Zip = (string)data["zip"],
            Latitude = (float?)data["lat"] ?? 0,
            Longitude = (float?)data["lng"] ?? 0
        };
    }

    #region [ private ]

    /// <summary>
    ///     Сырой JSON провайдера </summary>
    private async Task<string> RequestAsync(Task<ApiResult<string>> request) {
        var result = await request;
        if (result.Success == false) {
            throw new InfoException($"Place lookup failed: {result.ErrorMessage}");
        }
        return result.Data;
    }

    #endregion [ private ]
}
