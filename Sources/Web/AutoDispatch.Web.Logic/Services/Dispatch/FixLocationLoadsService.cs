using LinqToDB;
using LinqToDB.Async;
using Newtonsoft.Json.Linq;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Common.Helpers;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Разовая правка координат грузов по автокомплиту (V1 FixLocationLoadsService, админская операция) </summary>
public class FixLocationLoadsService : DispatchBaseService {

    public FixLocationLoadsService(ServicesHost host) : base(host) { }

    public async Task<string> FixLocationsAsync() {
        string result = String.Empty;
        var loads = await Alive<Load>().OrderBy(l => l.Index).ToListAsync();
        foreach (var load in loads) {
            var changed = false;
            result += await FixLocationAsync(load, CacheLoadIndexType.Origin, false, c => changed |= c);
            result += await FixLocationAsync(load, CacheLoadIndexType.Destination, false, c => changed |= c);
            if (changed) {
                await UpdateAsync(load);
            }
        }
        return result;
    }

    private async Task<string> FixLocationAsync(Load load, CacheLoadIndexType dir, bool withoutAddress, Action<bool> onChanged) {
        var service = Host.GetService<GeocodeService>();
        var dto = Mapper.Map<LoadDto>(load);
        LocationDto locationDto = dir == CacheLoadIndexType.Origin ? dto.OriginLocation : dto.DestinationLocation;
        var city = dir == CacheLoadIndexType.Origin ? dto.OriginCity : dto.DestinationCity;
        var address = dir == CacheLoadIndexType.Origin ? dto.OriginAddress : dto.DestinationAddress;

        var searchString = city;
        if (String.IsNullOrEmpty(address) == false && withoutAddress == false) {
            searchString = $"{address}, {city}";
        }
        searchString = searchString.Replace("Crosswell", "Croswell");

        var json = ZipExtension.DeserializeObject<string>(await service.GetPlaceAutocompleteAsync(searchString));
        var predictions = JObject.Parse(json)["predictions"] as JArray;
        if (predictions != null && predictions.Count > 0) {
            var detailsJson = ZipExtension.DeserializeObject<string>(await service.GetPlaceDetailsAsync((string)predictions.First["place_id"]));
            var details = JObject.Parse(detailsJson)["result"];
            if (details == null) {
                return $"SKIP: {searchString}<br>";
            }
            var fixLocation = new LocationDto {
                Latitude = (float)(double)details["geometry"]["location"]["lat"],
                Longitude = (float)(double)details["geometry"]["location"]["lng"]
            };
            var distance = DistanceHelper.GetDistance(locationDto.Latitude, locationDto.Longitude, fixLocation.Latitude, fixLocation.Longitude);
            if (distance > 2) {
                if (dir == CacheLoadIndexType.Origin) {
                    load.OriginLatitude = (decimal)fixLocation.Latitude;
                    load.OriginLongitude = (decimal)fixLocation.Longitude;
                } else {
                    load.DestinationLatitude = (decimal)fixLocation.Latitude;
                    load.DestinationLongitude = (decimal)fixLocation.Longitude;
                }
                onChanged(true);
            }
            return $"load.{load.Index}.{dir}, distance: {distance:F0}<br>";
        }
        if (withoutAddress == false) {
            return await FixLocationAsync(load, dir, true, onChanged);
        }
        return $"SKIP: {searchString}<br>";
    }
}
