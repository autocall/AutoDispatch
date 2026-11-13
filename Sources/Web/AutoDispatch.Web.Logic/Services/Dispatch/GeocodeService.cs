using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoCall.Dtos.Dtos;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Геокодинг грузов и положений (V1 GeocodeService): обратный геокодинг, автокомплит и place details — Safer (BrokersnapshotWebService),
///     часовой пояс — keeppingin.com, расстояние — Google Distance Matrix. Таблицы Cache V1 нет: кэш в памяти процесса
///     (Safer кэширует geo на своей стороне), AI/wiki/plan.md </summary>
public class GeocodeService : BaseService {

    /// <summary>
    ///     Mininal step for lat/lng </summary>
    private const double Step = 0.0005;

    private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    private static readonly ConcurrentDictionary<string, string> _addressCache = new ConcurrentDictionary<string, string>();
    private static readonly ConcurrentDictionary<string, string> _timeZoneCache = new ConcurrentDictionary<string, string>();
    private static readonly ConcurrentDictionary<string, byte[]> _placeCache = new ConcurrentDictionary<string, byte[]>();
    private static readonly ConcurrentDictionary<string, Tuple<int, int>> _distanceCache = new ConcurrentDictionary<string, Tuple<int, int>>();

    private BrokersnapshotWebService Brokersnapshot => Host.GetService<BrokersnapshotWebService>();

    public GeocodeService(ServicesHost host) : base(host) { }

    private static string LocationKey(LocationDto location) {
        var intLat = Convert.ToInt32(Math.Round(location.Latitude / Step));
        var intLng = Convert.ToInt32(Math.Round(location.Longitude / Step));
        return $"{intLat}:{intLng}";
    }

    #region [ time zone ]

    // https://www.timetemperature.com/tzus/daylight_saving_time.shtml - дни, когда переход времени
    public async ValueTask<string> GetTimeZoneAsync(LocationDto location, DateTime timestamp) {
        var key = $"{location.Latitude},{location.Longitude},{DateTimeToInt(timestamp)}";
        if (_timeZoneCache.TryGetValue(key, out var value)) {
            return value;
        }
        var result = await RequestTimeZoneAsync(location.Latitude, location.Longitude, timestamp);
        _timeZoneCache[key] = result;
        return result;
    }

