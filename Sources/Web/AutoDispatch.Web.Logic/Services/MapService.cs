using System.Buffers.Binary;
using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Services.Dispatch;
using AutoDispatch.Web.Logic.WebServices;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Карта маршрута /Map: ссылка с зашифрованными точками (AES-256-GCM, Secrets:Map:Key), маршрут и адреса через Safer.
///     Замена Dispatch/RouteLocation WebV1 — там координаты шли в ссылке открытым текстом </summary>
public class MapService : BaseService {

    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int PointSize = 9; // kind byte + lat int32 + lng int32
    private const int CoordinateScale = 1_000_000;

    private BrokersnapshotWebService Brokersnapshot => Host.GetService<BrokersnapshotWebService>();
    private PlaceService Places => Host.GetService<PlaceService>();

    private readonly byte[] _key;

    public MapService(ServicesHost host) : base(host) {
        this._key = AppSettings.MapKey == null ? null : Convert.FromBase64String(AppSettings.MapKey);
    }

    #region [ Link ]

    /// <summary>
    ///     Абсолютный URL страницы; точек может не быть — пустая карта (кнопка Map в App) </summary>
    public string CreateLink(Guid userId, IReadOnlyList<MapPointDto> points) {
        return $"{AppSettings.Website}{AppSettings.MapPath}?r={CreateToken(userId, points)}";
    }

    /// <summary>
    ///     Токен той же ссылки с остановками, поставленными кликом (Stop); прежние остановки в ссылке заменяются </summary>
    public string CreateToken(MapLinkDto link, List<MapLatLngDto> stops) {
        stops ??= new List<MapLatLngDto>();
        if (stops.Count > AppSettings.MapMaxStops) {
            throw new InfoException($"No more than {AppSettings.MapMaxStops} stops");
        }
        var points = link.Points.Where(p => p.Kind != MapPointKindEnum.Stop)
            .Concat(stops.Select(s => new MapPointDto { Kind = MapPointKindEnum.Stop, Latitude = s.Latitude, Longitude = s.Longitude }))
            .ToList();
        return CreateToken(link.UserId, points);
    }

