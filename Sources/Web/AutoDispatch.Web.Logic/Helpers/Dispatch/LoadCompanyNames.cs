using System.Collections.Concurrent;
using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Имена и контакты компаний груза (Shipper/Receiver/DatBroker/Customer) в LoadDto с кэшем на процесс
///     (V1 AutoCallDispatchHelper.IncludeCompaniesAsync): навигации LinqToDB не грузит </summary>
public static class LoadCompanyNames {

    private record CompanyInfo(string Name, string Contacts, int? McNumber);

    private static readonly ConcurrentDictionary<Guid, CompanyInfo> Companies = new ConcurrentDictionary<Guid, CompanyInfo>();
    private static readonly ConcurrentDictionary<Guid, CompanyInfo> Customers = new ConcurrentDictionary<Guid, CompanyInfo>();
    private static readonly ConcurrentDictionary<Guid, CompanyInfo> DatBrokers = new ConcurrentDictionary<Guid, CompanyInfo>();

    public static async Task<List<LoadDto>> IncludeCompaniesAsync(this List<LoadDto> loads, LinqToDB.Data.DataConnection db) {
        // Shipper / Receiver
        {
            var ids = loads.Where(e => e.ShipperId != null && Companies.ContainsKey(e.ShipperId.Value) == false).Select(e => e.ShipperId.Value)
                .Concat(loads.Where(e => e.ReceiverId != null && Companies.ContainsKey(e.ReceiverId.Value) == false).Select(e => e.ReceiverId.Value))
                .Distinct().ToList();
            if (ids.Any()) {
                var models = await db.GetTable<ShipperReceiverCompany>().Where(e => ids.Contains(e.EntityId)).ToListAsync();
                foreach (var model in models) {
                    Companies[model.EntityId] = new CompanyInfo(model.Name, model.Contacts, null);
                }
            }
            foreach (var load in loads) {
                if (load.ShipperId != null && Companies.TryGetValue(load.ShipperId.Value, out var shipper)) {
                    load.ShipperCompany = shipper.Name;
                    load.ShipperContacts = shipper.Contacts;
                }
                if (load.ReceiverId != null && Companies.TryGetValue(load.ReceiverId.Value, out var receiver)) {
                    load.ReceiverCompany = receiver.Name;
                    load.ReceiverContacts = receiver.Contacts;
                }
            }
        }
        // DatBroker
        {
            var ids = loads.Where(e => e.DatBrokerId != null && DatBrokers.ContainsKey(e.DatBrokerId.Value) == false).Select(e => e.DatBrokerId.Value).Distinct().ToList();
            if (ids.Any()) {
                var models = await db.GetTable<DatCompany>().Where(e => ids.Contains(e.EntityId)).ToListAsync();
                foreach (var model in models) {
                    DatBrokers[model.EntityId] = new CompanyInfo(model.Name, model.Contacts, model.McNumber);
                }
            }
            foreach (var load in loads) {
                if (load.DatBrokerId != null && DatBrokers.TryGetValue(load.DatBrokerId.Value, out var broker)) {
                    load.DatBrokerCompany = broker.Name;
                }
            }
        }
        // Customer
        {
            var ids = loads.Where(e => e.CustomerId != null && Customers.ContainsKey(e.CustomerId.Value) == false).Select(e => e.CustomerId.Value).Distinct().ToList();
            if (ids.Any()) {
                var models = await db.GetTable<CustomerCompany>().Where(e => ids.Contains(e.EntityId)).ToListAsync();
                foreach (var model in models) {
                    Customers[model.EntityId] = new CompanyInfo(model.Name, model.Contacts, null);
                }
            }
            foreach (var load in loads) {
                if (load.CustomerId != null && Customers.TryGetValue(load.CustomerId.Value, out var customer)) {
                    load.CustomerCompany = customer.Name;
                }
            }
        }
        return loads;
    }

    public static async Task IncludeCompaniesAsync(this LoadDto load, LinqToDB.Data.DataConnection db) {
        await IncludeCompaniesAsync(new List<LoadDto> { load }, db);
    }

    public static void RemoveCustomer(Guid key) => Customers.TryRemove(key, out _);
    public static void RemoveDatBroker(Guid key) => DatBrokers.TryRemove(key, out _);
    public static void RemoveCompany(Guid key) => Companies.TryRemove(key, out _);
}
