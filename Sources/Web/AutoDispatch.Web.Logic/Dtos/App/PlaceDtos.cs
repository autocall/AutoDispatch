namespace AutoDispatch.Web.Logic.Dtos.App;

/// <summary>
///     Подсказка автокомплита без координат; координаты — вторым запросом по PlaceId </summary>
public class PlaceDto {
    public string PlaceId { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
    /// <summary>
    ///     Полная строка Google, например "Dallas, TX, USA" </summary>
    public string Display { get; set; }
}

public class PlaceDetailsDto {
    public string PlaceId { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}
