using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    private class QueryLoadRow {
        public string City { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Contacts { get; set; }
        public string Company { get; set; }
        public Guid CompanyId { get; set; }
    }

    /// <summary>
    ///     Автокомплит «город / компания» в окне груза (V1 CacheService.SearchLoadsAsync, тип Both) без таблицы CacheLoadIndex
    ///     (решено 2026-11-16): origin + shipper и destination + receiver грузов, где отправитель/получатель — своей компании,
    ///     `like 'текст%'` по ключу словаря (City или Company), distinct, по имени компании, top 5. V1 резал текст до 16 символов — здесь нет </summary>
    public async Task<List<QueryLoadDto>> SearchLoadsAsync(Guid userId, Dictionary<string, string> texts) {
        var companyId = await GetCompanyIdByUserIdAsync(userId);
        var origins =
            from l in Alive<Load>()
            join c in Alive<ShipperReceiverCompany>() on l.ShipperId equals (Guid?)c.EntityId
            where c.CompanyId == companyId
            select new QueryLoadRow {
                City = l.OriginCity, Address = l.OriginAddress, Latitude = l.OriginLatitude, Longitude = l.OriginLongitude,
                Contacts = c.Contacts, Company = c.Name, CompanyId = c.EntityId
            };
        var destinations =
            from l in Alive<Load>()
            join c in Alive<ShipperReceiverCompany>() on l.ReceiverId equals (Guid?)c.EntityId
            where c.CompanyId == companyId
            select new QueryLoadRow {
                City = l.DestinationCity, Address = l.DestinationAddress, Latitude = l.DestinationLatitude, Longitude = l.DestinationLongitude,
                Contacts = c.Contacts, Company = c.Name, CompanyId = c.EntityId
            };
        foreach (var text in texts) {
            var value = text.Value ?? "";
            switch (text.Key) {
                case "City":
                    origins = origins.Where(r => r.City.StartsWith(value));
                    destinations = destinations.Where(r => r.City.StartsWith(value));
                    break;
                case "Company":
                    origins = origins.Where(r => r.Company.StartsWith(value));
                    destinations = destinations.Where(r => r.Company.StartsWith(value));
                    break;
                default:
                    throw new ArgumentException($"SearchLoads: unknown key {text.Key}");
            }
        }
        var rows = await origins.Union(destinations).OrderBy(r => r.Company).Take(5).ToListAsync();
        return rows.Select(r => new QueryLoadDto {
            City = r.City,
            Address = r.Address,
            Location = LocationConverter.ToDto(r.Latitude, r.Longitude),
            Contacts = r.Contacts,
            Company = r.Company,
            CompanyId = r.CompanyId
        }).ToList();
    }
}
