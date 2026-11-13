using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Web.Logic.Dtos.Dispatch;

/// <summary>
///     Что нужно LoadIndexCacheService от груза </summary>
public class LoadIndexSource {
    public Guid LoadId { get; set; }
    public string OriginCity { get; set; }
    public string DestinationCity { get; set; }
    public string ShipperCompany { get; set; }
    public string ReceiverCompany { get; set; }

    public static LoadIndexSource From(LoadDto dto) => new LoadIndexSource {
        LoadId = dto.EntityId,
        OriginCity = dto.OriginCity,
        DestinationCity = dto.DestinationCity,
        ShipperCompany = dto.ShipperCompany,
        ReceiverCompany = dto.ReceiverCompany
    };
}