    /// <summary>
    ///     r = base64url(nonce + AES-GCM(userId Guid 16 байт, [kind byte, lat int32, lng int32]…) + tag), без срока действия;
    ///     координаты — целые с 6 знаками, little-endian; ~80 символов на 3 точки против ~200 у Data Protection </summary>
    private string CreateToken(Guid userId, IReadOnlyList<MapPointDto> points) {
        foreach (var point in points) {
            if (point.Kind == MapPointKindEnum.Unknown) throw new InfoException("Point kind is required");
            Validate(point.Latitude, point.Longitude);
        }
        var payload = new byte[16 + points.Count * PointSize];
        userId.TryWriteBytes(payload);
        var offset = 16;
        foreach (var point in points) {
            payload[offset] = (byte)point.Kind;
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset + 1), ToInt(point.Latitude));
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset + 5), ToInt(point.Longitude));
            offset += PointSize;
        }
        return Encrypt(payload);
    }

    /// <summary>
    ///     null — ссылка подделана или повреждена </summary>
    public MapLinkDto ParseLink(string token) {
        var payload = Decrypt(token);
        if (payload == null || payload.Length < 16 || (payload.Length - 16) % PointSize != 0) {
            return null;
        }
        var link = new MapLinkDto { UserId = new Guid(payload.AsSpan(0, 16)), Points = new List<MapPointDto>() };
        for (var offset = 16; offset < payload.Length; offset += PointSize) {
            link.Points.Add(new MapPointDto {
                Kind = (MapPointKindEnum)payload[offset],
                Latitude = (decimal)BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(offset + 1)) / CoordinateScale,
                Longitude = (decimal)BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(offset + 5)) / CoordinateScale
            });
        }
        return link;
    }

    /// <summary>
    ///     Ссылка Map/SelectLocation (V1 Dispatch/SelectLocation?latlng=): точка — место из App, null — пустая карта.
    ///     Тот же формат токена, что у /Map, с одной точкой kind Location </summary>
    public string CreateSelectLocationLink(Guid userId, MapLatLngDto point) {
        var points = new List<MapPointDto>();
        if (point != null) {
            points.Add(new MapPointDto { Kind = MapPointKindEnum.Location, Latitude = point.Latitude, Longitude = point.Longitude });
        }
        return $"{AppSettings.Website}{AppSettings.MapSelectLocationPath}?r={CreateToken(userId, points)}";
    }

    /// <summary>
    ///     Ссылка Map/OrderStops (V1 Dispatch/OrderStops?TrailerId=): payload — userId + trailerId (32 байта),
    ///     от токена /Map отличается длиной — ParseLink его не примет (16 % 9 ≠ 0), ParseOrderStopsLink не примет токен /Map </summary>
    public string CreateOrderStopsLink(Guid userId, Guid trailerId) {
        var payload = new byte[32];
        userId.TryWriteBytes(payload);
        trailerId.TryWriteBytes(payload.AsSpan(16));
        return $"{AppSettings.Website}{AppSettings.MapOrderStopsPath}?r={Encrypt(payload)}";
    }

    public MapOrderStopsLinkDto ParseOrderStopsLink(string token) {
        var payload = Decrypt(token);
        if (payload == null || payload.Length != 32) {
            return null;
        }
        return new MapOrderStopsLinkDto { UserId = new Guid(payload.AsSpan(0, 16)), TrailerId = new Guid(payload.AsSpan(16, 16)) };
    }

    /// <summary>
    ///     base64url(nonce + AES-GCM(payload) + tag), без срока действия </summary>
    private string Encrypt(byte[] payload) {
        var token = new byte[NonceSize + payload.Length + TagSize];
        RandomNumberGenerator.Fill(token.AsSpan(0, NonceSize));
        using var aes = new AesGcm(Key, TagSize);
        aes.Encrypt(token.AsSpan(0, NonceSize), payload, token.AsSpan(NonceSize, payload.Length),
            token.AsSpan(NonceSize + payload.Length, TagSize));
        return Base64Url.EncodeToString(token);
    }

    /// <summary>
    ///     null — не base64url, обрезан, подделан или выдан до смены ключа </summary>
    private byte[] Decrypt(string token) {
        if (String.IsNullOrEmpty(token)) {
            return null;
        }
        byte[] bytes;
        try {
            bytes = Base64Url.DecodeFromChars(token);
        } catch (FormatException) {
            return null;
        }
        var length = bytes.Length - NonceSize - TagSize;
        if (length < 0) {
            return null;
        }
        var payload = new byte[length];
        try {
            using var aes = new AesGcm(Key, TagSize);
            aes.Decrypt(bytes.AsSpan(0, NonceSize), bytes.AsSpan(NonceSize, length),
                bytes.AsSpan(NonceSize + length, TagSize), payload);
        } catch (CryptographicException) {
            return null;
        }
        return payload;
    }

    private byte[] Key => _key ?? throw new InvalidOperationException("Secrets:Map:Key is required for map links");

    private static int ToInt(decimal value) => (int)Math.Round(value * CoordinateScale);

    #endregion [ Link ]

    #region [ OrderStops ]

    /// <summary>
    ///     Остановки трейлера в порядке PickupOrder/DeliveryOrder — как V1 DispatchController.OrderStops: грузы Carrier-борда
    ///     компании пользователя с этим трейлером; AtPickUp даёт pickup и delivery, InTransit — только delivery </summary>
    public async Task<List<MapStopDto>> GetOrderStopsAsync(MapOrderStopsLinkDto link) {
        var dispatch = Host.GetService<DispatchService>();
        var data = await dispatch.LoadLoads(new LoadLoadsRequestData(link.UserId) { Types = new[] { DispatchBoardType.Carrier } });
        var stops = new List<MapStopDto>();
        foreach (var load in data.Loads.Where(l => l.TrailerId == link.TrailerId)) {
            if (load.Status == LoadStatus.AtPickUp && load.PickupOrder != null) {
                stops.Add(Stop(load, StopType.Pickup, load.PickupOrder.Value, load.PickupFrom, load.PickupTo,
                    load.OriginAddress, load.OriginCity, load.OriginLocation));
            }
            if ((load.Status == LoadStatus.AtPickUp || load.Status == LoadStatus.InTransit) && load.DeliveryOrder != null) {
                stops.Add(Stop(load, StopType.Delivery, load.DeliveryOrder.Value, load.DeliveryFrom, load.DeliveryTo,
                    load.DestinationAddress, load.DestinationCity, load.DestinationLocation));
            }
        }
        return stops.OrderBy(s => s.Order).ToList();
    }

    private static MapStopDto Stop(LoadDto load, StopType type, byte order, DateTime from, DateTime to,
        string address, string city, LocationDto location) {
        return new MapStopDto {
            LoadId = load.EntityId,
            Index = load.IndexString,
            Type = (byte)type,
            Order = order,
            Latitude = (decimal)(location?.Latitude ?? 0),
            Longitude = (decimal)(location?.Longitude ?? 0),
            Address = address,
            City = city,
            Arrive = DispatchStringExtension.DateTimeToAppointmentFullDateTime(from.Date, from.TimeOfDay, to.TimeOfDay),
            Background = ToHex(load.Background),
            Foreground = ToHex(load.Foreground)
        };
    }

    /// <summary>
    ///     ARGB из WPF Color (V1 Base64ToColor брал байты 1..3) → #rrggbb; нет цвета — null </summary>
    private static string ToHex(byte[] argb) {
        return argb == null || argb.Length < 4 ? null : $"#{argb[1]:x2}{argb[2]:x2}{argb[3]:x2}";
    }

    /// <summary>
    ///     Маршрут между соседними остановками в заданном порядке — отрезки Record, как GetRouteAsync по точкам ссылки </summary>
    public Task<MapRouteDto> GetStopsRouteAsync(Guid userId, List<MapLatLngDto> points) {
        points ??= new List<MapLatLngDto>();
        if (points.Count > AppSettings.MapMaxStops) {
            throw new InfoException($"No more than {AppSettings.MapMaxStops} stops");
        }
        var link = new MapLinkDto {
            UserId = userId,
            Points = points.Select(p => new MapPointDto { Kind = MapPointKindEnum.Record, Latitude = p.Latitude, Longitude = p.Longitude }).ToList()
        };
        foreach (var point in link.Points) {
            Validate(point.Latitude, point.Longitude);
        }
        return GetRouteAsync(link);
    }

    #endregion [ OrderStops ]

    #region [ Route ]

    /// <summary>
    ///     Отрезки между соседними точками ссылки, по одному запросу в Safer на отрезок (как V1), запросы параллельно.
    ///     Остановки (Stop-точки ссылки) идут в основной отрезок — первый Record, иначе единственный;
    ///     без точек — маршрут из самих остановок </summary>
    public async Task<MapRouteDto> GetRouteAsync(MapLinkDto link) {
        var segments = new List<(MapPointKindEnum Kind, List<MapLatLngDto> Points)>();
        var points = link.Points.Where(p => p.Kind != MapPointKindEnum.Stop).ToList();
        var stops = link.Points.Where(p => p.Kind == MapPointKindEnum.Stop).Select(ToLatLng).ToList();
        for (var i = 0; i + 1 < points.Count; i++) {
            var kind = points[i].Kind == MapPointKindEnum.Record && points[i + 1].Kind == MapPointKindEnum.Record
                ? MapPointKindEnum.Record : MapPointKindEnum.Pattern;
            segments.Add((kind, new List<MapLatLngDto> { ToLatLng(points[i]), ToLatLng(points[i + 1]) }));
        }
        if (segments.Count == 0) {
            if (stops.Count >= 2) {
                segments.Add((MapPointKindEnum.Record, stops.ToList()));
            }
        } else if (stops.Count > 0) {
            var mainIndex = segments.FindIndex(s => s.Kind == MapPointKindEnum.Record);
            if (mainIndex < 0 && segments.Count == 1) mainIndex = 0;
            if (mainIndex >= 0) {
                segments[mainIndex].Points.InsertRange(1, stops);
            }
        }

        // точки и остановки — в Details, длительность — суммарная по параллельным запросам Safer
        using var details = _l.Property(AppSettings.LogDetailsProperty, JsonExtension.Serialize(link.Points));
        using var timer = _l.it($"Route: user {link.UserId}, {points.Count} points, {stops.Count} stops, {segments.Count} Safer requests");
        var result = await Task.WhenAll(segments.Select(s => RequestSegmentAsync(s.Kind, s.Points)));
        return new MapRouteDto { Segments = result.ToList() };
    }

    private async Task<MapSegmentDto> RequestSegmentAsync(MapPointKindEnum kind, List<MapLatLngDto> points) {
        var query = String.Join("|", points.Select(p => $"{Format(p.Latitude)},{Format(p.Longitude)}"));
        var json = await RequestAsync(Brokersnapshot.RouteAsync(query), "Route");
        var legs = JObject.Parse(json)["Data"]?["legs"] as JArray;
        if (legs == null || legs.Count == 0) {
            throw new InfoException("Route not found");
        }
        return new MapSegmentDto {
            Kind = kind,
            Polylines = legs.Select(l => (string)l["polyline"]).Where(p => String.IsNullOrEmpty(p) == false).ToList()
        };
    }

    #endregion [ Route ]

    #region [ Places ]

    /// <summary>
    ///     Адрес точки (formatted_address Google без страны); null — не найден </summary>
    public async Task<string> GetAddressAsync(decimal lat, decimal lng) {
        Validate(lat, lng);
        var json = await RequestAsync(Brokersnapshot.LatLngAsync(lat, lng), "Address lookup");
        var data = JObject.Parse(json)["Data"];
        if (data == null || (bool?)data["found"] != true) {
            return null;
        }
        var address = (string)data["formatted_address"];
        return address == null ? null : Regex.Replace(address, @",\s*(USA|Canada)$", "");
    }

    /// <summary>
    ///     Точка для Map/SelectLocation: улица и «City, ST ZIP» раздельно (V1 GeocodeHelper.getCityAndAddress считал их из
    ///     address_components Google; Safer geo/google/latlng отдаёт address/city/state/zip готовыми). Не найдена — null </summary>
    public async Task<MapLocationDto> GetLocationAsync(decimal lat, decimal lng) {
        Validate(lat, lng);
        var json = await RequestAsync(Brokersnapshot.LatLngAsync(lat, lng), "Address lookup");
        var data = JObject.Parse(json)["Data"];
        if (data == null || (bool?)data["found"] != true) {
            return null;
        }
        var city = (string)data["city"];
        var state = (string)data["state"];
        var zip = (string)data["zip"];
        return new MapLocationDto {
            Latitude = lat,
            Longitude = lng,
            FormattedAddress = (string)data["formatted_address"],
            Address = (string)data["address"] ?? "",
            City = String.IsNullOrEmpty(city) || String.IsNullOrEmpty(state) ? "" : $"{city}, {state}{(String.IsNullOrEmpty(zip) ? "" : " " + zip)}"
        };
    }

    /// <summary>
    ///     Поиск на карте — адреса и города, в отличие от App </summary>
    public Task<List<PlaceDto>> SearchAsync(string search) => Places.SearchAsync(search, citiesOnly: false);

    public Task<PlaceDetailsDto> GetPlaceAsync(string placeId) => Places.GetAsync(placeId);

    #endregion [ Places ]

    #region [ private ]

    private static void Validate(decimal lat, decimal lng) {
        if (lat < -90 || lat > 90 || lng < -180 || lng > 180) {
            throw new InfoException("Invalid coordinates");
        }
    }

    private static string Format(decimal value) => value.ToString("0.######", CultureInfo.InvariantCulture);

    private static MapLatLngDto ToLatLng(MapPointDto point) => new() { Latitude = point.Latitude, Longitude = point.Longitude };

    private static async Task<string> RequestAsync(Task<ApiResult<string>> request, string operation) {
        var result = await request;
        if (result.Success == false) {
            // InfoException в лог не попадает — ошибки Safer (лимит, недоступность) писать здесь
            _l.w($"{operation} failed: {result.ErrorMessage}");
            throw new InfoException($"{operation} failed: {result.ErrorMessage}");
        }
        return result.Data;
    }

    #endregion [ private ]
}
