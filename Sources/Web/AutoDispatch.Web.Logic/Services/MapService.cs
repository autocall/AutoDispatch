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
using AutoDispatch.Web.Logic.WebServices;

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
    public string CreateLink(int userId, IReadOnlyList<MapPointDto> points) {
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
    ///     r = base64url(nonce + AES-GCM(userId int32, [kind byte, lat int32, lng int32]…) + tag), без срока действия;
    ///     координаты — целые с 6 знаками, little-endian; ~80 символов на 3 точки против ~200 у Data Protection </summary>
    private string CreateToken(int userId, IReadOnlyList<MapPointDto> points) {
        foreach (var point in points) {
            if (point.Kind == MapPointKindEnum.Unknown) throw new InfoException("Point kind is required");
            Validate(point.Latitude, point.Longitude);
        }
        var payload = new byte[4 + points.Count * PointSize];
        BinaryPrimitives.WriteInt32LittleEndian(payload, userId);
        var offset = 4;
        foreach (var point in points) {
            payload[offset] = (byte)point.Kind;
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset + 1), ToInt(point.Latitude));
            BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset + 5), ToInt(point.Longitude));
            offset += PointSize;
        }
        var token = new byte[NonceSize + payload.Length + TagSize];
        RandomNumberGenerator.Fill(token.AsSpan(0, NonceSize));
        using var aes = new AesGcm(Key, TagSize);
        aes.Encrypt(token.AsSpan(0, NonceSize), payload, token.AsSpan(NonceSize, payload.Length),
            token.AsSpan(NonceSize + payload.Length, TagSize));
        return Base64Url.EncodeToString(token);
    }

    /// <summary>
    ///     null — ссылка подделана или повреждена </summary>
    public MapLinkDto ParseLink(string token) {
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
        if (length < 4 || (length - 4) % PointSize != 0) {
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
        var link = new MapLinkDto { UserId = BinaryPrimitives.ReadInt32LittleEndian(payload), Points = new List<MapPointDto>() };
        for (var offset = 4; offset < payload.Length; offset += PointSize) {
            link.Points.Add(new MapPointDto {
                Kind = (MapPointKindEnum)payload[offset],
                Latitude = (decimal)BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(offset + 1)) / CoordinateScale,
                Longitude = (decimal)BinaryPrimitives.ReadInt32LittleEndian(payload.AsSpan(offset + 5)) / CoordinateScale
            });
        }
        return link;
    }

    private byte[] Key => _key ?? throw new InvalidOperationException("Secrets:Map:Key is required for map links");

    private static int ToInt(decimal value) => (int)Math.Round(value * CoordinateScale);

    #endregion [ Link ]

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