    /// <summary>
    ///     5/19/2023 - убрал преобразование даты в первый день месяца </summary>
    private static int DateTimeToInt(DateTime timestamp) {
        return (int)(timestamp - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public async Task<ApiResult<TimeZoneDto>> GetTypedTimeZoneAsync(LocationDto location, DateTime timestamp) {
        try {
            var json = await GetTimeZoneAsync(location, timestamp);
            var model = DispatchJson.Deserialize<TimeZoneResponseWebModel>(json);
            if (model.error_message != null) {
                throw new Exception(model.error_message);
            }
            return ApiResult<TimeZoneDto>.Successed(new TimeZoneDto {
                DstOffset = model.dstOffset,
                RawOffset = model.rawOffset,
                TimeZoneId = model.timeZoneId,
                TimeZoneName = model.timeZoneName
            });
        } catch (Exception exception) {
            _l.e("GetTypedTimeZone", exception);
            return ApiResult<TimeZoneDto>.Failed(exception);
        }
    }

    /// <example>
    /// { "dstOffset" : 0, "rawOffset" : -25200, "status" : "OK", "timeZoneId" : "America/Phoenix", "timeZoneName" : "Mountain Standard Time" }
    /// </example>
    private static async Task<string> RequestTimeZoneAsync(float lat, float lng, DateTime dateTime) {
        if (AppSettings.KeeppinginApiKey == null) {
            throw new Exception("Keeppingin is not configured");
        }
        var url = $"https://keeppingin.com/Api/GoogleMap/TimeZone_Get?latitude={lat.ToString(CultureInfo.InvariantCulture)}" +
                  $"&longitude={lng.ToString(CultureInfo.InvariantCulture)}&dateTime={Uri.EscapeDataString(dateTime.ToString("yyyy-MM-ddTHH:mm"))}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AppSettings.KeeppinginApiKey);
        using var response = await Http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private class TimeZoneResponseWebModel {
        public int dstOffset { get; set; }
        public int rawOffset { get; set; }
        public string timeZoneId { get; set; }
        public string timeZoneName { get; set; }
        public string status { get; set; }
        public string error_message { get; set; }
    }

    #endregion [ time zone ]

    #region [ formatted address ]

    /// <summary>
    ///     Адреса пачкой для списков объектов; found=false у Safer — адрес остаётся пустым (V1 ZERO_RESULTS) </summary>
    public async Task GetFormattedPlaceNamesAsync(List<LocationDto> locations) {
        // NOTES: locations contain duplicates
        var dic = new Dictionary<string, string>();
        foreach (var location in locations) {
            if (String.IsNullOrEmpty(location.FormattedAddress) == false) continue;
            var key = LocationKey(location);
            if (dic.TryGetValue(key, out var cached) || _addressCache.TryGetValue(key, out cached)) {
                location.FormattedAddress = cached;
                continue;
            }
            string result;
            try {
                result = await RequestFormattedPlaceNameAsync(location);
            } catch (Exception e) {
                if (e.Message.Contains("ZERO_RESULTS") == false) {
                    _l.e(e.Message);
                    continue;
                }
                result = String.Empty;
            }
            dic[key] = _addressCache[key] = location.FormattedAddress = result;
        }
    }

    public async ValueTask<string> GetFormattedPlaceNameAsync(LocationDto location) {
        var key = LocationKey(location);
        if (_addressCache.TryGetValue(key, out var value)) {
            return value;
        }
        var result = await RequestFormattedPlaceNameAsync(location);
        _addressCache[key] = result;
        return result;
    }

    /// <summary>
    ///     api/v2/geo/latlng Safer; found=false — то же исключение с ZERO_RESULTS, что у Google в V1 </summary>
    private async Task<string> RequestFormattedPlaceNameAsync(LocationDto location) {
        var result = await Brokersnapshot.LatLngAsync((decimal)location.Latitude, (decimal)location.Longitude);
        if (result.Success == false) {
            throw new Exception("Request name of place: " + result.ErrorMessage);
        }
        var data = JObject.Parse(result.Data)["Data"];
        if (data == null || (bool?)data["found"] != true) {
            throw new Exception("Request name of place: ZERO_RESULTS");
        }
        return (string)data["formatted_address"];
    }

    #endregion [ formatted address ]

    #region [ places (форма legacy Google Places для клиента) ]

    /// <summary>
    ///     Autocomplete (New) через Safer в форме legacy — {predictions:[{description, place_id, structured_formatting, types}], status}:
    ///     её читают WPF (PlaceAutocompleteJsonParser) и FixLocationLoadsService. Ответ — gzip JSON (ZipExtension), как в V1 </summary>
    public async Task<byte[]> GetPlaceAutocompleteAsync(string search) {
        var key = "Place Autocomplete " + search;
        if (_placeCache.TryGetValue(key, out var cached)) {
            return cached;
        }
        var result = await Brokersnapshot.AutocompleteAsync(search, false);
        if (result.Success == false) {
            throw new Exception("Safer geo: " + result.ErrorMessage);
        }
        var data = JObject.Parse(result.Data)["Data"] as JObject;
        var predictions = new JArray();
        foreach (var p in data?["predictions"] as JArray ?? new JArray()) {
            var mainText = (string)p["main_text"] ?? "";
            var formatting = new JObject { ["main_text"] = mainText, ["secondary_text"] = (string)p["secondary_text"] };
            var offset = mainText.IndexOf(search ?? "", StringComparison.OrdinalIgnoreCase);
            if (offset >= 0 && String.IsNullOrEmpty(search) == false) {
                formatting["main_text_matched_substrings"] = new JArray(new JObject { ["offset"] = offset, ["length"] = search.Length });
            }
            predictions.Add(new JObject {
                ["description"] = (string)p["description"],
                ["place_id"] = (string)p["place_id"],
                ["structured_formatting"] = formatting,
                ["types"] = p["types"] as JArray ?? new JArray(),
            });
        }
        var json = new JObject { ["predictions"] = predictions, ["status"] = predictions.Count > 0 ? "OK" : "ZERO_RESULTS" }.ToString(Formatting.None);
        var bytes = ZipExtension.SerializeObject(json);
        _placeCache[key] = bytes;
        return bytes;
    }

    /// <summary>
    ///     Place Details в форме legacy: result.address_components / formatted_address / geometry.location; found=false — NOT_FOUND </summary>
    public async Task<byte[]> GetPlaceDetailsAsync(string placeId) {
        var key = "Place Details " + placeId;
        if (_placeCache.TryGetValue(key, out var cached)) {
            return cached;
        }
        var result = await Brokersnapshot.PlaceIdAsync(placeId);
        if (result.Success == false) {
            throw new Exception("Safer geo: " + result.ErrorMessage);
        }
        var data = JObject.Parse(result.Data)["Data"] as JObject;
        string json;
        if (data == null || (bool?)data["found"] != true) {
            json = new JObject { ["result"] = null, ["status"] = "NOT_FOUND" }.ToString(Formatting.None);
        } else {
            var components = new JArray();
            void Add(string type, string longName, string shortName = null) {
                if (String.IsNullOrEmpty(longName)) return;
                components.Add(new JObject { ["long_name"] = longName, ["short_name"] = shortName ?? longName, ["types"] = new JArray(type) });
            }
            Add("route", (string)data["address"]);
            Add("locality", (string)data["city"]);
            Add("administrative_area_level_1", (string)data["state"]);
            Add("postal_code", (string)data["zip"]);
            Add("country", (string)data["country"]);
            var placeResult = new JObject {
                ["address_components"] = components,
                ["formatted_address"] = (string)data["formatted_address"],
                ["geometry"] = new JObject {
                    ["location"] = new JObject { ["lat"] = (double?)data["lat"] ?? 0, ["lng"] = (double?)data["lng"] ?? 0 },
                },
                ["place_id"] = (string)data["place_id"] ?? placeId,
            };
            json = new JObject { ["result"] = placeResult, ["status"] = "OK" }.ToString(Formatting.None);
        }
        var bytes = ZipExtension.SerializeObject(json);
        _placeCache[key] = bytes;
        return bytes;
    }

    #endregion [ places (форма legacy Google Places для клиента) ]

    #region [ distance ]

    /// <summary>
    ///     Google Distance Matrix: (метры, секунды) </summary>
    public async Task<Tuple<int, int>> GeDistanceAsync(float lat1, float lng1, float lat2, float lng2) {
        var key = $"{lat1},{lng1};{lat2},{lng2}";
        if (_distanceCache.TryGetValue(key, out var cached)) {
            return cached;
        }
        if (AppSettings.GoogleMapApiKey == null) {
            throw new Exception("GoogleMap:Api is not configured");
        }
        var ci = CultureInfo.InvariantCulture;
        var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins={lat1.ToString(ci)},{lng1.ToString(ci)}" +
                  $"&destinations={lat2.ToString(ci)},{lng2.ToString(ci)}&key={AppSettings.GoogleMapApiKey}";
        var json = await Http.GetStringAsync(url);
        var obj = JObject.Parse(json);
        if ((string)obj["status"] != "OK") {
            throw new Exception("Request distance: " + (string)obj["status"]);
        }
        var element = obj["rows"]?.First?["elements"]?.First;
        if ((string)element?["status"] != "OK") {
            throw new Exception("Request distance: " + (string)element?["status"]);
        }
        var result = new Tuple<int, int>((int)element["distance"]["value"], (int)element["duration"]["value"]);
        _distanceCache[key] = result;
        return result;
    }

    #endregion [ distance ]
}
